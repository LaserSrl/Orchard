using Laser.Orchard.Commons.Extensions;
using Laser.Orchard.StartupConfig.Extensions;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Email.Models;
using Orchard.Email.Services;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Messaging.Services;
using Orchard.Security;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.TemplateManagement.Activities {
    [OrchardFeature("Laser.Orchard.TemplateEmailActivities")]
    public class MailActivity : Task {
        private readonly IOrchardServices _orchardServices;
        private readonly IMessageService _messageService;
        private readonly IMembershipService _membershipService;
        private readonly ITemplateService _templateServices;


        public const string MessageType = "ActionTemplatedEmail";

        public MailActivity(
            IMessageService messageService,
            IOrchardServices orchardServices,
            IMembershipService membershipService,
            ITemplateService templateServices) {
            _messageService = messageService;
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _templateServices = templateServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Sent") };
        }

        public override string Form {
            get {
                return "ActivityActionTemplatedEmail";
            }
        }

        public override LocalizedString Category {
            get { return T("Messaging"); }
        }

        public override string Name {
            get { return "SendTemplatedEmail"; }
        }


        public override LocalizedString Description {
            get { return T("Sends an e-mail using a template to a specific user."); }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            string recipient = activityContext.GetState<string>("Recipient");

            var properties = new Dictionary<string, string> {
                {"Body", activityContext.GetState<string>("Body")}, 
                {"Subject", activityContext.GetState<string>("Subject")},
                {"RecipientOther",activityContext.GetState<string>("RecipientOther")},
                {"EmailTemplate",activityContext.GetState<string>("EmailTemplate")}
            };
            List<string> sendTo = new List<string>();
            var templateId = 0;
            int.TryParse(properties["EmailTemplate"], out templateId);
            var contentVersion = workflowContext.Content.ContentItem.Version;
            dynamic contentModel = new {
                ContentItem = _orchardServices.ContentManager.GetAllVersions(workflowContext.Content.Id).Single(w => w.Version == contentVersion), // devo ricalcolare il content altrimenti MediaParts (e forse tutti i lazy fields!) è null!
                FormCollection = _orchardServices.WorkContext.HttpContext.Request.Form,
                QueryStringCollection = _orchardServices.WorkContext.HttpContext.Request.QueryString
            };
            if (recipient == "owner") {
                var content = workflowContext.Content;
                if (content.Has<CommonPart>()) {
                    var owner = content.As<CommonPart>().Owner;
                    if (owner != null && owner.ContentItem != null && owner.ContentItem.Record != null) {
                        sendTo.AddRange(SplitEmail(owner.As<IUser>().Email));
                        // _messageManager.Send(owner.ContentItem.Record, MessageType, "email", properties);
                    }
                    sendTo.AddRange(SplitEmail(owner.As<IUser>().Email));
                }
            } else if (recipient == "author") {
                var user = _orchardServices.WorkContext.CurrentUser;

                // can be null if user is anonymous
                if (user != null && !String.IsNullOrWhiteSpace(user.Email)) {
                    sendTo.AddRange(SplitEmail(user.Email));
                }
            } else if (recipient == "admin") {
                var username = _orchardServices.WorkContext.CurrentSite.SuperUser;
                var user = _membershipService.GetUser(username);

                // can be null if user is no super user is defined
                if (user != null && !String.IsNullOrWhiteSpace(user.Email)) {
                    sendTo.AddRange(SplitEmail(user.As<IUser>().Email));
                }
            } else if (recipient == "other") {
                sendTo.AddRange(SplitEmail(activityContext.GetState<string>("RecipientOther")));
            }
            SendEmail(contentModel, templateId, sendTo, null);
            yield return T("Sent");
        }

        private static IEnumerable<string> SplitEmail(string commaSeparated) {
            if (commaSeparated == null) return null;
            return commaSeparated.Split(new[] { ',', ';' });
        }

        private void SendEmail(dynamic contentModel, int templateId, IEnumerable<string> sendTo, IEnumerable<string> bcc) {
            ParseTemplateContext templatectx = new ParseTemplateContext();
            var template = _templateServices.GetTemplate(templateId);
            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);

            // Creo un model che ha Content (il contentModel), Urls con alcuni oggetti utili per il template
            // Nel template pertanto Model, diventa Model.Content
            var host = string.Format("{0}://{1}{2}",
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Scheme,
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Host,
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Port == 80
                                        ? string.Empty
                                        : ":" + _orchardServices.WorkContext.HttpContext.Request.Url.Port);
            var dynamicModel = new {
                WorkContext = _orchardServices.WorkContext,
                Content = contentModel,
                Urls = new {
                    MediaUrl = urlHelper.MediaExtensionsImageUrl(),
                    Domain = host,

                }.ToExpando()
            };
            templatectx.Model = dynamicModel;
            var body = _templateServices.ParseTemplate(template, templatectx);
            var data = new Dictionary<string, object>();
            var smtp = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            var recipient = sendTo != null ? sendTo : new List<string> { smtp.Address };

            data.Add("Subject", template.Subject);
            data.Add("Body", body);
            data.Add("Recipients", String.Join(",", recipient));
            if (bcc != null) {
                data.Add("Bcc", String.Join(",", bcc));
            }
            _messageService.Send(SmtpMessageChannel.MessageType, data);

        }

    }
}