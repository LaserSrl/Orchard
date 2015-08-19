using Orchard.ContentManagement.Records;
using System.Collections.Generic;

namespace Laser.Orchard.Queues.Models
{
    public class QueueUserPartRecord : ContentPartRecord
    {
        public QueueUserPartRecord()
        {
            UserQueues = new List<QueueUserRecord>();
        }

        public virtual IList<QueueUserRecord> UserQueues { get; set; }
    }
}