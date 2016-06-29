using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Models {
    public class SentRecord {
        public virtual int Id { get; set; }
        public virtual int PushNotificationRecord_Id { get; set; }
        public virtual int PushedItem { get; set; }
        public virtual DateTime SentDate { get; set; }
        public virtual string DeviceType { get; set; }
    }
}
