using System.Collections.Generic;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.Queries.Services;
using Laser.Orchard.TemplateManagement.Services;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.StartupConfig.Extensions;
using Laser.Orchard.Commons.Extensions;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Environment.Extensions;
using Orchard.Email.Models;
using Orchard.Mvc.Extensions;
using System.Linq;
using System;
using System.Web.Mvc;
using Laser.Orchard.Services.MailCommunication;
using Laser.Orchard.MailCommunication.ViewModels;
using Orchard.Core.Title.Models;
using Orchard.Data;
using NHibernate;
using NHibernate.Transform;
using System.Collections;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.StartupConfig.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


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
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private MailerSiteSettingsPart _mailerConfig;

        public MailCommunicationPartHandler(IControllerContextAccessor controllerContextAccessor,INotifier notifier, ITemplateService templateService, IOrchardServices orchardServices, IQueryPickerService queryPickerServices, IMailCommunicationService mailCommunicationService,
            IRepository<CommunicationEmailRecord> repoMail, IRepository<TitlePartRecord> repoTitle, ITransactionManager transactionManager, ISessionLocator session, ICommunicationService communicationService) {
            _repoMail = repoMail;
            _repoTitle = repoTitle;
            _controllerContextAccessor = controllerContextAccessor;
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
                         Dictionary<string,object> similViewBag= new Dictionary<string,object>();
                        similViewBag.Add("CampaignLink", _communicationService.GetCampaignLink("Email", part));
                        //if (_controllerContextAccessor.Context.Controller.TempData.Keys.Contains("CampaignLink")) {
                        //                    }else{
                        //    _controllerContextAccessor.Context.Controller.TempData.Add("CampaignLink", _communicationService.GetCampaignLink("Email", part));
                        //} 
                        _templateService.SendTemplatedEmail(content,
                            ((Laser.Orchard.TemplateManagement.Models.CustomTemplatePickerPart)content.CustomTemplatePickerPart).SelectedTemplate.Id,
                            null,
                            new List<string> { part.EmailForTest },
                            similViewBag
                            //_controllerContextAccessor.Context.Controller.ViewBag
                            //new {
                            //    CampaignLink = _communicationService.GetCampaignLink("Email", part)
                            //}
                            ,
                            false);
                    }
                }
            });
            OnPublished<MailCommunicationPart>((context, part) => {
                _mailerConfig = _orchardServices.WorkContext.CurrentSite.As<MailerSiteSettingsPart>();
                if (part.SendOnNextPublish && !part.MailMessageSent)
                {
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

                    // ricava i settings e li invia tramite FTP
                    var templateId = ((Laser.Orchard.TemplateManagement.Models.CustomTemplatePickerPart)content.CustomTemplatePickerPart).SelectedTemplate.Id;
                    Dictionary<string, object> settings = GetSettings(content, templateId);
                    SendSettings(settings, part.Id);

                    // impagina e invia i recipiens tramite FTP
                    int pageNum = 0;
                    List<object> pagina = new List<object>();
                    int pageSize = _mailerConfig.RecipientsPerJsonFile;
                    for (int i = 0; i < lista.Count; i++)
                    {
                        if (((i+1) % pageSize) == 0)
                        {
                            SendRecipients(pagina, part.Id, pageNum);
                            pageNum++;
                            pagina = new List<object>();
                        }
                        pagina.Add(lista[i]);
                    }
                    // invia l'ultima pagina se non è vuota
                    if (pagina.Count > 0)
                    {
                        SendRecipients(pagina, part.Id, pageNum);
                    }

                    // inizializza RecipientsNumber, SentMailsNumber e MailMessageSent
                    part.RecipientsNumber = lista.Count;
                    part.SentMailsNumber = 0;
                    part.MailMessageSent = true;
                }
            });
        }

        private Dictionary<string, object> GetSettings(dynamic contentModel, int templateId)
        {
            ParseTemplateContext templatectx = new ParseTemplateContext();
            var template = _orchardServices.ContentManager.Get<TemplatePart>(templateId);
            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);

            // Creo un model che ha Content (il contentModel), Urls con alcuni oggetti utili per il template
            // Nel template pertanto Model, diventa Model.Content
            var host = string.Format("{0}://{1}{2}",
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Scheme,
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Host,
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Port == 80
                                        ? string.Empty
                                        : ":" + _orchardServices.WorkContext.HttpContext.Request.Url.Port);
            var dynamicModel = new
            {
                Content = contentModel,
                Urls = new
                {
                    //SubscriptionSubscribe = urlHelper.SubscriptionSubscribe(),
                    //SubscriptionUnsubscribe = urlHelper.SubscriptionUnsubscribe(),
                    //SubscriptionConfirmSubscribe = urlHelper.SubscriptionConfirmSubscribe(),
                    //SubscriptionConfirmUnsubscribe = urlHelper.SubscriptionConfirmUnsubscribe(),
                    BaseUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl,
                    MediaUrl = urlHelper.MediaExtensionsImageUrl(),
                    Domain = host,

                }.ToExpando()
            };
            templatectx.Model = dynamicModel;

            var body = _templateService.ParseTemplate(template, templatectx);
            var subject = (contentModel as ContentItem).As<TitlePart>().Title;
            var smtp = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            string priority = "L";
            switch (_mailerConfig.MailPriority)
            {
                case MailPriorityValues.High:
                    priority = "H";
                    break;
                case MailPriorityValues.Normal:
                    priority = "N";
                    break;
                default:
                    priority = "L";
                    break;
            }

            var baseUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
            // token di sicurezza: contiene data e ora (senza minuti e secondi) e id del content item
            var token = string.Format("{0}{1}", DateTime.Now.ToString("yyyyMMddHH"), (contentModel as ContentItem).Id);
            token = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(token));
            //var url = string.Format("{0}/Laser.Orchard.MailCommunication/MailerResult?tk={1}", baseUrl, token);  // versione per il GET
            var url = string.Format("{0}/api/Laser.Orchard.MailCommunication/MailerResultAPI?tk={1}", baseUrl, token);  // versione per il POST
            var data = new Dictionary<string, object>();
            data.Add("Subject", subject);
            data.Add("Body", body);
            data.Add("Sender", smtp.Address);
            data.Add("Priority", priority);
            data.Add("Url", url);  // url di ritorno per comunicare a Orchard il numero di mail inviate con successo
            data.Add("Attachments", ""); // TODO esempio: "[\"prova.pdf\",\"prova.docx\"]" 2016-01-14: per ora non li gestiamo

            return data;
        }

        private void SendRecipients(List<object> recipients, int communicationId, int pageNum)
        {
            string pathFtp = _mailerConfig.FtpPath;
            string jsonDestinatari = JsonConvert.SerializeObject(recipients);
            SendFtp(jsonDestinatari, _mailerConfig.FtpHost, _mailerConfig.FtpUser, _mailerConfig.FtpPassword, string.Format("{0}adv{1}-{2}-recipients.json", pathFtp, communicationId, pageNum));
        }

        private void SendSettings(object settings, int communicationId)
        {
            string pathFtp = _mailerConfig.FtpPath;
            string jsonSettings = JsonConvert.SerializeObject(settings);
            SendFtp(jsonSettings, _mailerConfig.FtpHost, _mailerConfig.FtpUser, _mailerConfig.FtpPassword, string.Format("{0}adv{1}-settings.json", pathFtp, communicationId));
        }

        /// <summary>
        /// Invia un testo (con codifica Unicode) tramite FTP.
        /// </summary>
        /// <param name="contenuto">Il testo da inviare.</param>
        /// <param name="host"></param>
        /// <param name="usr"></param>
        /// <param name="pwd"></param>
        /// <param name="fileName">Path e nome del file di destinazione.</param>
        private void SendFtp(string contenuto, string host, string usr, string pwd, string fileName)
        {
            // upload di un file tramite ftp
            using (System.Net.FtpClient.FtpClient client = new System.Net.FtpClient.FtpClient())
            {
                client.Host = host;
                client.Credentials = new System.Net.NetworkCredential(usr, pwd);
                client.Connect();
                using (var ftpStream = client.OpenWrite(fileName))
                {
                    byte[] buffer = System.Text.ASCIIEncoding.Unicode.GetBytes(contenuto);
                    ftpStream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
