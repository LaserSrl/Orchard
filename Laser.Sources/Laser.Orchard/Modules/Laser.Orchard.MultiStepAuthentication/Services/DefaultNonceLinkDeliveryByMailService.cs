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

namespace Laser.Orchard.MultiStepAuthentication.Services {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class DefaultNonceLinkDeliveryByMailService : IOTPDeliveryService {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly INonceLinkProvider _nonceLinkProvider;
        private readonly IMessageService _messageService;

        public DefaultNonceLinkDeliveryByMailService(
            IWorkContextAccessor workContextAccessor,
            INonceLinkProvider nonceLinkProvider,
            IMessageService messageService) {

            _workContextAccessor = workContextAccessor;
            _nonceLinkProvider = nonceLinkProvider;
            _messageService = messageService;

            T = NullLocalizer.Instance;
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
            if (otp == null // paarmeter validation
                || user == null
                || otp.UserRecord.UserName != user.UserName) {
                return false;
            }

            var currentSite = _workContextAccessor.GetContext().CurrentSite;
            var data = new Dictionary<string, object>();

            // get link
            var link = _nonceLinkProvider.FormatURI(otp.Password);
            // fill in email dictionary
            data.Add("Subject", T("{0} - Login", currentSite.SiteName).Text);
            data.Add("Body", T("<html><body>To login on \"{0}\", please open the following link: <a href=\"{1}\">Login</a></body></html>", currentSite.SiteName, link).Text);
            data.Add("Recipients", user.Email);

            //send email
            _messageService.Send(SmtpMessageChannel.MessageType, data);

            return true;
        }
    }
}