using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Models {
    public class HIDSettingsPartRecord : ContentPartRecord {
        public virtual int CustomerID { get; set; }
        public virtual bool UseTestEnvironment { get; set; }
        public virtual string ClientID { get; set; } //username
        public virtual string ClientSecret { get; set; } //password
        public virtual string PartNumbers { get; set; } //array of part numbers, stored as a single string
    }
}