using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.MailCommunication.ViewModels;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Laser.Orchard.MailCommunication.Drivers
{
    public class MailerSiteSettingsPartDriver : ContentPartDriver<MailerSiteSettingsPart>
    {
        private readonly ITemplateService _templateService;

        public MailerSiteSettingsPartDriver(ITemplateService templateService)
        {
            T = NullLocalizer.Instance;
            _templateService = templateService;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "MailerSettings"; } }

        // GET
        protected override DriverResult Editor(MailerSiteSettingsPart part, dynamic shapeHelper) {

            

            return Editor(part, null, shapeHelper);
        }

        // POST
        protected override DriverResult Editor(MailerSiteSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_MailerSiteSettings_Edit", () => {

                var vModel = new MailerSiteSettingsVM {
                    TemplatesUnsubscribeList = _templateService.GetTemplates().Where(w => ((dynamic)w.ContentItem).CustomTemplate.ForEmailCommunication.Value == true),
                    Settings = part
                };

                if (updater != null) {
                    if (updater.TryUpdateModel(part, Prefix, null, null) && updater.TryUpdateModel(vModel, Prefix, null, null)) {
                        part.IdTemplateUnsubscribe = vModel.Settings.IdTemplateUnsubscribe;
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Parts/MailerSiteSettings", Model: vModel, Prefix: Prefix);
            })
                .OnGroup("Mailer");
        }
    }
}