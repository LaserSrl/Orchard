using Laser.Orchard.Caligoo.Models;
using Laser.Orchard.Caligoo.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
                var jObj = _caligooService.GetLocations();
                foreach(var jLoc in jObj) {
                    var location = jLoc.ToObject<LocationMessage>();
                    _caligooService.CreateOrUpdateLocation(location);
                }
            } catch(Exception ex) {
                Logger.Error(ex, "LocationSynchronize");
            }
        }
    }
}