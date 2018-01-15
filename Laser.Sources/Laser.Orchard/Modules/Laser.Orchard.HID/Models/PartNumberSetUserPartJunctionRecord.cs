namespace Laser.Orchard.HID.Models {
    /// <summary>
    /// This class represents records in a junction table to implement a many-to-many relationship
    /// between users and their selected part number sets.
    /// </summary>
    public class PartNumberSetUserPartJunctionRecord {
        public virtual int Id { get; set; }
        public virtual HIDPartNumberSet HIDPartNumberSet { get; set; }
        public virtual PartNumberSetsUserPartRecord PartNumberSetsUserPartRecord { get; set; }
    }
}