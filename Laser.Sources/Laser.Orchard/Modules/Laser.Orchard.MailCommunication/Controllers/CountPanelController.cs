﻿using Laser.Orchard.Commons.Attributes;
using Laser.Orchard.MailCommunication.Services;
using Orchard.Environment.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Laser.Orchard.MailCommunication.Controllers {

    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class CountPanelController : Controller {
        private readonly IMailCommunicationService _mailCommunicationService;
        public CountPanelController(IMailCommunicationService mailCommunicationService) {
            _mailCommunicationService = mailCommunicationService;
        }

        [HttpPost]
        [AdminService]
        public JsonResult GetTotal(Int32[] ids, string[] manualRecipients, int? contentId, Int32? idlocalization, Int32? tot) {
         //   Int32[] Ids = ids.Split(',').Where(tag => !string.IsNullOrEmpty(tag)).Select(x => int.Parse(x)).ToArray();
            Dictionary<string,string> Total=new Dictionary<string,string>();
            Total.Add("Key","<i class=\"fa fa-envelope\"></i>");
            if (tot.HasValue)
            {
                Total.Add("Value", tot.ToString());
            }
            else
            {
                var elenco = _mailCommunicationService.GetMailQueryResult(ids, idlocalization, true, contentId.HasValue ? contentId.Value : 0);
                Total.Add("Value", ((long)(((Hashtable)(elenco[0]))["Tot"])).ToString("#,##0"));
            }
            return Json(Total, JsonRequestBehavior.AllowGet);
        }
    }
}