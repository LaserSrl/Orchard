using Laser.Orchard.Caligoo.Models;
using Laser.Orchard.Caligoo.Services;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.Caligoo.Controllers {
    public class UserApiController : ApiController {
        private readonly ICaligooService _caliggoService;
        private readonly IWorkflowManager _workflowManager;
        public UserApiController(ICaligooService caliggoService, IWorkflowManager workflowManager) {
            _caliggoService = caliggoService;
            _workflowManager = workflowManager;
        }
        [HttpPost]
        public HttpResponseMessage Post(JObject message) {
            var esito = "";
            var msgObj = message.ToObject<LoginLogoutEventMessage>();
            var contact = _caliggoService.GetContactId(msgObj.CaligooUserId);
            if (contact == null) {
                contact = _caliggoService.CreateContact(msgObj.CaligooUserId, null, null, null, null);
            }
            // trigger workflow event
            var eventType = "";
            if (string.Equals(msgObj.EventType, "login_event", StringComparison.InvariantCultureIgnoreCase)) {
                eventType = "login";
            } else if (string.Equals(msgObj.EventType, "logout_event", StringComparison.InvariantCultureIgnoreCase)) {
                eventType = "logout";
            }
            _workflowManager.TriggerEvent("CaligooLoginLogoutEvent", contact, () => new Dictionary<string, object> { { "Event", eventType}, { "Content", contact } });
            // create response and return
            esito = string.Format("{{ \"contact_id\":{0} }}", contact.Id);
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new System.Net.Http.StringContent(esito, Encoding.UTF8, "text/plain");
            return result;
        }
    }
}