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
using Laser.Orchard.MailCommunication.ViewModels;
using Orchard.Core.Title.Models;
using Orchard.Data;
using NHibernate;
using NHibernate.Transform;
using System.Collections;
using Laser.Orchard.CommunicationGateway.Services;

namespace Laser.Orchard.MailCommunication.Handlers {


    public class MailCommunicationPartHandler : ContentHandler {
        public Localizer T { get; set; }
        private readonly INotifier _notifier;
        private readonly ITemplateService _templateService;
        private readonly IOrchardServices _orchardServices;
        private readonly IQueryPickerService _queryPickerServices;
        private readonly IMailCommunicationService _mailCommunicationService;
        private readonly IRepository<CommunicationEmailRecord> _repoMail;
        private readonly IRepository<TitlePartRecord> _repoTitle;
        private readonly ITransactionManager _transactionManager;
        private readonly ISessionLocator _session;
        private readonly ICommunicationService _communicationService;


        public MailCommunicationPartHandler(INotifier notifier, ITemplateService templateService, IOrchardServices orchardServices, IQueryPickerService queryPickerServices, IMailCommunicationService mailCommunicationService,
            IRepository<CommunicationEmailRecord> repoMail, IRepository<TitlePartRecord> repoTitle, ITransactionManager transactionManager, ISessionLocator session, ICommunicationService communicationService) {
            _repoMail = repoMail;
            _repoTitle = repoTitle;
            _transactionManager = transactionManager;
            _notifier = notifier;
            _templateService = templateService;
            _orchardServices = orchardServices;
            _queryPickerServices = queryPickerServices;
            _mailCommunicationService = mailCommunicationService;
            _communicationService = communicationService;
            _session = session;
            T = NullLocalizer.Instance;
            OnUpdated<MailCommunicationPart>((context, part) => {
                if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.MailTest") {

                    if (part.SendToTestEmail && part.EmailForTest != string.Empty) {
                        dynamic content = context.ContentItem;
                        _templateService.SendTemplatedEmail(content,
                            ((Laser.Orchard.TemplateManagement.Models.CustomTemplatePickerPart)content.CustomTemplatePickerPart).SelectedTemplate.Id,
                            null,
                            new List<string> { part.EmailForTest },
                            new {
                                CampaignLink = _communicationService.GetCampaignLink("Email", part)
                            },
                            false);
                    }
                }
            });
            OnPublished<MailCommunicationPart>((context, part) => {
                if (part.SendOnNextPublish && !part.MailMessageSent) {
                    dynamic content = context.ContentItem;
                    IHqlQuery query;
                    if (content.QueryPickerPart != null && content.QueryPickerPart.Ids.Length > 0) {
                        query = _mailCommunicationService.IntegrateAdditionalConditions(_queryPickerServices.GetCombinedContentQuery(content.QueryPickerPart.Ids, null, new string[] { "CommunicationContact" }), content);
                    } else {
                        query = _mailCommunicationService.IntegrateAdditionalConditions(null, content);
                    }

                    // Trasformo in stringa HQL
                    var stringHQL = ((DefaultHqlQuery)query).ToHql(false);

                    // Rimuovo la Order by per poter fare la query annidata
                    // TODO: trovare un modo migliore per rimuovere la order by
                    stringHQL = stringHQL.ToString().Replace("order by civ.Id", "");

                    var queryForEmail = "SELECT distinct cir.Id as Id, TitlePart.Title as Title, EmailRecord.Email as EmailAddress FROM " +
                        "Orchard.ContentManagement.Records.ContentItemVersionRecord as civr join " +
                        "civr.ContentItemRecord as cir join " +
                        "civr.TitlePartRecord as TitlePart join " +
                        "cir.EmailContactPartRecord as EmailPart join " +
                            "EmailPart.EmailRecord as EmailRecord " +
                        "WHERE civr.Published=1 AND civr.Id in (" + stringHQL + ")";

                    // Creo query ottimizzata per le performance
                    var fullStatement = _session.For(null)
                        .CreateQuery(queryForEmail)
                        .SetCacheable(false)
                        ;
                    var lista = fullStatement
                        .SetResultTransformer(Transformers.AliasToEntityMap)
                        .List();

                    //TODO: Per Giuseppe
                    // Creazione Blob per FTP

                    // TemplateParser

                    // FTP

                    //Finisihed

                    //TODO: End Per Giuseppe

                }
            });
        }

    }
}
