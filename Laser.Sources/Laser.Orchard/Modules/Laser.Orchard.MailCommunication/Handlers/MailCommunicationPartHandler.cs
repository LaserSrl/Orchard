using System.Collections.Generic;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.Queries.Services;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Linq;
using System;
using Laser.Orchard.Services.MailCommunication;

namespace Laser.Orchard.MailCommunication.Handlers {


    public class MailCommunicationPartHandler : ContentHandler {
        public Localizer T { get; set; }
        private readonly INotifier _notifier;
        private readonly ITemplateService _templateService;
        private readonly IOrchardServices _orchardServices;
        private readonly IQueryPickerService _queryPickerServices;
        private readonly IMailCommunicationService _mailCommunicationService;


        public MailCommunicationPartHandler(INotifier notifier, ITemplateService templateService, IOrchardServices orchardServices, IQueryPickerService queryPickerServices, IMailCommunicationService mailCommunicationService) {
            _notifier = notifier;
            _templateService = templateService;
            _orchardServices = orchardServices;
            _queryPickerServices = queryPickerServices;
            _mailCommunicationService = mailCommunicationService;
            T = NullLocalizer.Instance;
            OnUpdated<MailCommunicationPart>((context, part) => {
                if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.MailTest") {

                    if (part.SendToTestEmail && part.EmailForTest != string.Empty) {
                        dynamic content = context.ContentItem;
                        _templateService.SendTemplatedEmail(content,
                            ((Laser.Orchard.TemplateManagement.Models.CustomTemplatePickerPart)content.CustomTemplatePickerPart).SelectedTemplate.Id,
                            null,
                            new List<string> { part.EmailForTest },
                            false);
                    }
                }
            });
            OnPublished<MailCommunicationPart>((context, part) => {
                if (part.SendOnNextPublish && !part.MailMessageSent) {
                    dynamic content = context.ContentItem;
                    IHqlQuery query;
                    if (content.QueryPickerPart != null && content.QueryPickerPart.Ids.Length > 0) {
                        query = _mailCommunicationService.IntegrateAdditionalConditions(_queryPickerServices.GetCombinedContentQuery(content.QueryPickerPart.Ids, null, new string[] { "CommunicationContact" }));
                    } else {
                        query = _mailCommunicationService.IntegrateAdditionalConditions();
                    }
                    var time1 = DateTime.Now;
                    // ciclo a blocchi da 500 record
                    var results = query.Slice(0,500);
                    while (results.Count() >= 500) {
                        var serializableEmails = results.Select(s => new { Emails = s.As<CommunicationContactPart>().Emails }).ToList();
                    } 
                    
                    var timeSpan = DateTime.Now.Subtract(time1);
                }
            });
        }

    }
}
