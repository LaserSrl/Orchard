using Laser.Orchard.Mobile.Services;
using Orchard;
using Orchard.Core.Title.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.Mobile.Controllers {
    public class ContactsListApiController : ApiController {
        private readonly IPushGatewayService _pushService;

        public ContactsListApiController(IPushGatewayService pushService) {
            _pushService = pushService;
        }

        public HttpResponseMessage Get(string nameFilter) {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            StringBuilder sb = new StringBuilder();
            var contatti = _pushService.GetContactsWithDevice(nameFilter);
            sb.Append("{\"elenco\":[");
            bool insertComma = false;
            foreach (var contact in contatti) {
                if (insertComma) {
                    sb.Append(",");
                }
                sb.AppendFormat("{{\"name\":\"{0}\", \"num\":{1}}}", contact["Title"], contact["NumDevice"]);
                insertComma = true;
            }
            sb.Append("]}");
            result.Content = new System.Net.Http.StringContent(sb.ToString(), Encoding.UTF8, "application/json");
            return result;
        }
    }
}