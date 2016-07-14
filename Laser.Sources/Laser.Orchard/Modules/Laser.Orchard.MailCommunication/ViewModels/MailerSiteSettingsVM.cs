using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.TemplateManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MailCommunication.ViewModels {

    public class MailerSiteSettingsVM {
        public IEnumerable<TemplatePart> TemplatesUnsubscribeList { get; set; }
        public MailerSiteSettingsPart Settings { get; set; }
    }
}