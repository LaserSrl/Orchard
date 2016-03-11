using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Laser.Orchard.TemplateManagement.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Email.Models;
using Orchard.Environment.Extensions;
using Laser.Orchard.StartupConfig.Extensions;
using Laser.Orchard.Commons.Extensions;
using Orchard.Messaging.Services;
using Orchard.Email.Services;
using Orchard.JobsQueue.Services;
using RazorEngine.Templating;
using Newtonsoft.Json;
using Orchard.UI.Notify;


namespace Laser.Orchard.TemplateManagement.Services {
    public interface ITemplateService : IDependency {
        IEnumerable<TemplatePart> GetLayouts();
        IEnumerable<TemplatePart> GetTemplates();
        IEnumerable<TemplatePart> GetTemplatesWithLayout(int LayoutIdSelected);
        TemplatePart GetTemplate(int id);
        string ParseTemplate(TemplatePart template, ParseTemplateContext context);
        IEnumerable<IParserEngine> GetParsers();
        IParserEngine GetParser(string id);
        IParserEngine SelectParser(TemplatePart template);
        bool SendTemplatedEmail(dynamic contentModel, int templateId, IEnumerable<string> sendTo, IEnumerable<string> bcc, object viewBag = null, bool queued = true);
   }

    [OrchardFeature("Laser.Orchard.TemplateManagement")]
    public class TemplateService : Component, ITemplateService {
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IParserEngine> _parsers;
        private readonly IOrchardServices _services;
        private readonly IMessageService _messageService;
        private readonly IJobsQueueService _jobsQueueService;
        private readonly INotifier _notifier;

        public TemplateService(INotifier notifier,IEnumerable<IParserEngine> parsers, IOrchardServices services, IMessageService messageService, IJobsQueueService jobsQueueService) {
            _contentManager = services.ContentManager;
            _parsers = parsers;
            _services = services;
            _messageService = messageService;
            _jobsQueueService = jobsQueueService;
            _notifier = notifier;
        }

        public IEnumerable<TemplatePart> GetLayouts() {
            return _contentManager.Query<TemplatePart, TemplatePartRecord>().Where(x => x.IsLayout).List();
        }

        public IEnumerable<TemplatePart> GetTemplates() {
            return _contentManager.Query<TemplatePart, TemplatePartRecord>().Where(x => !x.IsLayout).List();
        }

        public IEnumerable<TemplatePart> GetTemplatesWithLayout(int LayoutIdSelected) {
            return _contentManager.Query<TemplatePart, TemplatePartRecord>().Where(x => x.LayoutIdSelected == LayoutIdSelected).List();
        }

        public TemplatePart GetTemplate(int id) {
            return _contentManager.Get<TemplatePart>(id);
        }

        public string ParseTemplate(TemplatePart template, ParseTemplateContext context) {
            var parser = SelectParser(template);
            return parser.ParseTemplate(template, context);
        }

        public IParserEngine GetParser(string id) {
            return _parsers.SingleOrDefault(x => x.Id == id);
        }

        public IParserEngine SelectParser(TemplatePart template) {
            var parserId = template.DefaultParserIdSelected;
            IParserEngine parser = null;

            if (!string.IsNullOrWhiteSpace(parserId)) {
                parser = GetParser(parserId);
            }

            if (parser == null) {
                parserId = _services.WorkContext.CurrentSite.As<SiteSettingsPart>().DefaultParserIdSelected;
                parser = GetParser(parserId);
            }

            return parser ?? _parsers.First();
        }

        public IEnumerable<IParserEngine> GetParsers() {
            return _parsers;
        }

        public bool SendTemplatedEmail(dynamic contentModel, int templateId, IEnumerable<string> sendTo, IEnumerable<string> bcc, object viewBag=null, bool queued = true) {
            ParseTemplateContext templatectx = new ParseTemplateContext();
            var template = GetTemplate(templateId);
            var urlHelper = new UrlHelper(_services.WorkContext.HttpContext.Request.RequestContext);

            // Creo un model che ha Content (il contentModel), Urls con alcuni oggetti utili per il template
            // Nel template pertanto Model, diventa Model.Content
            var host = string.Format("{0}://{1}{2}",
                                    _services.WorkContext.HttpContext.Request.Url.Scheme,
                                    _services.WorkContext.HttpContext.Request.Url.Host,
                                    _services.WorkContext.HttpContext.Request.Url.Port == 80
                                        ? string.Empty
                                        : ":" + _services.WorkContext.HttpContext.Request.Url.Port);
            var dynamicModel = new {
                WorkContext = _services.WorkContext,
                Content = contentModel,
                Urls = new {
                    //SubscriptionSubscribe = urlHelper.SubscriptionSubscribe(),
                    //SubscriptionUnsubscribe = urlHelper.SubscriptionUnsubscribe(),
                    //SubscriptionConfirmSubscribe = urlHelper.SubscriptionConfirmSubscribe(),
                    //SubscriptionConfirmUnsubscribe = urlHelper.SubscriptionConfirmUnsubscribe(),
                    BaseUrl = _services.WorkContext.CurrentSite.BaseUrl,
                    MediaUrl = urlHelper.MediaExtensionsImageUrl(),
                    Domain = host,

                }.ToExpando()
            };
            templatectx.Model = dynamicModel;
            var razorviewBag = viewBag;
            RazorEngine.Templating.DynamicViewBag vb = new DynamicViewBag();
            try {
                foreach (string key in ((Dictionary<string, object>)viewBag).Keys) {
                    vb.AddValue(key, ((IDictionary<string, object>)viewBag)[key]);
                }
            }
            catch { }
            templatectx.ViewBag = vb;
            var body = ParseTemplate(template, templatectx);
            if (body.StartsWith("Error On Template")) {
                _notifier.Add(NotifyType.Error,T("Error on template, mail not sended"));
                return false;
            }

            var data = new Dictionary<string, object>();
            var smtp = _services.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            var recipient = sendTo != null ? sendTo : new List<string> { smtp.Address };
            data.Add("Subject", template.Subject);
            data.Add("Body", body);
            data.Add("Recipients", String.Join(",", recipient));
            if (bcc != null) {
                data.Add("Bcc", String.Join(",", bcc));
            }
            //var watch = Stopwatch.StartNew();
            //int msgsent = 0;

            //for(int i=0;i<20;i++) {
            //    msgsent++;
            //    data["Subject"] = msgsent.ToString();
            //    data["Bcc"] = "lorenzo.frediani@laser-group.com";
            //    _messageService.Send(SmtpMessageChannel.MessageType, data);
            //}
            //watch.Stop();
            //_notifier.Add(NotifyType.Information, T("Sent " + msgsent.ToString()+" email in Milliseconds:" + watch.ElapsedMilliseconds.ToString()));            
            if (!queued) {
                _messageService.Send(SmtpMessageChannel.MessageType, data);
            } else {
                var priority = 0;//normal 50 to hight -50 to low

                _jobsQueueService.Enqueue("IMessageService.Send", new { type = SmtpMessageChannel.MessageType, parameters = data }, priority);
            }

            return true;
        }


    }
}