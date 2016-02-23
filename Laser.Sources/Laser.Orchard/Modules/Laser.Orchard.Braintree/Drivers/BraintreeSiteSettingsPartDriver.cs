using Laser.Orchard.Braintree.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Braintree.Drivers
{
    public class BraintreeSiteSettingsPartDriver : ContentPartDriver<BraintreeSiteSettingsPart>
    {
        private const string TemplateName = "Parts/BraintreeSiteSettings";

        public BraintreeSiteSettingsPartDriver()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "BraintreeSettings"; } }

        protected override DriverResult Editor(BraintreeSiteSettingsPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(BraintreeSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape("Parts_BraintreeSiteSettings_Edit", () =>
            {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            })
                .OnGroup("Braintree PayPal");
        }
    }
}