using Laser.Orchard.HID.Models;
using Orchard.Data;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Security;
using Laser.Orchard.HID.ViewModels;


namespace Laser.Orchard.HID.Services {
    public class HIDPartNumbersService : IHIDPartNumbersService {

        private readonly IHIDAdminService _HIDAdminService;
        private readonly IRepository<HIDPartNumberSet> _repository;

        public HIDPartNumbersService(
            IHIDAdminService HIDAdminService,
            IRepository<HIDPartNumberSet> repository) {

            _HIDAdminService = HIDAdminService;
            _repository = repository;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private string[] _partNumbers;
        public string[] PartNumbers {
            get {
                if (_partNumbers == null) {
                    _partNumbers = _repository
                        .Table
                        .Select(pns => new HIDPartNumberSetViewModel(pns))
                        .SelectMany(vm => vm.PartNumbers)
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
            var oldSets = _repository.Table.ToList();

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
                        _repository.Delete(update.OldSet);
                        // Revoke all corresponding credentials
                    } else {
                        // update the old set using data from the new set
                        var updatedSet = update.NewSet;
                        updatedSet.Id = update.OldSet.Id;
                        _repository.Update(updatedSet);
                        // if the part numbers have changed, we need to issue/revoke credentials accordingly
                    }
                } else {
                    // we need to add the new set
                    if (update.NewSet != null) {
                        _repository.Create(update.NewSet);
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
            // TODO

            return PartNumbers;
        }

        public string[] GetPartNumbersForUser(HIDUser hidUser) {
            // TODO: use the HIDUser.Emails.First

            return PartNumbers;
        }

        class SetUpdate {
            public HIDPartNumberSet OldSet { get; set; }
            public HIDPartNumberSet NewSet { get; set; }

            public bool Delete { get; set; }
            
        }
    }
}