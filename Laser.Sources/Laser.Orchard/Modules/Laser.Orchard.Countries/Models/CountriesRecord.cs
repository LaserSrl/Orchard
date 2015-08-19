using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laser.Orchard.Countries.Models {
    public class CountriesRecord {
        public virtual int Id { get; set; }
        public virtual string Des_en { get; set; }
        public virtual string Des_fr { get; set; }
        public virtual string Des_local { get; set; }
        public virtual string Des_Region { get; set; }
    }

    public class LaserCountriesZoneRecord {
        public virtual int Id { get; set; }
        public virtual string Descrizione { get; set; }
    }

    public class LaserLinkedCountriesZoneRecord {
        public virtual int Id { get; set; }
        public virtual int LaserCountriesZoneRecord_Id { get; set; }
        public virtual int CountriesRecord_Id { get; set; }
    }
}