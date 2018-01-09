using Laser.Orchard.HID.Extensions;
using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.HID.Models {
    /// <summary>
    /// This class is used to represent sets of PartNumbers to be associated to a single access.
    /// e.g. the idea here is that one object of this class represents a single building and all
    /// the PartNumbers associated with its points of entry.
    /// This class is both its own model and view model.
    /// </summary>
    public class HIDPartNumberSet {

        public HIDPartNumberSet() {
            PartNumbers = new List<string>();
        }

        public virtual int Id { get; set; } // Primary Key

        /// <summary>
        /// The name of this set.
        /// </summary>
        public virtual string Name { get; set; }
        
        [StringLengthMax]
        public virtual string StoredPartNumbers { get; set; }

        /// <summary>
        /// The part numbers associated with this set
        /// </summary>
        public IEnumerable<string> PartNumbers {
            get { return Helpers.NumbersStringToArray(StoredPartNumbers); }
            set { StoredPartNumbers = Helpers.NumbersArrayToString(value); }
        }

        /// <summary>
        /// Need to pass a single string for editing.
        /// </summary>
        public string SerializedPartNumbers {
            get { return (PartNumbers != null && PartNumbers.Count() > 0) 
                    ? String.Join(Environment.NewLine, PartNumbers) : ""; }
            set { PartNumbers = value
                    .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(pn => pn.Trim())
                    .ToList(); }
        }

        /// <summary>
        /// Used to mark sets that we need to remove.
        /// </summary>
        public bool Delete { get; set; }
    }
}