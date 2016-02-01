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
        private readonly IOrchardServices _orchardServices;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.CommunicationAdvertisingPartDriver"; }
        }

        public CommunicationAdvertisingPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            //    Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }


        protected override DriverResult Editor(CommunicationAdvertisingPart part, dynamic shapeHelper) {
            bool linkinterno = true;
            if (!string.IsNullOrEmpty(((dynamic)part).UrlLinked.Value)) {
                linkinterno = false;
            }

            var shapes = new List<DriverResult>();
            shapes.Add(ContentShape("Parts_Advertising_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/Advertising_Edit", Model: null, Prefix: Prefix)));
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
    }
}