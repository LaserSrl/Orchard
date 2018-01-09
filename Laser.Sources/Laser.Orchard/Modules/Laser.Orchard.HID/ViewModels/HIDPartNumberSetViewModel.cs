using Laser.Orchard.HID.Extensions;
using Laser.Orchard.HID.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.ViewModels {
    public class HIDPartNumberSetViewModel {

        public HIDPartNumberSetViewModel() {
            Set = HIDPartNumberSet.DefaultEmptySet();
        }

        public HIDPartNumberSetViewModel(HIDPartNumberSet set) {
            Set = set;
        }

        public HIDPartNumberSet Set { get; set; }

        public string Name { get { return Set.Name; } }

        /// <summary>
        /// The part numbers associated with this set
        /// </summary>
        public IEnumerable<string> PartNumbers {
            get { return Helpers.NumbersStringToArray(Set.StoredPartNumbers); }
            set { Set.StoredPartNumbers = Helpers.NumbersArrayToString(value); }
        }

        /// <summary>
        /// Need to pass a single string for editing.
        /// </summary>
        public string SerializedPartNumbers {
            get {
                return (PartNumbers != null && PartNumbers.Count() > 0)
                  ? String.Join(Environment.NewLine, PartNumbers) : "";
            }
            set {
                PartNumbers = value
                  .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(pn => pn.Trim())
                  .ToList();
            }
        }

        /// <summary>
        /// Used to mark sets that we need to remove.
        /// </summary>
        public bool Delete { get; set; }

    }
}