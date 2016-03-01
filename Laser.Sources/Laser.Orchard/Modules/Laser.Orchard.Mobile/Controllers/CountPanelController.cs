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
        public CountPanelController(IPushNotificationService pushNotificationService)
        {
            _pushNotificationService = pushNotificationService;
        }
        [HttpGet]
        public JsonResult GetTotal(Int32[] ids, Int32? idlocalization, Int32? tot)
        {
            Dictionary<string, string> Total = new Dictionary<string, string>();
            Total.Add("Key", "<i class=\"fa fa-mobile\"></i>");
            if (tot.HasValue)
            {
                Total.Add("Value", tot.ToString());
            }
            else
            {
                Total.Add("Value", _pushNotificationService.GetPushQueryResult(ids).Count.ToString());
            }
            return Json(Total, JsonRequestBehavior.AllowGet);
        }
    }
}