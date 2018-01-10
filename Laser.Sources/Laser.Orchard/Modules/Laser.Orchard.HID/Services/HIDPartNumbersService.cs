using Laser.Orchard.HID.Models;
using Orchard.Data;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Security;
using Laser.Orchard.HID.ViewModels;
using Orchard.ContentManagement;
using Orchard.Users.Models;

namespace Laser.Orchard.HID.Services {
    public class HIDPartNumbersService : IHIDPartNumbersService {

        private readonly IHIDAdminService _HIDAdminService;
        private readonly IRepository<HIDPartNumberSet> _HIDPartNumberSetRepository;
        private readonly IContentManager _contentManager;
        private readonly IRepository<PartNumberSetUserPartJunctionRecord> _setUserJunctionRecordRepository;

        public HIDPartNumbersService(
            IHIDAdminService HIDAdminService,
            IRepository<HIDPartNumberSet> HIDPartNumberSetrepository,
            IContentManager contentManager,
            IRepository<PartNumberSetUserPartJunctionRecord> setUserJunctionRecordRepository) {

            _HIDAdminService = HIDAdminService;
            _HIDPartNumberSetRepository = HIDPartNumberSetrepository;
            _contentManager = contentManager;
            _setUserJunctionRecordRepository = setUserJunctionRecordRepository;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private string[] _partNumbers;
        public string[] PartNumbers {
            get {
                if (_partNumbers == null) {
                    _partNumbers = _HIDPartNumberSetRepository
                        .Table
                        .Select(pns => new HIDPartNumberSetViewModel(pns))
                        .SelectMany(vm => vm.PartNumbers)
                        .Distinct()
                        .ToArray();
                }
                return _partNumbers;
            }
            private set { _partNumbers = value; } // set to null to force refetch on next get
        }

        public PartNumberValidationResult TryUpdatePartNumbers(HIDSiteSettingsViewModel settings) {

            // Get all Part Numbers from HID
            var allowedPartNumbers = HIDPartNumbers();

            // See whether any of the part numbers we are trying to configure is not allowed
            // (i.e. is not among the numbers returned by hid)
            var badNumbers = settings.PartNumberSets
                    .Where(pns => !pns.Delete)
                    .SelectMany(pns => pns.PartNumbers.ToList())
                    .Except(allowedPartNumbers);
            if (badNumbers.Any()) {
                return new PartNumberValidationResult {
                    Success = false,
                    Message = T("Some of the PartNumbers are not allowed from HID: {0}",
                        string.Join(", ", badNumbers)).Text,
                    Error = PartNumberError.PartNumbersNotValid
                };
            }

            // Get the old HIDPartNumberSets from the records
            var oldSets = _HIDPartNumberSetRepository.Table.ToList();

            var todo = new List<SetUpdate>(); //list of the changes
            // Check whether any set has changed
            if (oldSets.Any()) {
                todo.AddRange(oldSets
                    .Select(os => {
                        var newSet = settings
                            .PartNumberSets
                            .FirstOrDefault(pns => pns.Set.Id == os.Id); // should never be null, unless data has been tampered with
                        if (newSet == null || newSet.Set == null) {
                            return new SetUpdate {
                                OldSet = os,
                                NewSet = null, 
                                Delete = true
                            };
                        }
                        return new SetUpdate {
                            OldSet = os,
                            NewSet = newSet.Set,
                            Delete = newSet.Delete
                        };
                    }));
            }
            // Add new sets
            todo.AddRange(settings.PartNumberSets
                .Where(pns => pns.Set.Id == 0 && !pns.Delete)
                .Select(pns => {
                    return new SetUpdate {
                        OldSet = null,
                        NewSet = pns.Set,
                        Delete = false
                    };
                }));

            try {
                ExecuteUpdates(todo);
            } catch (Exception) {
                return new PartNumberValidationResult {
                    Success = false,
                    Message = T("Unknown Error while updating HID part Number Sets.").Text,
                    Error = PartNumberError.UnknownError
                };
            }
            
            return PartNumberValidationResult.SuccessResult();
        }

        private void ExecuteUpdates(List<SetUpdate> updates) {
            foreach (var update in updates) {
                if (update.OldSet != null) {
                    if (update.Delete) {
                        // delete the old set
                        _HIDPartNumberSetRepository.Delete(update.OldSet);
                        // Remove all Junction records
                        // Revoke all corresponding credentials (unless the users have credentials for
                        // the same part numbers from other sets)
                    } else {
                        // update the old set using data from the new set
                        // We start from the OldSetand change it, otherwise there is an issue with
                        // pre-existing relations in the db 
                        var updatedSet = update.OldSet;
                        updatedSet.Name = update.NewSet.Name;
                        updatedSet.StoredPartNumbers = update.NewSet.StoredPartNumbers;
                        _HIDPartNumberSetRepository.Update(updatedSet);
                        // if the part numbers have changed, we need to issue/revoke credentials accordingly
                    }
                } else {
                    // we need to add the new set
                    if (update.NewSet != null) {
                        _HIDPartNumberSetRepository.Create(update.NewSet);
                        // since this is a new set, it cannot have any connected user yet, so we don't need to
                        // issue/revoke credentials just yet.
                    }
                }
            }
        }

        private IEnumerable<string> HIDPartNumbers() {
            // TODO
            return new string[] { "asd", "qwe" };
            return new List<string>();
        }

        public string[] GetPartNumbersForUser(IUser user) {
            return GetSets(user)
                .SelectMany(pns => pns.PartNumbers)
                .Distinct()
                .ToArray();
        }

        public string[] GetPartNumbersForUser(HIDUser hidUser) {
            return GetSets(hidUser)
                .SelectMany(pns => pns.PartNumbers)
                .Distinct()
                .ToArray();
        }

        public IEnumerable<HIDPartNumberSetViewModel> GetAllSets() {
            return _HIDPartNumberSetRepository.Table
                .Select(pns => new HIDPartNumberSetViewModel(pns))
                .ToList();
        }

        public IEnumerable<HIDPartNumberSetViewModel> GetSets(IUser user) {
            if (user == null) {
                return new List<HIDPartNumberSetViewModel>();
            }
            var part = user.ContentItem.As<PartNumberSetsUserPart>();
            if (part == null) {
                return new List<HIDPartNumberSetViewModel>();
            }
            return part.PartNumberSets.Select(pns => new HIDPartNumberSetViewModel(pns));
        }

        public IEnumerable<HIDPartNumberSetViewModel> GetSets(HIDUser hidUser) {
            if (hidUser == null || hidUser.Emails == null || !hidUser.Emails.Any()) {
                return new List<HIDPartNumberSetViewModel>();
            }
            var user = _contentManager
                .Query("User")
                .Where<UserPartRecord>(x => x.Email == hidUser.Emails.First())
                .Slice(0, 1)
                .FirstOrDefault();
            if (user != null) {
                return GetSets(user.As<UserPart>());
            }
            return new List<HIDPartNumberSetViewModel>();
        }

        public void UpdatePart(PartNumberSetsUserPart part, PartNumberSetsUserPartEditViewModel vm) {
            var record = part.Record;
            // fetch old part number sets (actually, the junction records)
            var oldSets = _setUserJunctionRecordRepository
                .Fetch(os => os.PartNumberSetsUserPartRecord == record)
                .ToList();
            // see what the new part number sets are
            var newIds = vm.Sets
                .Where(entry => entry.IsSelected)
                .Select(entry => entry.Id);
            var newSetsLookup = _HIDPartNumberSetRepository
                .Fetch(pns => newIds.Contains(pns.Id))
                .ToDictionary(s => s.Id, s => new HIDPartNumberSetViewModel(s));

            // get the part numbers that the user will have, to prevent revoking them in case
            // some of them are also configured to PartNumberSets we will remove
            var oldPartNumbers = oldSets
                .SelectMany(jr => new HIDPartNumberSetViewModel(jr.HIDPartNumberSet).PartNumbers)
                .Distinct();
            var newPartNumbers = newSetsLookup.Values.ToList()
                .SelectMany(pns => pns.PartNumbers)
                .Distinct();

            var toRevoke = oldPartNumbers.Except(newPartNumbers);
            var toIssue = newPartNumbers.Except(oldPartNumbers);

            // Some sets may have been removed
            foreach (var old in oldSets) {
                var key = old.HIDPartNumberSet.Id;
                if (!newSetsLookup.ContainsKey(key)) {
                    // set removed
                    _setUserJunctionRecordRepository.Delete(old);
                } else {
                    // set was there and still is. We remove it from the lookup, so that dictionary
                    // will end up containing only the sets we have to actually add
                    newSetsLookup.Remove(key);
                }
            }
            // Some sets may have been added
            foreach (var item in newSetsLookup) {
                _setUserJunctionRecordRepository.Create(new PartNumberSetUserPartJunctionRecord {
                    HIDPartNumberSet = item.Value.Set,
                    PartNumberSetsUserPartRecord = record
                });
            }

            // TODO: issue and revoke credentials
            var user = part.As<UserPart>();
        }

        class SetUpdate {
            public HIDPartNumberSet OldSet { get; set; }
            public HIDPartNumberSet NewSet { get; set; }

            public bool Delete { get; set; }
            
        }
    }
}