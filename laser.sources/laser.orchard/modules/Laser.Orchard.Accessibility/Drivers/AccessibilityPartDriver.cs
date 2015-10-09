using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Accessibility.Models;
using Orchard.ContentManagement.Drivers;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Environment.Configuration;

namespace Laser.Orchard.Accessibility.Drivers
{
    public class AccessibilityPartDriver : ContentPartDriver<AccessibilityPart>
    {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private Localizer t;
        public ILogger Logger { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.Accessibility"; }
        }

        public AccessibilityPartDriver(IOrchardServices orchardServices, ShellSettings shellSetting)
        {
            _orchardServices = orchardServices;
            _shellSettings = shellSetting;
            t = NullLocalizer.Instance;
        }

        protected override DriverResult Display(AccessibilityPart part, string displayType, dynamic shapeHelper)
        {
            // calcola l'url del controller
            string tenantPath = new Utils().GetTenantBaseUrl(_shellSettings);
            tenantPath = (tenantPath.EndsWith("/")) ? tenantPath : tenantPath + "/";
            tenantPath = tenantPath + "Laser.Orchard.Accessibility/Accessibility";

            // gestisce il tipo di visualizzazione
            if (displayType == "Summary")
                return ContentShape("Parts_Accessibility_Summary",
                    () => shapeHelper.Parts_Accessibility_Summary());
            if (displayType == "SummaryAdmin")
                return ContentShape("Parts_Accessibility_SummaryAdmin",
                    () => shapeHelper.Parts_Accessibility_SummaryAdmin());

            // visualizzazione di dettaglio
            return ContentShape("Parts_Accessibility",
                () => shapeHelper.Parts_Accessibility(Url: tenantPath));
        }
    }
}