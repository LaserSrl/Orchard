using Laser.Orchard.HID.Models;
using Orchard.Data;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Security;

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
                        .ToList()
                        .SelectMany(pns => pns.PartNumbers)
                        .ToArray();
                }
                return _partNumbers;
            }
            private set { _partNumbers = value; } // set to null to force refetch on next get
        }

        public PartNumberValidationResult TryUpdatePartNumbers(HIDSiteSettingsPart settings) {
            
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
                var changedSets = oldSets
                    .Select(os => {
                        var newSet = settings
                            .PartNumberSets
                            .FirstOrDefault(pns => pns.Id == os.Id);
                        return new SetUpdate {
                            OldSet = os,
                            NewSet = newSet,
                            Delete = newSet.Delete
                        };
                    });
            }

            return PartNumberValidationResult.SuccessResult();
        }

        private IEnumerable<string> HIDPartNumbers() {
            // TODO
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