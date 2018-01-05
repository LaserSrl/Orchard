using Laser.Orchard.Caligoo.DataContracts;
using Laser.Orchard.Caligoo.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;

namespace Laser.Orchard.Caligoo.Tasks {
    public class LocationSynchronizeScheduledTaskHandler : IScheduledTaskHandler {
        private readonly ICaligooService _caligooService;
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        private const string TaskType = "Laser.Orchard.Caligoo.Tasks.LocationSynchronize";
        public LocationSynchronizeScheduledTaskHandler(ICaligooService caligooService) {
            _caligooService = caligooService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }
        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType != TaskType) {
                return;
            }
            try {
                var locationList = _caligooService.GetLocations();
                foreach(var location in locationList) {
                    _caligooService.UpdateLocation(location);
                }
            } catch(Exception ex) {
                Logger.Error(ex, "LocationSynchronize");
            }
        }
    }
}