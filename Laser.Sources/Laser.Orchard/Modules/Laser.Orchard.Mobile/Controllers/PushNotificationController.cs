using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.Mobile.ViewModels;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Linq;
using System.Web.Mvc;


namespace Laser.Orchard.Mobile.Controllers {
    [OrchardFeature("Laser.Orchard.PushGateway")]
    public class PushNotificationController : Controller {

        private readonly IPushNotificationService _pushNotificationService;
        private readonly IPushGatewayService _pushGatewayService;
        private readonly INotifier _notifier;
        private readonly IOrchardServices _orchardServices;

        public PushNotificationController(
            IOrchardServices orchardServices
            , IPushNotificationService pushNotificationService
            , IPushGatewayService pushGatewayService
            , INotifier notifier
            ) {
            _orchardServices = orchardServices;
            _pushNotificationService = pushNotificationService;
            _pushGatewayService = pushGatewayService;
            _notifier = notifier;
        }

       

        [System.Web.Mvc.HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, PushSearch search) {
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, search);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, PushSearch search) {
            var AllRecord = _pushGatewayService.SearchPushNotification(search.Expression);
            var totRecord = AllRecord.Count();
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            dynamic pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(totRecord);

            // Generate a list of shapes, restricting by pager parameters
            var list = _orchardServices.New.List();
            list.AddRange(AllRecord.Skip(pager.GetStartIndex())
                                .Take(pager.PageSize)
                // .Select(r => _orchardService.ContentManager.BuildDisplay(r, "ciao"))
                                );
            //   (object) new model {Orders: list, Pager: pagerShape, Admn: hasPermission};

            //var model = Shape.Orders(Orders: list, Pager: pagerShape, Admn: hasPermission, OrderPayedCount: countOrdersNew, Search: search);
            var model = new PushIndex(list, search, pagerShape);
          
            return View((object)model);
            //return View((object)new {
            //    Orders = list,
            //    Pager = pagerShape,
            //    Admn = hasPermission
            //});
        }

        public void Crea() {
            PushNotificationRecord test = new PushNotificationRecord();
            test.DataInserimento = DateTime.Today;
            test.DataModifica = DateTime.Today;
            test.Device = TipoDispositivo.Apple;
            //test.Id = 0;
            test.Validated = true;
            test.Language = "ITA";
            test.Produzione = true;
            test.Token = "awerwqdasfafsa";
            test.UUIdentifier = "iosonounid";
            _pushNotificationService.StorePushNotification(test);
            
        }

        //[Admin]
        //public ActionResult Send() {
        //    _PushNotificationService.SendPush(1, "Prova con Orchard");
        //    return View();
        //}

    }
}