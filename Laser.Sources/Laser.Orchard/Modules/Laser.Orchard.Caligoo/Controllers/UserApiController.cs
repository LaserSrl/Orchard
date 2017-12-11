using Laser.Orchard.Caligoo.Models;
using Laser.Orchard.Caligoo.Services;
using Newtonsoft.Json.Linq;
using Orchard;
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
        public UserApiController(ICaligooService caliggoService) {
            _caliggoService = caliggoService;
        }
        [HttpPost]
        public HttpResponseMessage Post(JObject message) {
            var esito = "";
            var aux = message.ToObject<LoginLogoutEventMessage>();
            //esito = aux.StartDateTime.ToString("yyyy-MM-dd HH:mm:ss");

            var contactId = _caliggoService.GetContactId(aux.CaligooUserId);
            if (contactId == 0) {
                contactId = _caliggoService.CreateContact(aux.CaligooUserId, null, null, null, null);
            }

            // create response and return
            esito = string.Format("{{ \"contact_id\":{0} }}", contactId);
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new System.Net.Http.StringContent(esito, Encoding.UTF8, "text/plain");
            return result;
        }
    }
}