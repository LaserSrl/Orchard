using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Laser.Orchard.Queues.Models;

namespace Laser.Orchard.Queues.Handlers
{
    public class QueueUserPartHandler : ContentHandler
    {
        public QueueUserPartHandler(IRepository<QueueUserPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}