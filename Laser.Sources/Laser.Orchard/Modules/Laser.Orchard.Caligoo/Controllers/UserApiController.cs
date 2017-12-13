using Laser.Orchard.Caligoo.Models;
using Laser.Orchard.Caligoo.Services;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Newtonsoft.Json.Linq;
using Orchard.Workflows.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace Laser.Orchard.Caligoo.Controllers {
    public class UserApiController : ApiController {
        private readonly ICaligooService _caligooService;
        private readonly IWorkflowManager _workflowManager;
        private readonly IUtilsServices _utilsServices;
        public UserApiController(ICaligooService caligooService, IWorkflowManager workflowManager, IUtilsServices utilsServices) {
            _caligooService = caligooService;
            _workflowManager = workflowManager;
            _utilsServices = utilsServices;
        }
        public Response Post(JObject message) {
            var msgObj = message.ToObject<LoginLogoutEventMessage>();
            var contact = _caligooService.GetContact(msgObj.CaligooUserId);
            if (contact == null) {
                contact = _caligooService.CreateContact(msgObj);
            }
            // trigger workflow event
            var eventType = "";
            if (string.Equals(msgObj.EventType, "login_event", StringComparison.InvariantCultureIgnoreCase)) {
                eventType = "login";
            } else if (string.Equals(msgObj.EventType, "logout_event", StringComparison.InvariantCultureIgnoreCase)) {
                eventType = "logout";
            }
            _workflowManager.TriggerEvent("CaligooLoginLogoutEvent", contact, () => new Dictionary<string, object> { { "Event", eventType}, { "Content", contact } });
            return _utilsServices.GetResponse(StartupConfig.ViewModels.ResponseType.Success, "", new { contact_id = contact.Id });
        }
        /// <summary>
        /// Metodo utile solo per test vari
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage Put(JObject message) {
            //_caligooService.CaligooLogin("", "");
            //_caligooService.CaligooLogin("", "");

            //var msgObj = message.ToObject<CaligooUserMessage>();
            //var aux = msgObj.CaligooUserName;

            var msgObj = message.ToObject<LocationMessage>();
            var aux = msgObj.Address;
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}