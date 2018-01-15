using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.MultiStepAuthentication.Models;
using Orchard.Security;
using Orchard;
using Orchard.Localization;
using Orchard.Messaging.Services;
using Orchard.Email.Services;
using Orchard.Environment.Extensions;
using Orchard.ContentManagement;
using Laser.Orchard.StartupConfig.Models;
using Orchard.Localization.Models;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.Logging;

namespace Laser.Orchard.MultiStepAuthentication.Services {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class DefaultNonceLinkDeliveryByMailService : IOTPDeliveryService {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly INonceLinkProvider _nonceLinkProvider;
        private readonly IMessageService _messageService;
        private readonly IContentManager _contentManager;
        private readonly ITemplateService _templateService;
        public ILogger Logger{ get; set; }
        public DefaultNonceLinkDeliveryByMailService(
            IWorkContextAccessor workContextAccessor,
            INonceLinkProvider nonceLinkProvider,
            IMessageService messageService,
            IContentManager contentManager,
            ITemplateService templateService) {
            _contentManager = contentManager;
            _workContextAccessor = workContextAccessor;
            _nonceLinkProvider = nonceLinkProvider;
            _messageService = messageService;
            _templateService = templateService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T;

        public DeliveryChannelType ChannelType {
            get { return DeliveryChannelType.Email; }
            set { }
        }

        public int Priority {
            get { return 0; }
            set { }
        }

        public bool TrySendOTP(OTPRecord otp, IUser user) {
            return TrySendOTP(otp, user, null);
        }

        public bool TrySendOTP(OTPRecord otp, IUser user,FlowType? flow) {
            if (otp == null // parameter validation
                || user == null
                || otp.UserRecord.UserName != user.UserName) {
                return false;
            }

            var currentSite = _workContextAccessor.GetContext().CurrentSite;
            var data = new Dictionary<string, object>();

            // get link
            var link = _nonceLinkProvider.FormatURI(otp.Password, flow);
            //          var userlang = _workContextAccessor.GetContext().CurrentSite.SiteCulture;
            //          if (user.ContentItem.As<FavoriteCulturePart>()!=null)
            //              userlang =  user.ContentItem.As<FavoriteCulturePart>().Culture;
            // deve vedere se ha un tenplate, se è in lingua allora fa il ragionamento

            // cambiare il nome della feature aggiungendo template per questa interfaccia

            // template with this culture
            //         var nonceTemplate = _contentManager.Query<NonceTemplateSettingsPart>().List().FirstOrDefault(x => x.ContentItem.As<LocalizationPart>().Culture.Culture.Equals(userlang));
            //        if (nonceTemplate == null) {
            if (true) { 
                       data.Add("Subject", T("{0} - Login", currentSite.SiteName).Text);
                data.Add("Body", T("<html><body>To login on \"{0}\", please open the following link: <a href=\"{1}\">Login</a></body></html>", currentSite.SiteName, link).Text);
                data.Add("Recipients", user.Email);

                
                      _messageService.Send(SmtpMessageChannel.MessageType, data);
                Logger.Error("NonceTemplatePart must be added to CustomTemplate used for nonce");
                return false;
            }
            else {
                //dynamic contentModel = new {
                //    ContentItem = user,
                //    Link=link
                //};
                //var templateId = nonceTemplate.Id;
                //List<string> sendTo = new List<string>(new string[] { user.Email });
                //_templateService.SendTemplatedEmail(contentModel, templateId, sendTo, null);
                //return true;
            }
            //               .List().Select(user => ((dynamic)user).ProfilePart).Select(user => new { user.FirstName, user.LastName })
            //foreach template con una parte, nonce template
            //cerco quello in lingua
            //mando email con quel template

            // fill in email dictionary
      //      data.Add("Subject", T("{0} - Login", currentSite.SiteName).Text);
      //      data.Add("Body", T("<html><body>To login on \"{0}\", please open the following link: <a href=\"{1}\">Login</a></body></html>", currentSite.SiteName, link).Text);
      //      data.Add("Recipients", user.Email);

            //send email
      //      _messageService.Send(SmtpMessageChannel.MessageType, data);

          //  return true;
        }
    }
}