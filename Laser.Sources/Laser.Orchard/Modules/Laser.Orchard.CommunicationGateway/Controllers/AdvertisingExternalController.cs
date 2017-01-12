using Laser.Orchard.CommunicationGateway.Events;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
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
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }

        public AdvertisingExternalController(IContentManager contentManager, 
                                              ICommunicationEventHandler communicationEventHandlers,
                                              IPublishLaterService publishLaterService, IOrchardServices orchardServices) {
            _contentManager = contentManager;
            _communicationEventHandlers = communicationEventHandlers;
            _publishLaterService = publishLaterService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public void Get() { }

        /// POST api/<controller>
        ///{
        ///  "Advertising": {
        ///    "Title": "Comunicato Sms - Id 5007",
        ///    "SmsGateway": {
        ///      "Id": 5007,
        ///      "Text": "Test invio da WSKrakeAdvertisingSms",
        ///      "PhoneNumbers": [
        ///        "393401831897"
        ///      ]
        ///    },
        ///    "DatePublish": "2016-12-23T13:00:00Z"
        ///  }
        ///}
        [Authorize]
        public AdvertisingCommunicationAPIResult Post(AdvertisingCommunication adv) {

            int communicationAdvertising_Id = -1;
            string errorString = "";
            string infoAdvertising = "";

            try {
                if (adv == null)
                    errorString = T("The provided data does not correspond to the required format.").ToString();
                else {
                    // Create Advertising
                    ContentItem content = _contentManager.New("CommunicationAdvertising");

                    // Check permissions User 
                    if (_orchardServices.Authorizer.Authorize(Permissions.PublishCommunicationAdv, content, T("Couldn't create content"))) {

                        ((dynamic)content).TitlePart.Title = adv.Advertising.Title;
                        _communicationEventHandlers.PopulateChannel(content, adv.Advertising);

                        _contentManager.Create(content, VersionOptions.Draft);

                        // Data Publish in formato UTC
                        DateTime dataPublish = adv.Advertising.DatePublish;
                        if (dataPublish == null || dataPublish == DateTime.MinValue) {
                            dataPublish = DateTime.UtcNow;
                        }

                        if (dataPublish.CompareTo(DateTime.UtcNow) > 0) {
                            // Publish Later
                            _publishLaterService.Publish(content, dataPublish);
                        } else {
                            // Publish
                            _contentManager.Publish(content);
                        }

                        communicationAdvertising_Id = content.Id;
                        infoAdvertising = "Create Advertising Id: " + content.Id + " - Title: " + adv.Advertising.Title;
                    } 
                    else {
                        infoAdvertising = "User couldn't create Advertising";
                    }
                }
            } 
            catch (Exception ex) {
                errorString = ex.Message;
            }

            return new AdvertisingCommunicationAPIResult { Id = communicationAdvertising_Id, Error = errorString, Information = infoAdvertising };
        }

        public void Put() { }

        public void Delete() { }
    }
}