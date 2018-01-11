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

        /// <summary>
        /// This property controls the behaviour of the system when this HIDPartNumberSet gets updated.
        /// If it's true: when a new part number is added to this Set, credentials for it are issued to
        /// all users associated with the set; when the Set is associated to a user, credentials are issued
        /// to that user for all part numbers in the set. Its default value is false, which means that
        /// additions to the list of part numbers do not automatically cause credentials to be issued, and
        /// neither does the addition of this Set to a user.
        /// Credentials are always automatically revoked if part numbers are removed from the Set, if the Set
        /// is deleted, or if the Set is de-associated from the user.
        /// </summary>
        public virtual bool IssueCredentialsAutomatically { get; set; }

        public static HIDPartNumberSet DefaultEmptySet() {
            return new HIDPartNumberSet {
                Name = "Default",
                StoredPartNumbers = string.Empty
            };
        }

        /// <summary>
        /// Copy all this HIDPartNumberSet's properties to the destination object passed as parameter
        /// </summary>
        /// <param name="dest">The object that properties will be copied onto</param>
        public void CopyProperties(HIDPartNumberSet dest) {
            dest.Name = Name;
            dest.StoredPartNumbers = StoredPartNumbers;
            dest.IssueCredentialsAutomatically = IssueCredentialsAutomatically;
        }
    }
}