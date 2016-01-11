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


        public MailCommunicationPartHandler(INotifier notifier, ITemplateService templateService, IOrchardServices orchardServices, IQueryPickerService queryPickerServices, IMailCommunicationService mailCommunicationService,
            IRepository<CommunicationEmailRecord> repoMail, IRepository<TitlePartRecord> repoTitle, ITransactionManager transactionManager, ISessionLocator session) {
            _repoMail = repoMail;
            _repoTitle = repoTitle;
            _transactionManager = transactionManager;
            _notifier = notifier;
            _templateService = templateService;
            _orchardServices = orchardServices;
            _queryPickerServices = queryPickerServices;
            _mailCommunicationService = mailCommunicationService;
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
                    // List troppo impegnativo
                    // var results = query.List().Select(s => new { Emails = s.As<EmailContactPart>().EmailRecord });
                    // quindi ciclo a blocchi da 500 record
                    var i = 0;
                    var parts = new string[] { "TitlePart", "EmailContactPart" };
                    //var results = query.Include(parts).Slice((500 * i), 500).Select(s => new RecipientData { 
                    //    Email = ((dynamic)s).EmailContactPart.EmailRecord[0].Email,
                    //    Title = ((dynamic)s).TitlePart.Title,
                    //}).ToList();

                    // DA HQL A SQL
                    var stringHQL = ((DefaultHqlQuery)query).ToHql(false);
                    
                    // Rimuovo la Order by per poter fare la query annidata
                    // TODO: trovare un modo migliore per rimuovere la order by
                    stringHQL = stringHQL.ToString().Replace("order by civ.Id", "");
                    var queryForEmail = "SELECT TitlePart.Title as Title, EmailRecord.Email as EmailAddress FROM Orchard.Core.Title.Models.TitlePartRecord as TitlePart, Laser.Orchard.CommunicationGateway.Models.EmailContactPartRecord as EmailPart  join EmailPart.EmailRecord as EmailRecord  WHERE TitlePart.Id=EmailPart.Id AND EmailPart.Id in (" + stringHQL + ")";
                    // Creo query ottimizzata per le performance
                    var secondQuery = _orchardServices.ContentManager.HqlQuery().ForVersion(VersionOptions.Published).Join(x => x.ContentPartRecord<TitlePartRecord>()).Join(y => y.ContentPartRecord<EmailContactPartRecord>());
                    var SQLStatement2 = _session.For(null)
                        .CreateQuery(queryForEmail)
                        .SetCacheable(false)
                        ;
                    var lista = SQLStatement2
                        .SetResultTransformer(Transformers.AliasToEntityMap)
                        .List();

                    // 150000 record in < 5sec
                    // ELENCO IDS

                    var timeSpan = DateTime.Now.Subtract(time1);
                }
            });
        }

    }
}
