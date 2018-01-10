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
    /// </summary>
    public class HIDPartNumberSet {

        public HIDPartNumberSet() {

            PartNumberSetsUserPartsJR = new List<PartNumberSetUserPartJunctionRecord>();
        }

        public virtual int Id { get; set; } // Primary Key

        /// <summary>
        /// The name of this set.
        /// </summary>
        public virtual string Name { get; set; }
        
        [StringLengthMax]
        public virtual string StoredPartNumbers { get; set; }

        public virtual IList<PartNumberSetUserPartJunctionRecord> PartNumberSetsUserPartsJR { get; set; }



        public static HIDPartNumberSet DefaultEmptySet() {
            return new HIDPartNumberSet {
                Name = "Default",
                StoredPartNumbers = string.Empty
            };
        }
    }
}