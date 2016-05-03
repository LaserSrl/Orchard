using Laser.Orchard.NewsLetters.Models;
using Laser.Orchard.MailCommunication.Models;
using Laser.Orchard.NewsLetters.Services;
using Laser.Orchard.NewsLetters.Extensions;
using Laser.Orchard.StartupConfig.Extensions;
using Laser.Orchard.Commons.Extensions;
using Laser.Orchard.TemplateManagement.Services;
using Laser.Orchard.MailCommunication;
using Newtonsoft.Json;
using Orchard;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Tasks.Scheduling;
using Orchard.Environment.Configuration;
using Orchard.Email.Models;
using Orchard.UI.Notify;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Laser.Orchard.NewsLetters.Handlers {
    public class NewsletterEditionSendTaskHandler : IScheduledTaskHandler {

        public Localizer T { get; set; }

        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly INewsletterServices _newslServices;
        private readonly ITemplateService _templateService;
        private readonly INotifier _notifier;
        private readonly IRepository<NewsletterEditionPartRecord> _repositoryNewsletterEdition;
        private readonly ShellSettings _shellSettings;
        
        private MailerSiteSettingsPart _mailerConfig;
        
        private const string TaskType = "Laser.Orchard.NewsLetters.SendEdition.Task";

        public NewsletterEditionSendTaskHandler(IContentManager contentManager, IOrchardServices orchardServices,
                                                INewsletterServices newslServices, ITemplateService templateService,
                                                INotifier notifier, ShellSettings shellSettings,
                                                IRepository<NewsletterEditionPartRecord> repositoryNewsletterEdition) {
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _newslServices = newslServices;
            _templateService = templateService;
            _shellSettings = shellSettings;
            _notifier = notifier;
            _repositoryNewsletterEdition = repositoryNewsletterEdition;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType != TaskType) {
                return;
            }

            dynamic content = context.Task.ContentItem;
            NewsletterEditionPart part = context.Task.ContentItem.As<NewsletterEditionPart>();

            _mailerConfig = _orchardServices.WorkContext.CurrentSite.As<MailerSiteSettingsPart>();

            int[] selectedAnnIds;
            IList<AnnouncementPart> items = null;
            if (!String.IsNullOrWhiteSpace(part.AnnouncementIds)) {
                selectedAnnIds = !String.IsNullOrWhiteSpace(part.AnnouncementIds) ? part.AnnouncementIds.Split(',').Select(s => Convert.ToInt32(s)).ToArray() : null;
                items = GetAnnouncements(selectedAnnIds);
            }

            var subscribers = _newslServices.GetSubscribers(part.NewsletterDefinitionPartRecord_Id).Where(w => w.Confirmed);
            var subscribersEmails = subscribers.Select(s => new { s.Id, s.Name, s.Email });

            // ricava i settings e li invia tramite FTP
            var templateId = _newslServices.GetNewsletterDefinition(part.NewsletterDefinitionPartRecord_Id,
                VersionOptions.Published).As<NewsletterDefinitionPart>().TemplateRecord_Id;

            Dictionary<string, object> settings = GetSettings(content, templateId, part);
            
            if (settings.Count > 0) {
                SendSettings(settings, part.Id);

                // impagina e invia i recipiens tramite FTP
                int pageNum = 0;
                List<object> pagina = new List<object>();
                List<object> listaSubscribers = new List<object>(subscribersEmails);
                int pageSize = _mailerConfig.RecipientsPerJsonFile;

                for (int i = 0; i < listaSubscribers.Count; i++) {
                    if (((i + 1) % pageSize) == 0) {
                        SendRecipients(pagina, part.Id, pageNum);
                        pageNum++;
                        pagina = new List<object>();
                    }
                    pagina.Add(listaSubscribers[i]);
                }
                // invia l'ultima pagina se non è vuota
                if (pagina.Count > 0) {
                    SendRecipients(pagina, part.Id, pageNum);
                }

                // Aggiorno la newsletter edition, e rimuovo la relazione tra Newletter e Announcement 
                part.Dispatched = true;
                part.DispatchDate = DateTime.Now;
                part.Number = GetNextNumber(part.NewsletterDefinitionPartRecord_Id); ;
                foreach (var item in items) {
                    var ids = ("," + item.AttachToNextNewsletterIds + ",").Replace("," + part.NewsletterDefinitionPartRecord_Id + ",", "");
                    item.AttachToNextNewsletterIds = ids;
                }

                // TODO: 
                // - Chiedere ad Hermes come effetture delle Subscribe
                // - Chiedere se è necessario un ResulAPIController come per le mail (nel nostro caso è presente un SentMailsNumber)
                // - Nel caso non ci fosse bisogno url nei settings non serve a nulla (da togliere)
                // - Altrimenti modificare NewsletterEditionPart e creare un controller tipo MailerResultAPIController

                _contentManager.Publish(context.Task.ContentItem);
            }
            else {
                _notifier.Error(T("Error parsing mail template."));
            }
        }

        private IList<AnnouncementPart> GetAnnouncements(int[] selectedIds) {
            var list = _contentManager.Query<AnnouncementPart, AnnouncementPartRecord>(VersionOptions.Published)
                .Where(w => selectedIds.Contains(w.Id))
                .OrderBy(br => br.AnnouncementTitle)
                .List();
            return list.ToList();
        }

        private void SendRecipients(List<object> recipients, int communicationId, int pageNum) {
            string pathFtp = _mailerConfig.FtpPath;
            string jsonDestinatari = JsonConvert.SerializeObject(recipients);
            SendFtp(jsonDestinatari, _mailerConfig.FtpHost, _mailerConfig.FtpUser, _mailerConfig.FtpPassword, string.Format("{0}nws{1}.{2}-{3}-recipients.json", pathFtp, _shellSettings.Name, communicationId, pageNum));
        }

        private void SendSettings(object settings, int communicationId) {
            string pathFtp = _mailerConfig.FtpPath;
            string jsonSettings = JsonConvert.SerializeObject(settings);
            SendFtp(jsonSettings, _mailerConfig.FtpHost, _mailerConfig.FtpUser, _mailerConfig.FtpPassword, string.Format("{0}nws{1}.{2}-settings.json", pathFtp, _shellSettings.Name, communicationId));
        }

        private void SendFtp(string contenuto, string host, string usr, string pwd, string fileName) {
            // upload di un file tramite ftp
            using (System.Net.FtpClient.FtpClient client = new System.Net.FtpClient.FtpClient()) {
                client.Host = host;
                client.Credentials = new System.Net.NetworkCredential(usr, pwd);
                client.Connect();
                using (var ftpStream = client.OpenWrite(fileName)) {
                    byte[] buffer = System.Text.ASCIIEncoding.Unicode.GetBytes(contenuto);
                    ftpStream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private Dictionary<string, object> GetSettings(dynamic contentModel, int templateId, NewsletterEditionPart part) {
            var data = new Dictionary<string, object>();

            ParseTemplateContext templatectx = new ParseTemplateContext();
            var template = _templateService.GetTemplate(templateId);
            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);

            var baseUri = new Uri(_orchardServices.WorkContext.CurrentSite.BaseUrl);
            var tenantPrefix = GetTenantUrlPrexix(_shellSettings);

            // Creo un model che ha Content (il contentModel), Urls con alcuni oggetti utili per il template
            // Nel template pertanto Model, diventa Model.Content
            var host = string.Format("{0}://{1}{2}",
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Scheme,
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Host,
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Port == 80
                                        ? string.Empty
                                        : ":" + _orchardServices.WorkContext.HttpContext.Request.Url.Port);
            
            var dynamicModel = new {
                Content = contentModel,
                Urls = new {
                    SubscriptionSubscribe = urlHelper.SubscriptionSubscribe(),
                    SubscriptionUnsubscribe = urlHelper.SubscriptionUnsubscribe(),
                    SubscriptionConfirmSubscribe = urlHelper.SubscriptionConfirmSubscribe(),
                    SubscriptionConfirmUnsubscribe = urlHelper.SubscriptionConfirmUnsubscribe(),
                    BaseUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl,
                    MediaUrl = urlHelper.MediaExtensionsImageUrl(),
                    Domain = host,

                }.ToExpando()
            };
            templatectx.Model = dynamicModel;

            var body = _templateService.ParseTemplate(template, templatectx);

            if (!body.StartsWith("Error On Template")) {
                var subject = template.Subject;
                var smtp = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
                string priority = "L";
                switch (_mailerConfig.MailPriority) {
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

                var baseUrl = baseUri.ToString();
                // token di sicurezza: contiene data e ora (senza minuti e secondi) e id del content item
                var token = string.Format("{0}{1}", DateTime.Now.ToString("yyyyMMddHH"), (contentModel as ContentItem).Id);
                token = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(token));

                //var url = string.Format("{0}/Laser.Orchard.Newsletter/MailerResult?tk={1}", baseUrl, token);  // versione per il GET
                var url = string.Format("{0}/{1}api/Laser.Orchard.Newsletter/MailerResultAPI?tk={2}", baseUrl, tenantPrefix, token);  // versione per il POST

                data.Add("Subject", subject);
                data.Add("Body", body);
                data.Add("Sender", smtp.Address);
                data.Add("Priority", priority);
                data.Add("Url", url);  // url di ritorno per comunicare a Orchard il numero di newsletter inviate con successo
                data.Add("Attachments", ""); // TODO esempio: "[\"prova.pdf\",\"prova.docx\"]" 2016-01-14: per ora non li gestiamo
            }
            return data;
        }

        private string GetTenantUrlPrexix(ShellSettings shellSettings) {
            // calcola il prefix del tenant corrente
            string tenantPath = shellSettings.RequestUrlPrefix ?? "";

            if (tenantPath != "") {
                tenantPath = tenantPath + "/";
            }
            return tenantPath;
        }

        public int GetNextNumber(int newsltterId) {
            var maxNumber = _repositoryNewsletterEdition.Table
                 .Where(w => w.NewsletterDefinitionPartRecord_Id == newsltterId)
                 .Select(s => s.Number)
                 .Max();
            return (maxNumber.HasValue ? maxNumber.Value + 1 : 1);
        }

    }
}