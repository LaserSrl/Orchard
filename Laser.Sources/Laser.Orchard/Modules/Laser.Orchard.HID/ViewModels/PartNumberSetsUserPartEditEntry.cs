namespace Laser.Orchard.HID.ViewModels {
    public class PartNumberSetsUserPartEditEntry {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
        public int Id { get; set; } // It's easier to get back the set through its Id
    }
}