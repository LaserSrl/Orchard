﻿using Laser.Orchard.Commons.Attributes;
using Laser.Orchard.Mobile.Services;
using Orchard;
using Orchard.Environment.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

namespace Laser.Orchard.Mobile.Controllers {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class CountPanelController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IPushGatewayService _pushGatewayService;
        private readonly ISmsCommunicationService _smsCommunicationService;
        public CountPanelController(IOrchardServices orchardServices, IPushGatewayService pushGatewayService) {
            _orchardServices = orchardServices;
            _pushGatewayService = pushGatewayService;
            _orchardServices.WorkContext.TryResolve<ISmsCommunicationService>(out _smsCommunicationService);
        }

        [HttpPost]
        [AdminService]
        public JsonResult GetTotalPush(Int32[] ids, string[] manualRecipients, Int32? idlocalization, Int32? tot) {
            if (manualRecipients != null) {
                manualRecipients = manualRecipients.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            }
            Dictionary<string, string> Total = new Dictionary<string, string>();
            Total.Add("Key", "<i class=\"fa fa-mobile\"></i>");
            if (tot.HasValue) {
                Total.Add("Value", tot.ToString());
            } else {
                IList elenco;
                if (manualRecipients == null || manualRecipients.Length == 0) {
                    elenco = _pushGatewayService.GetPushQueryResult(ids, true);
                } else {
                    elenco = _pushGatewayService.GetPushQueryResultByUserNames(manualRecipients, null, true, "All", true);
                }
                var android = Convert.ToInt64((((Hashtable)(elenco[0]))["Android"]) ?? 0); //elenco.Where(x => x.Device == TipoDispositivo.Android).Count();
                var apple = Convert.ToInt64((((Hashtable)(elenco[0]))["Apple"]) ?? 0);  //elenco.Where(x => x.Device == TipoDispositivo.Apple).Count();
                var win = Convert.ToInt64((((Hashtable)(elenco[0]))["WindowsMobile"]) ?? 0);  //elenco.Where(x => x.Device == TipoDispositivo.WindowsMobile).Count();
                Total.Add("Value", string.Format("{0:#,##0} (<i class=\"fa fa-android\"></i> {1:#,##0}, <i class=\"fa fa-apple\"></i> {2:#,##0}, <i class=\"fa fa-windows\"></i> {3:#,##0})", ((long)(((Hashtable)(elenco[0]))["Tot"])), android, apple, win));
            }
            return Json(Total, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AdminService]
        public JsonResult GetTotalSms(Int32[] ids, string[] manualRecipients, Int32? idlocalization, Int32? tot) {
            manualRecipients = manualRecipients.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            Dictionary<string, string> Total = new Dictionary<string, string>();
            Total.Add("Key", "<i class=\"fa fa-phone\"></i>");
            if (tot.HasValue) {
                Total.Add("Value", tot.ToString());
            } else {
                var elenco = _smsCommunicationService.GetSmsQueryResult(ids, idlocalization, true);
                Total.Add("Value", ((long)(((Hashtable)(elenco[0]))["Tot"])).ToString("#,##0"));
            }
            return Json(Total, JsonRequestBehavior.AllowGet);
        }
    }
}