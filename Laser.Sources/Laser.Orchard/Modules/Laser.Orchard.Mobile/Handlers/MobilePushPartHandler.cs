using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Mobile.Handlers   {
    public class MobilePushPartHandler: ContentHandler {
        private readonly IPushNotificationService _pushNotificationService;
        public MobilePushPartHandler(IRepository<MobilePushPartRecord> repository,IPushNotificationService pushNotificationService) {
            _pushNotificationService=pushNotificationService;
            Filters.Add(StorageFilter.For(repository));

            OnPublished<MobilePushPart>((context, part) =>
            _pushNotificationService.PublishedPushEvent(context,part.ContentItem));  
        }
    }
}

