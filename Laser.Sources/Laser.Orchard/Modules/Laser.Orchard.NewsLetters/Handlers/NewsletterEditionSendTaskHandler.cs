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
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Web.Routing;
using Orchard.Logging;


namespace Laser.Orchard.NewsLetters.Handlers {
    public class NewsletterEditionSendTaskHandler : IScheduledTaskHandler {

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly INewsletterServices _newslServices;
        private readonly ITemplateService _templateService;
        private readonly IRepository<NewsletterEditionPartRecord> _repositoryNewsletterEdition;
        private readonly ShellSettings _shellSettings;
        
        private MailerSiteSettingsPart _mailerConfig;
        
        private const string TaskType = "Laser.Orchard.NewsLetters.SendEdition.Task";

        public NewsletterEditionSendTaskHandler(IContentManager contentManager, IOrchardServices orchardServices,
                                                INewsletterServices newslServices, ITemplateService templateService,
                                                ShellSettings shellSettings,
                                                IRepository<NewsletterEditionPartRecord> repositoryNewsletterEdition) {
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _newslServices = newslServices;
            _templateService = templateService;
            _shellSettings = shellSettings;
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

                if (items != null) {
                    foreach (var item in items) {
                        var ids = ("," + item.AttachToNextNewsletterIds + ",").Replace("," + part.NewsletterDefinitionPartRecord_Id + ",", "");
                        item.AttachToNextNewsletterIds = ids;
                    }
                }

                _contentManager.Publish(context.Task.ContentItem);
            }
            else {
                Logger.Error(T("Error parsing mail template.").Text);
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
            //var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext); 

            var baseUri = new Uri(_orchardServices.WorkContext.CurrentSite.BaseUrl);

            var host = string.Format("{0}://{1}{2}",
                                    baseUri.Scheme,
                                    baseUri.Host,
                                    baseUri.Port == 80 ? string.Empty : ":" + baseUri.Port);

            // Ricostruisco urlHelper senza usare HttpContext
            var httpRequest = new HttpRequest("/", _orchardServices.WorkContext.CurrentSite.BaseUrl, "");
            var httpResponse = new HttpResponse(new StringWriter());
            var httpContext = new HttpContext(httpRequest, httpResponse);
            var httpContextBase = new HttpContextWrapper(httpContext);

            var virtualRequestContext = new RequestContext(httpContextBase, new RouteData());
            var urlHelper = new UrlHelper(virtualRequestContext);

            // Creo un model che ha Content (il contentModel), Urls con alcuni oggetti utili per il template
            // Nel template pertanto Model, diventa Model.Content
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

                data.Add("Subject", subject);
                data.Add("Body", body);
                data.Add("Sender", smtp.Address);
                data.Add("Priority", priority);
            }
            return data;
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