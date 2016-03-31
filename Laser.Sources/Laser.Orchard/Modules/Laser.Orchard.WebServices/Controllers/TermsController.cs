using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Taxonomies.Models;

namespace Laser.Orchard.WebServices.Controllers {
    public class TermsController : Controller {
        private readonly IContentManager _contentManager;
        public TermsController(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        [HttpGet]
        public JsonResult GetIconsIds() {
            var items = _contentManager.Query<TermPart, TermPartRecord>().List();
            var listIconIds = new List<int>();
            foreach (dynamic item in items) {
                if (item.Icon != null && ((int[])item.Icon.Ids).Length > 0) {
                    listIconIds = listIconIds.Union((int[])item.Icon.Ids).ToList();
                }
            }
            return Json(listIconIds.ToArray(), JsonRequestBehavior.AllowGet);
        }
    }
}