using System;

namespace Laser.Orchard.AppDirect.Models {
    public class UserTenantRecord {
        public virtual int Id { get; set; }
        public virtual string Email { get; set; }
        public virtual string Product { get; set; }
        public virtual string AccountIdentifier { get; set; }
        public virtual string UuidCreator { get; set; }
        public virtual bool Enabled { get; set; }
        public virtual DateTime TimeStamp { get; set; }
        public UserTenantRecord() {
            this.TimeStamp = DateTime.UtcNow;
        }
    }
}