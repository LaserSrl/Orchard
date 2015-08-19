using System;

namespace Laser.Orchard.Queues.Models
{
    public class QueueUserRecord
    {
        public virtual int Id { get; set; }
        public virtual int QueueNumber { get; set; }
        public virtual int NumNotifications { get; set; }
        public virtual DateTime RegistrationDate { get; set; }
        public virtual QueueRecord QueueRecord { get; set; }
        public virtual QueueUserPartRecord QueueUserPartRecord { get; set; }
    }
}