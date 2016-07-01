using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Mobile.Handlers   {
    public class MobilePushPartHandler: ContentHandler {
        private readonly IPushNotificationService _pushNotificationService;
        private IOrchardServices _orchardServices;

        public MobilePushPartHandler(IRepository<MobilePushPartRecord> repository,IPushNotificationService pushNotificationService, IOrchardServices orchardServices) {
            _pushNotificationService=pushNotificationService;
            _orchardServices = orchardServices;

            Filters.Add(StorageFilter.For(repository));

            OnUpdated<MobilePushPart>((context, part) => {
                if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.PushTest"] == "submit.PushTest") {
                    // Invio Push di Test
                    _pushNotificationService.PublishedPushEventTest(context, part.ContentItem); 
                }
            });

            OnPublished<MobilePushPart>((context, part) =>
            _pushNotificationService.PublishedPushEvent(context,part.ContentItem));  
        }
    }
}

