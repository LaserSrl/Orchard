using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.MailCommunication.ViewModels;
using Laser.Orchard.TemplateManagement.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Orchard;

namespace Laser.Orchard.MailCommunication.Drivers
{
    [OrchardFeature("Laser.Orchard.MailerUtility")]
    public class MailerSiteSettingsPartDriver : ContentPartDriver<MailerSiteSettingsPart>
    {
        private readonly IOrchardServices _orchardServices;
        public MailerSiteSettingsPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        protected override string Prefix { get { return "MailerSettings"; } }

        // GET
        protected override DriverResult Editor(MailerSiteSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        // POST
        protected override DriverResult Editor(MailerSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_MailerSiteSettings_Edit", () => {
                var getpart = _orchardServices.WorkContext.CurrentSite.As<MailerSiteSettingsPart>();
                var vModel = new MailerSiteSettingsVM();
                vModel.Settings = getpart;
                if (updater != null) {
                    if(updater.TryUpdateModel(vModel, Prefix, null, null)) {
                        part = vModel.Settings;
                    }
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts/MailerSiteSettings", Model: vModel, Prefix: Prefix);
            })
                .OnGroup("Mailer");
        }
    }
}