using Laser.Orchard.Services.MailCommunication;
using System;
using System.Collections;
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
        public JsonResult GetTotal(Int32[] ids, Int32? idlocalization, Int32? tot) {
         //   Int32[] Ids = ids.Split(',').Where(tag => !string.IsNullOrEmpty(tag)).Select(x => int.Parse(x)).ToArray();
            Dictionary<string,string> Total=new Dictionary<string,string>();
            Total.Add("Key","<i class=\"fa fa-envelope\"></i>");
            if (tot.HasValue)
            {
                Total.Add("Value", tot.ToString());
            }
            else
            {
                var elenco = _mailCommunicationService.GetMailQueryResult(ids, idlocalization, true);
                Total.Add("Value", ((long)(((Hashtable)(elenco[0]))["Tot"])).ToString("#,##0"));
            }
            return Json(Total, JsonRequestBehavior.AllowGet);
        }
    }
}