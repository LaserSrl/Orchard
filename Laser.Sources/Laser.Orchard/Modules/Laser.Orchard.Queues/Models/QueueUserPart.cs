using Orchard.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Queues.Models
{
    public class QueueUserPart : ContentPart<QueueUserPartRecord>
    {
        public IList<QueueUserRecord> UserQueues
        {
            get { return Record.UserQueues; }
        }
    }
}