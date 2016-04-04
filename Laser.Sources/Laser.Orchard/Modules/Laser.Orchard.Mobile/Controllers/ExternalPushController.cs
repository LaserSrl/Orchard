using Laser.Orchard.Mobile.Services;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.Mobile.Controllers {
    [WebApiKeyFilter(true)]
    [OrchardFeature("Laser.Orchard.Mobile.ExternalPush")]
    public class ExternalPushController : ApiController {
        private readonly IPushNotificationService _pushService;
        public class PushRequest {
            public string Text { get; set; }
            public string DevType { get; set; }
            public bool Prod { get; set; }
        }

        public ExternalPushController(IPushNotificationService pushService) {
            _pushService = pushService;
        }

        public HttpResponseMessage Post(PushRequest req) {
            HttpResponseMessage message = null;
            try {
                _pushService.SendPushService(req.Prod, req.DevType, 0, "", req.Text, req.Text, "", req.Text, null);
                message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                message.Content = new StringContent("OK");
            }
            catch(Exception ex) {
                message = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                message.Content = new StringContent(ex.Message);
            }
            return message;
        }
    }
}