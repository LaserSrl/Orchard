using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Tasks.Scheduling;
using System;

namespace Laser.Orchard.Mobile.Handlers   {
    public class MobilePushPartHandler: ContentHandler {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IScheduledTaskManager _taskManager;

        public MobilePushPartHandler(IRepository<MobilePushPartRecord> repository,IPushNotificationService pushNotificationService, IScheduledTaskManager taskManager) {
            _pushNotificationService=pushNotificationService;
            _taskManager = taskManager;
            Filters.Add(StorageFilter.For(repository));

            OnPublished<MobilePushPart>((context, part) => {
                //ContentItem ci = _orchardServices.ContentManager.Get(part.ContentItem.Id);
                _taskManager.CreateTask("Laser.Orchard.PushNotification.Task", DateTime.UtcNow.AddMinutes(1), part.ContentItem);
            });
        }
    }
}

