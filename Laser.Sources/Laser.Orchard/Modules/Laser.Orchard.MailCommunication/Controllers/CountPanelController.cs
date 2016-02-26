using Laser.Orchard.Services.MailCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.MailCommunication.Controllers {

    public class CountPanelController : Controller {
        private readonly IMailCommunicationService _mailCommunicationService;
        public CountPanelController(IMailCommunicationService mailCommunicationService) {
            _mailCommunicationService = mailCommunicationService;
        }
        [HttpGet]
        public JsonResult GetTotal(Int32[] ids,Int32? idlocalization) {
            Dictionary<string,string> Total=new Dictionary<string,string>();
            Total.Add("Key","Mail");
            Total.Add("Total", _mailCommunicationService.GetMailQueryResult(ids, idlocalization).Count.ToString());
            return Json(Total, JsonRequestBehavior.AllowGet);
        }
    }
}