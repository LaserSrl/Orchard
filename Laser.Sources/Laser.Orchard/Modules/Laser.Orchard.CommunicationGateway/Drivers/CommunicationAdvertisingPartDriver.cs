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
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.CommunicationGateway.Drivers {
    public class CommunicationAdvertisingPartDriver : ContentPartDriver<CommunicationAdvertisingPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly ICultureManager _cultureManager;
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.CommunicationAdvertisingPartDriver"; }
        }

        public CommunicationAdvertisingPartDriver(IOrchardServices orchardServices, ICultureManager cultureManager) {
            _orchardServices = orchardServices;
            //    Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _cultureManager = cultureManager;
        }

        protected override DriverResult Editor(CommunicationAdvertisingPart part, dynamic shapeHelper) {
            bool linkinterno = true;
            if (!string.IsNullOrEmpty(((dynamic)part).UrlLinked.Value)) {
                linkinterno = false;
            }

            var shapes = new List<DriverResult>();
            Dictionary<string, Int32> model = new Dictionary<string, int>();
            if (part.ContentItem.As<LocalizationPart>().Culture != null)
                model.Add("LocalizationId", part.ContentItem.As<LocalizationPart>().Culture.Id);
            else
                model.Add("LocalizationId", _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id);
            shapes.Add(ContentShape("Parts_Advertising_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/Advertising_Edit", Model: model, Prefix: Prefix)));//
            shapes.Add(ContentShape("Parts_AdvertisingSwitcher", () => shapeHelper.EditorTemplate(TemplateName: "Parts/AdvertisingSwitcher", Model: linkinterno, Prefix: Prefix)));

            return new CombinedResult(shapes);
        }

        protected override DriverResult Editor(CommunicationAdvertisingPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                updater.AddModelError("Communication Advertising", T("AdvertisingPart Error"));
            }
            else
                if (_orchardServices.WorkContext.HttpContext.Request.Form["linkinterno"] == "1") //switch urlinterno - urlesterno
                    ((dynamic)part).UrlLinked.Value = null;
                else
                    ((dynamic)part).ContentLinked.Ids = new int[] { };
            return Editor(part, shapeHelper);
            //  return null;
        }


        protected override void Importing(CommunicationAdvertisingPart part, ImportContentContext context) {
            //throw new NotImplementedException();
           // context.ImportAttribute(part.PartDefinition.Name, "CampaignId", s => part.ContentItem = context.GetItemFromSession(s));

            var root = context.Data.Element(part.PartDefinition.Name);
            part.CampaignId = int.Parse(root.Attribute("CampaignId").Value);
        }

        protected override void Exporting(CommunicationAdvertisingPart part, ExportContentContext context) {
            //throw new NotImplementedException();
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("CampaignId", part.CampaignId);

        }

    }
}