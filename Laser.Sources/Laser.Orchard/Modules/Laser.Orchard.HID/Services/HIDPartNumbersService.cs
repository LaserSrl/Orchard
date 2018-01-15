using Laser.Orchard.HID.Extensions;
using Laser.Orchard.HID.Models;
using Laser.Orchard.HID.ViewModels;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Laser.Orchard.HID.Services {
    public class HIDPartNumbersService : IHIDPartNumbersService {

        private readonly IHIDAdminService _HIDAdminService;
        private readonly IRepository<HIDPartNumberSet> _HIDPartNumberSetRepository;
        private readonly IContentManager _contentManager;
        private readonly IRepository<PartNumberSetUserPartJunctionRecord> _setUserJunctionRecordRepository;
        private readonly IHIDCredentialsService _HIDCredentialsService;

        public HIDPartNumbersService(
            IHIDAdminService HIDAdminService,
            IRepository<HIDPartNumberSet> HIDPartNumberSetrepository,
            IContentManager contentManager,
            IRepository<PartNumberSetUserPartJunctionRecord> setUserJunctionRecordRepository,
            IHIDCredentialsService HIDCredentialsService) {

            _HIDAdminService = HIDAdminService;
            _HIDPartNumberSetRepository = HIDPartNumberSetrepository;
            _contentManager = contentManager;
            _setUserJunctionRecordRepository = setUserJunctionRecordRepository;
            _HIDCredentialsService = HIDCredentialsService;

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

            var message = string.Empty;

            // Get all Part Numbers from HID to attempt validation. 
            var allowedPartNumbers = HIDPartNumbers();
            if (allowedPartNumbers != null) {

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
            } else {
                message = T("It was impossible to contact HID to validate the Part Numbers.").Text;
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
            
            return PartNumberValidationResult.SuccessResult(message);
        }

        private void ExecuteUpdates(List<SetUpdate> updates) {
            var context = new BulkCredentialsOperationsContext(true); // prioritize issueing credentials over revoking them
            foreach (var update in updates) {
                if (update.OldSet != null) {
                    if (update.Delete) {
                        var oldJunctions = _setUserJunctionRecordRepository
                            .Fetch(jr => jr.HIDPartNumberSet.Id == update.OldSet.Id);
                        // get all affected users
                        var users = oldJunctions
                            .Select(jr => _contentManager.Get<UserPart>(jr.PartNumberSetsUserPartRecord.Id))
                            .ToList();
                        foreach (var junction in oldJunctions) {
                            // Remove all Junction records
                            _setUserJunctionRecordRepository.Delete(junction);
                        }
                        // Get the part numbers from the set we are going to remove
                        var partNumbersToRevoke = Helpers.NumbersStringToArray(update.OldSet.StoredPartNumbers);
                        // delete the old set
                        _HIDPartNumberSetRepository.Delete(update.OldSet);
                        // Revoke all corresponding credentials (unless the users have credentials for
                        // the same part numbers from other sets)
                        if (partNumbersToRevoke.Any() && users.Any()) {
                            foreach (var user in users) {
                                context.AddRevokeAction(user, partNumbersToRevoke.Except(PartNumbersRightNow(user)));
                            }
                        }
                    } else {
                        if (update.NewSet != null) { // should alway be true
                            // update the old set using data from the new set
                            // We start from the OldSet and change it, otherwise there is an issue with
                            // pre-existing relations in the db 
                            var oldPartNumbers = Helpers.NumbersStringToArray(update.OldSet.StoredPartNumbers);
                            var newPartNumbers = Helpers.NumbersStringToArray(update.NewSet.StoredPartNumbers);

                            var oldToIssue = update.OldSet.IssueCredentialsAutomatically;

                            // Do the update
                            var updatedSet = update.OldSet;
                            update.NewSet.CopyProperties(updatedSet);

                            _HIDPartNumberSetRepository.Update(updatedSet);
                            // If the part numbers have changed, we need to issue/revoke credentials accordingly
                            // Get all the affected users
                            var users = _setUserJunctionRecordRepository
                                .Fetch(jr => jr.HIDPartNumberSet.Id == update.OldSet.Id)
                                .Select(jr => _contentManager.Get<UserPart>(jr.PartNumberSetsUserPartRecord.Id));
                            if (users.Any()) {
                                if (update.NewSet.IssueCredentialsAutomatically) {
                                    // We need to issue credentials for all new part numbers
                                    // if we had not issued earlier, we may need to do it now
                                    var toIssue = oldToIssue
                                        ? newPartNumbers.Except(oldPartNumbers)
                                        : newPartNumbers;
                                    if (toIssue.Any()) {
                                        foreach (var user in users) {
                                            context.AddIssueAction(user, toIssue);
                                        }
                                    }
                                }
                                // We need to revoke credentials for all removed part numbers
                                var toRevoke = oldPartNumbers.Except(newPartNumbers);
                                if (toRevoke.Any()) {
                                    foreach (var user in users) {
                                        context.AddRevokeAction(user, toRevoke.Except(PartNumbersRightNow(user)));
                                    }
                                }
                            }
                        }
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
            // create task to handle all the issue/revokes
            context.ConsolidateDictionary();
            if (context.UserActions.Any()) {
                _HIDCredentialsService.ScheduleCredentialActions(context);
            }
        }

        /// <summary>
        /// This method will return null if it's not able to get the list of Part Numbers from
        /// HID's servers.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> HIDPartNumbers() {
            // First we GET from {Base_URI}/customer/{customer_id}/part-number
            // This returns a json, that looks like this:
            // {
            //  "schemas": [ "urn:ietf:params:scim:api:messages:2.0:ListResponse" ],
            //      "totalResults": 1,
            //      "itemsPerPage": 1,
            //      "startIndex": 1,
            //      "Resources": [
            //          {"meta": {
            //              "resourceType": "PartNumber",
            //              "lastModified": "2016-12-12T17:07:44Z",
            //              "location": "URI"
            //          }},
            //          {"meta": {
            //              "resourceType": "PartNumber",
            //              "lastModified": "2016-12-12T17:07:44Z",
            //              "location": "URI"
            //          }}
            //      ]
            //  }
            // On principle there could be several of those "Resources". The "URI" in their "Location" property
            // is the URI of the Part Number Resource. 
            // Calling that in GET, we obtain the details for the Part Number:
            //{
            //    "schemas": [ "urn:hid:scim:api:ma:1.0:PartNumber" ],
            //    "urn:hid:scim:api:ma:1.0:PartNumber": [
            //        {
            //            "meta": {
            //                "resourceType": "PartNumber",
            //                "lastModified": "2016-12-12T17:07:44Z",
            //                "location": "URI"
            //            },
            //            "id": "1625",
            //            "partNumber": "THIS IS THE PART NUMBER STRING",
            //            "friendlyName": "Bla bla",
            //            "description": "bla bla bla",
            //            "availableQty": 284
            //        }
            //    ]
            //}
            // In that resource we need to take the "partNumber" property.
            // This means we have to do a whole bunch of calls.
            if (!_HIDAdminService.VerifyAuthentication()) {
                return null;
            }
            return null; //TODO IMPLEMENT THIS
            // Create the first request
            HttpWebRequest wr = HttpWebRequest.CreateHttp(_HIDAdminService.BaseEndpoint + "/part-number");
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    // Process result of the first request and do the following ones.
                }
            } catch (Exception) {

                throw;
            }

            // TODO
            return new string[] { "asd", "qwe" };
            return new List<string>();
        }

        /// <summary>
        /// Use this internally while processing stuff as a shorthand call to fetching stuff from the Junction records.
        /// It is actually fetching stuff from jusnction records.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private IEnumerable<string> PartNumbersRightNow(IUser user) {
            return _setUserJunctionRecordRepository
                .Fetch(jr => jr.PartNumberSetsUserPartRecord.Id == user.Id) // junctions remaining for user
                .SelectMany(jr => Helpers.NumbersStringToArray(jr.HIDPartNumberSet.StoredPartNumbers)) // part numbers
                .Distinct();
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

        public BulkCredentialsOperationsContext UpdatePart(PartNumberSetsUserPart part, PartNumberSetsUserPartEditViewModel vm) {
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

            // Get the part numbers associated with the user as of now (before the update)
            var oldPartNumbers = oldSets
                .SelectMany(jr => Helpers.NumbersStringToArray(jr.HIDPartNumberSet.StoredPartNumbers))
                .Distinct();
            // get the part numbers that the user will have, to prevent revoking them in case
            // some of them are also configured to PartNumberSets we will remove
            var newPartNumbers = newSetsLookup.Values.ToList()
                .SelectMany(pns => pns.PartNumbers)
                .Distinct();
            var toRevoke = oldPartNumbers.Except(newPartNumbers);

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
                // Add the set to the user, but don't issue credentials yet
                AddSetToUser(item.Value.Set, part, false);
            }

            // We only need to issue credentials for part numbers that are in new sets
            // that have their IssueCredentialsAutomatically flag set
            var toIssue = newSetsLookup.Values
                .Where(pns => pns.IssueCredentialsAutomatically)
                .SelectMany(pns => pns.PartNumbers)
                .Distinct();
            
            var context = new BulkCredentialsOperationsContext(true);

            if (toRevoke.Any() || toIssue.Any()) {
                var user = part.As<UserPart>();
                context.AddIssueAction(user, toIssue);
                context.AddRevokeAction(user, toRevoke);

                _HIDCredentialsService.ProcessUserCredentialActions(context);
            }
            return context;
        }

        public BulkCredentialsOperationsContext AddSetToUser(HIDPartNumberSet pnSet, IUser user) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }
            if (pnSet == null) {
                throw new ArgumentNullException("pnSet");
            }
            var part = user.As<PartNumberSetsUserPart>();
            if (part == null) {
                throw new ArgumentException(T("User must have PartNumberSetUserPart.").Text);
            }
            return AddSetToUser(pnSet, part);
        }

        public BulkCredentialsOperationsContext AddSetToUser(HIDPartNumberSet pnSet, PartNumberSetsUserPart part) {
            if (part == null) {
                throw new ArgumentNullException("part");
            }
            if (pnSet == null) {
                throw new ArgumentNullException("pnSet");
            }

            return AddSetToUser(pnSet, part, pnSet.IssueCredentialsAutomatically);
        }

        private BulkCredentialsOperationsContext AddSetToUser(HIDPartNumberSet pnSet, PartNumberSetsUserPart part, bool issueCredentials) {
            var record = part.Record;
            // fetch old part number sets (actually, the junction records)
            var oldSets = _setUserJunctionRecordRepository
                .Fetch(os => os.PartNumberSetsUserPartRecord == record)
                .ToList();

            var context = new BulkCredentialsOperationsContext(true);

            // if the user is already associated with the set there is nothing to do here
            if (!oldSets.Any(os => os.HIDPartNumberSet.Id == pnSet.Id)) {
                // the user was not yet associated with the set
                // actually add the set, by creating the corresponding junction record
                _setUserJunctionRecordRepository.Create(new PartNumberSetUserPartJunctionRecord {
                    HIDPartNumberSet = pnSet,
                    PartNumberSetsUserPartRecord = record
                });

                // Issue credentials if we have to
                if (issueCredentials) {
                    var toIssue = Helpers.NumbersStringToArray(pnSet.StoredPartNumbers);
                    context.AddIssueAction(part.As<UserPart>(), toIssue);
                    _HIDCredentialsService.ProcessUserCredentialActions(context);
                }
            }

            return context;
        }


        public BulkCredentialsOperationsContext RemoveSetFromUser(HIDPartNumberSet pnSet, IUser user) {
            if (user == null) {
                throw new ArgumentNullException("user");
            }
            if (pnSet == null) {
                throw new ArgumentNullException("pnSet");
            }
            var part = user.As<PartNumberSetsUserPart>();
            if (part == null) {
                throw new ArgumentException(T("User must have PartNumberSetUserPart.").Text);
            }

            return RemoveSetFromUser(pnSet, part);
        }

        public BulkCredentialsOperationsContext RemoveSetFromUser(HIDPartNumberSet pnSet, PartNumberSetsUserPart part) {
            if (part == null) {
                throw new ArgumentNullException("part");
            }
            if (pnSet == null) {
                throw new ArgumentNullException("pnSet");
            }

            var record = part.Record;
            // fetch old part number sets (actually, the junction records)
            var oldSets = _setUserJunctionRecordRepository
                .Fetch(os => os.PartNumberSetsUserPartRecord == record)
                .ToList();

            var context = new BulkCredentialsOperationsContext(true);

            // If the user is not associated with the set, there is nothing to do
            var old = oldSets.FirstOrDefault(os => os.HIDPartNumberSet.Id == pnSet.Id);
            if (old != null) {
                // The user is associated with the set
                // Find the part numbers for which we will revoke credentials: these are the part numbers
                // for the set we are removing, except those that are shared with some sets that will 
                // remain associated with the user
                var toRevoke = Helpers.NumbersStringToArray(pnSet.StoredPartNumbers)
                    .Except(oldSets.Where(os => os.HIDPartNumberSet.Id != pnSet.Id)
                        .SelectMany(os => Helpers
                            .NumbersStringToArray(os.HIDPartNumberSet.StoredPartNumbers)));
                // remove association
                _setUserJunctionRecordRepository.Delete(old);

                context.AddRevokeAction(part.As<UserPart>(), toRevoke);
                _HIDCredentialsService.ProcessUserCredentialActions(context);
            }

            return context;
        }

        class SetUpdate {
            public HIDPartNumberSet OldSet { get; set; }
            public HIDPartNumberSet NewSet { get; set; }

            public bool Delete { get; set; }
            
        }
    }
}