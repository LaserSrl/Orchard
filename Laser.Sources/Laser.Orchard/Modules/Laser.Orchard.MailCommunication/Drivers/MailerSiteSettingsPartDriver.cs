using Laser.Orchard.MailCommunication.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MailCommunication.Drivers
{
    public class MailerSiteSettingsPartDriver : ContentPartDriver<MailerSiteSettingsPart>
    {
        public MailerSiteSettingsPartDriver()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "MailerSettings"; } }

        protected override DriverResult Editor(MailerSiteSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(MailerSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_MailerSiteSettings_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts/MailerSiteSettings", Model: part, Prefix: Prefix);
            })
                .OnGroup("Mailer");
        }
    }
}