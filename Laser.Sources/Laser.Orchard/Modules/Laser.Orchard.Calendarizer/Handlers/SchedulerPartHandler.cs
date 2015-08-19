using Laser.Orchard.Calendarizer.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Calendarizer.Handlers {
    public class SchedulerPartHandler : ContentHandler {
        public SchedulerPartHandler(IRepository<SchedulerPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}