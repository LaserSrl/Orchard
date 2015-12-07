using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Collections.Generic;
using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;

namespace Laser.Orchard.MailCommunication.Handlers {


    public class MailCommunicationPartHandler : ContentHandler {
        public Localizer T { get; set; }
        private readonly INotifier _notifier;
        private readonly ITemplateService _templateService;
        private readonly IOrchardServices _orchardServices;

        public MailCommunicationPartHandler(INotifier notifier, ITemplateService templateService, IOrchardServices orchardServices) {
            _notifier = notifier;
            _templateService = templateService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
            OnUpdated<MailCommunicationPart>((context, part) => {
                if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.MailTest") {

                    if (part.SendToTestEmail && part.EmailForTest != string.Empty) {
                        dynamic content = context.ContentItem;
                        _templateService.SendTemplatedEmail(content, ((Laser.Orchard.TemplateManagement.Models.CustomTemplatePickerPart)content.CustomTemplatePickerPart).SelectedTemplate.Id, null, new List<string> { part.EmailForTest }, false);
                    }
                }
            });
        }
    }
}