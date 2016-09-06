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
        private readonly IEnumerable<PaymentBase> _posList;
        private readonly IRepository<PaymentRecord> _repository;
        private readonly IOrchardServices _orchardServices;
        private readonly IModuleService _moduleService;

        public PaymentController(IEnumerable<PaymentBase> posList, IRepository<PaymentRecord> repository, IOrchardServices orchardServices, IModuleService moduleService) {
            _posList = posList;
            _repository = repository;
            _orchardServices = orchardServices;
            _moduleService = moduleService;
        }
        [Themed]
        public ActionResult Pay(string reason, decimal amount, string currency, int itemId = 0) {
            ContentItem item = null;
            List<string> posNames = GetAllPos(); //new List<string>();
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
        private List<string> GetAllPos() {
            List<string> result = new List<string>();
            var type = typeof(PaymentBase);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (assembly.GetName().Name.StartsWith("Laser.Orchard")) {
                    var aux = assembly.DefinedTypes.Where(x => type.IsAssignableFrom(x) && (x.Name != type.Name));
                    result.AddRange(aux.Select(z => z.Name));
                }
            }
            return result;
        }
    }
}