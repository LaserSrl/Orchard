using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;

namespace Laser.Orchard.Mobile.Handlers   {
    public class MobilePushPartHandler: ContentHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IScheduledTaskManager _taskManager;

        public MobilePushPartHandler(IRepository<MobilePushPartRecord> repository, IPushNotificationService pushNotificationService, IScheduledTaskManager taskManager, IOrchardServices orchardServices) {
            Logger = NullLogger.Instance;
            _pushNotificationService=pushNotificationService;
            _orchardServices = orchardServices;
            _taskManager = taskManager;
            Filters.Add(StorageFilter.For(repository));

            OnUpdated<MobilePushPart>((context, part) => {
                if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.PushTest"] == "submit.PushTest") {
                    // Invio Push di Test
                    _pushNotificationService.PublishedPushEventTest(part.ContentItem); 
                }
            });

            OnPublished<MobilePushPart>((context, part) => {
                try {
                    _taskManager.CreateTask("Laser.Orchard.PushNotification.Task", DateTime.UtcNow.AddMinutes(1), part.ContentItem);
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error starting asynchronous thread to send push notifications.");
                }
            });
        }
    }
}

