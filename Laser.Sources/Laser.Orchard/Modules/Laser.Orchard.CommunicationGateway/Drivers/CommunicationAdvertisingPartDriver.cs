using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.CommunicationGateway.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Logging;
using Orchard;
using Orchard.Localization;
using Orchard.ContentManagement;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class CommunicationAdvertisingPartDriver : ContentPartDriver<CommunicationAdvertisingPart> {


        //private readonly IOrchardServices _orchardServices;

        //public ILogger Logger { get; set; }
        //public Localizer T { get; set; }

        //protected override string Prefix {
        //    get { return "Laser.Orchard.CommunicationGateway"; }
        //}

        //public CommunicationAdvertisingPartDriver(IOrchardServices orchardServices) {
        //    _orchardServices = orchardServices;
        //    Logger = NullLogger.Instance;
        //    T = NullLocalizer.Instance;
        //}


        //protected override DriverResult Editor(CommunicationAdvertisingPart part, dynamic shapeHelper) {
        //    return null;
        //}

        //protected override DriverResult Editor(CommunicationAdvertisingPart part, IUpdateModel updater, dynamic shapeHelper) {
        //    DateTime datepublish = ((DateTime)(((dynamic)part).ContentItem.PublishLaterPart.ScheduledPublishUtc.Value));
        //    if (part.CampaignId > 0) {
        //        ContentItem campaign = _orchardServices.ContentManager.Get(part.CampaignId, VersionOptions.Latest);
        //        DateTime datelimitcampaign =(DateTime)( ((dynamic)campaign).CommunicationCampaignPart.ToDate.DateTime);
        //        if (datepublish > datelimitcampaign) {
        //            throw new Exception("Date in publish later is over date campaign");
        //        }
        //    }
        //    return null;
        //}
    }
}