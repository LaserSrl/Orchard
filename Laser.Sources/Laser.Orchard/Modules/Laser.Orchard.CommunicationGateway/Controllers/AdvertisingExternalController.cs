using Laser.Orchard.CommunicationGateway.Events;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.PublishLater.Models;
using Orchard.PublishLater.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.CommunicationGateway.Controllers {

    public class AdvertisingExternalController : ApiController {

        private readonly IContentManager _contentManager;
        private readonly ICommunicationEventHandler _communicationEventHandlers;
        private readonly IPublishLaterService _publishLaterService;

        public AdvertisingExternalController(IContentManager contentManager, 
                                              ICommunicationEventHandler communicationEventHandlers, 
                                              IPublishLaterService publishLaterService) {
            _contentManager = contentManager;
            _communicationEventHandlers = communicationEventHandlers;
            _publishLaterService = publishLaterService;
        }

        public void Get() { }

        /// POST api/<controller>
        ///{
        ///   "Advertising": {
        ///      "Title": "Test",
        ///      "SmsGateway": {
        ///         "Id": 1,
        ///         "Text": "prova invio esterno Orchard",
        ///         "Contacts": {
        ///            "PhoneNumbers": [
        ///               "3401831897"
        ///            ]
        ///         }
        ///      },
        ///      "DatePublish": 20-12-2016 17:00 UTC
        ///   }
        ///}
        public void Post(AdvertisingVM.AdvertisingCommunication adv) {
            
            // Create Advertising
            ContentItem content = _contentManager.New("CommunicationAdvertising");

            ((dynamic)content).TitlePart.Title = adv.Advertising.Title;
            _communicationEventHandlers.PopulateChannel(content, adv.Advertising);

            _contentManager.Create(content, VersionOptions.Draft);

            // Data Publish in formato UTC
            DateTime dataPublish = adv.Advertising.DatePublish; 
            
            if (dataPublish.CompareTo(DateTime.Now) > 0) {
                // Publish Later
                _publishLaterService.Publish(content, dataPublish);
            }
            else {
                // Publish
                _contentManager.Publish(content);
            }
        }

        public void Put() { }

        public void Delete() { }
    }
}