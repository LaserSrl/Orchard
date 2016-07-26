using Laser.Orchard.Mobile.Services;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.Controllers {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class ContactListController : Controller {
        private readonly IPushGatewayService _pushService;

        public ContactListController(IPushGatewayService pushService) {
            _pushService = pushService;
        }

        [HttpPost]
        public JsonResult Search(string nameFilter) {
            List<FoundContact> result = new List<FoundContact>();
            ContactsArray array = new ContactsArray();
            var contatti = _pushService.GetContactsWithDevice(nameFilter);
            foreach (var contact in contatti) {
                result.Add(new FoundContact { name = Convert.ToString(contact["Title"]), num= Convert.ToInt32(contact["NumDevice"]) });
            }
            array.elenco = result.ToArray();
            return Json(array);
        }

        private class ContactsArray {
            public FoundContact[] elenco { get; set; }
        }

        private class FoundContact {
            public string name { get; set; }
            public int num { get; set; }
        }
    }
}