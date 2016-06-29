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
        private readonly IOrchardServices _orchardServices;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IScheduledTaskManager _taskManager;

        public ILogger Logger { get; set; }

        public MobilePushPartHandler(IRepository<MobilePushPartRecord> repository,IPushNotificationService pushNotificationService, IScheduledTaskManager taskManager) {
        //public MobilePushPartHandler( IWorkContextAccessor workContextAccessor,IRepository<MobilePushPartRecord> repository, IPushNotificationService pushNotificationService, IOrchardServices orchardServices) {
            Logger = NullLogger.Instance;
            //_workContextAccessor = workContextAccessor;
            _pushNotificationService=pushNotificationService;
            //_orchardServices = orchardServices;
            _taskManager = taskManager;
            Filters.Add(StorageFilter.For(repository));

            OnPublished<MobilePushPart>((context, part) => {
                try {
                    ////ContentItem ci = _orchardServices.ContentManager.Get(part.ContentItem.Id);
                    _taskManager.CreateTask("Laser.Orchard.PushNotification.Task", DateTime.UtcNow.AddMinutes(1), part.ContentItem);
                    
                    //// esegue l'invio in un thread asincrono
                    //string siteCulture = _orchardServices.WorkContext.CurrentSite.SiteCulture;
                    //PushMobileSettingsPart settings = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>();
                    ////WorkContextAccessor wca = _workContextAccessor;
                    //System.Threading.Tasks.Task.Run(() => {
                    //    _pushNotificationService.PublishedPushEvent(part.ContentItem,_workContextAccessor.GetContext());// context, siteCulture, settings, part.ContentItem);
                    //});
                    
                    //_pushNotificationService.PublishedPushEvent(part.ContentItem);
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error starting asynchronous thread to send push notifications.");
                }
            });
        }
    }
}

