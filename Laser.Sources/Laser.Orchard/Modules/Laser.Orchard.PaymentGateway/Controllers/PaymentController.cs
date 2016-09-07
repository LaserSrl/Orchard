using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Modules.Services;
using Orchard.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    public class PaymentController : Controller {
        private readonly IRepository<PaymentRecord> _repository;
        private readonly IOrchardServices _orchardServices;
        private readonly IEnumerable<IPosService> _posServices;

        public PaymentController(IRepository<PaymentRecord> repository, IOrchardServices orchardServices, IEnumerable<IPosService> posServices) {
            _repository = repository;
            _orchardServices = orchardServices;
            _posServices = posServices;
        }
        [Themed]
        public ActionResult Pay(string reason, decimal amount, string currency, int itemId = 0) {
            ContentItem item = null;
            List<string> posNames = _posServices.Select(x => x.GetPosName()).ToList(); //GetAllPos(); //new List<string>();
            //posNames.Add("Dummy"); // _posList.Select(x => x.PosName).ToList();
            string featurePrefix = "Laser.Orchard.PaymentPos";
            //posNames = _moduleService.GetAvailableFeatures().Where(x => x.Descriptor.Id.StartsWith(featurePrefix)).Select(x => x.Descriptor.Id.Substring(featurePrefix.Length)).ToList();
            if (itemId > 0){
                item = _orchardServices.ContentManager.Get(itemId);
            }
            PaymentVM model = new PaymentVM {
                Reason = reason,
                Amount = amount,
                Currency = currency,
                PosList = posNames,
                ContentItem = item
            };
            return View("Pay", model);
        }
        [Themed]
        public ActionResult Info(int paymentId) {
            var payment = _repository.Get(paymentId);
            return View("Info", payment);
        }
    }
}