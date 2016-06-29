using Laser.Orchard.Mobile.Services;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Handlers {
    public class PushScheduledTaskHandler : IScheduledTaskHandler {
        private readonly IPushNotificationService _pushNotificationService;
        private const string TaskType = "Laser.Orchard.PushNotification.Task";

        public ILogger Logger { get; set; }

        public PushScheduledTaskHandler(IPushNotificationService pushNotificationService) {
            _pushNotificationService = pushNotificationService;
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context) {
            try {
                if (context.Task.TaskType != TaskType) {
                    return;
                }
                // esegue l'invio in un task schedulato
                _pushNotificationService.PublishedPushEvent(context.Task.ContentItem);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error in PushScheduledTaskHandler. ContentItem: {0}, ScheduledUtc: {1:yyyy-MM-dd HH.mm.ss} Please verify if it is necessary sending your push again.", context.Task.ContentItem, context.Task.ScheduledUtc);
            }
        }
    }
}