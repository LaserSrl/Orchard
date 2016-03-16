﻿using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Mobile.Controllers
{
    public class CountPanelController : Controller
    {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly ISmsCommunicationService _smsCommunicationService;
        public CountPanelController(IPushNotificationService pushNotificationService, ISmsCommunicationService smsCommunicationService)
        {
            _pushNotificationService = pushNotificationService;
            _smsCommunicationService = smsCommunicationService;
        }
        [HttpGet]
        public JsonResult GetTotalPush(Int32[] ids, Int32? idlocalization, Int32? tot)
        {
            Dictionary<string, string> Total = new Dictionary<string, string>();
            Total.Add("Key", "<i class=\"fa fa-mobile\"></i>");
            if (tot.HasValue)
            {
                Total.Add("Value", tot.ToString());
            }
            else
            {
                var elenco = _pushNotificationService.GetPushQueryResult(ids);
                var android = elenco.Where(x => x.Device == TipoDispositivo.Android).Count();
                var apple = elenco.Where(x => x.Device == TipoDispositivo.Apple).Count();
                var win = elenco.Where(x => x.Device == TipoDispositivo.WindowsMobile).Count();
                Total.Add("Value", string.Format("{0} (<i class=\"fa fa-android\"></i> {1}, <i class=\"fa fa-apple\"></i> {2}, <i class=\"fa fa-windows\"></i> {3})", elenco.Count, android, apple, win));
            }
            return Json(Total, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetTotalSms(Int32[] ids, Int32? idlocalization, Int32? tot)
        {
            Dictionary<string, string> Total = new Dictionary<string, string>();
            Total.Add("Key", "<i class=\"fa fa-phone\"></i>");
            if (tot.HasValue)
            {
                Total.Add("Value", tot.ToString());
            }
            else
            {
                var elenco = _smsCommunicationService.GetSmsQueryResult(ids, idlocalization);
                Total.Add("Value", elenco.Count.ToString());
            }
            return Json(Total, JsonRequestBehavior.AllowGet);
        }
    }
}