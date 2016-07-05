using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;

namespace Laser.Orchard.Mobile.Handlers   {
    public class MobilePushPartHandler: ContentHandler {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IScheduledTaskManager _taskManager;

        public MobilePushPartHandler(IRepository<MobilePushPartRecord> repository,IPushNotificationService pushNotificationService, IScheduledTaskManager taskManager) {
            Logger = NullLogger.Instance;
            _pushNotificationService=pushNotificationService;
            _taskManager = taskManager;
            Filters.Add(StorageFilter.For(repository));

            OnPublished<MobilePushPart>((context, part) => {
                try {
                    _taskManager.CreateTask("Laser.Orchard.PushNotification.Task", DateTime.UtcNow.AddMinutes(1), part.ContentItem);
                    part.PushSent = true;
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error starting asynchronous thread to send push notifications.");
                }
            });
        }
    }
}

