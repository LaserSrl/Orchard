using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Laser.Orchard.Caligoo.Models {
    public class CaligooLocationPart : ContentPart<CaligooLocationPartRecord> {
        public int CaligooLocationId {
            get { return Retrieve(r => r.CaligooLocationId); }
            set { Store(r => r.CaligooLocationId , value); }
        }
        public string DisplayName {
            get { return Retrieve(r => r.DisplayName); }
            set { Store(r => r.DisplayName, value); }
        }
        public string Address {
            get { return Retrieve(r => r.Address); }
            set { Store(r => r.Address, value); }
        }
        public string PostalCode {
            get { return Retrieve(r => r.PostalCode); }
            set { Store(r => r.PostalCode, value); }
        }
        public string City {
            get { return Retrieve(r => r.City); }
            set { Store(r => r.City, value); }
        }
        public string Country {
            get { return Retrieve(r => r.Country); }
            set { Store(r => r.Country, value); }
        }
        public decimal Latitude {
            get { return Retrieve(r => r.Latitude); }
            set { Store(r => r.Latitude, value); }
        }
        public decimal Longitude {
            get { return Retrieve(r => r.Longitude); }
            set { Store(r => r.Longitude, value); }
        }
    }

    public class CaligooLocationPartRecord: ContentPartRecord {
        public virtual int CaligooLocationId { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string Address { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual string City { get; set; }
        public virtual string Country { get; set; }
        public virtual decimal Latitude { get; set; }
        public virtual decimal Longitude { get; set; }
    }
}