using Laser.Orchard.PaymentCartaSi.Extensions;
using Laser.Orchard.PaymentCartaSi.Models;
using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentCartaSi.Services {
    public class CartaSiPosService : PosServiceBase, ICartaSiTransactionService {

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public CartaSiPosService(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler) :
            base(orchardServices, repository, paymentEventHandler) {

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public override string GetPosName() {
            return Constants.PosName;
        }
        public override string GetSettingsControllerName() {
            return "Admin";
        }
        /// <summary>
        /// This gets called by the "general" payment services.
        /// </summary>
        /// <param name="paymentId">The id corresponding to a <type>PaymentRecord</type> for the transaction we want to start.</param>
        /// <returns>The url corresponding to an action that will start the CartaSì transaction </returns>
        public override string GetPosUrl(int paymentId) {
            //create the url for the controller action that takes care of the redirect, passing the id as parameter
            //Controller: Transactions
            //Action; RedirectToCartaSìPage
            //Area: Laser.Orchard.PaymentCartaSi
            var hp = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            var ub = new UriBuilder(_orchardServices.WorkContext.HttpContext.Request.Url.AbsoluteUri) {
                Path = hp.Action("RedirectToCartaSìPage", "Transactions", new { Area = Constants.LocalArea, Id = paymentId })
            };
            return ub.Uri.ToString();
        }

        private string ActionUrl(string aName, string cName = "Transactions", string areaName = Constants.LocalArea) {
            string sName = _orchardServices.WorkContext.CurrentSite.SiteName;
            string bUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
            var hp = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            string aPath = hp.Action(aName, cName, new { Area = areaName });
            int cut = aPath.IndexOf(sName) - 1;
            return bUrl + aPath.Substring(cut);
        }
        public string StartCartaSiTransaction(int paymentId) {
            var settings = _orchardServices.WorkContext.CurrentSite.As<PaymentCartaSiSettingsPart>();
            
            string pURL = settings.UseTestEnvironment ? EndPoints.TestPaymentURL : EndPoints.PaymentURL;

            StartPaymentMessage spMsg = new StartPaymentMessage(settings.CartaSiShopAlias, GetPaymentInfo(paymentId));
            spMsg.url = ActionUrl("CartaSiOutcome");
            spMsg.url_back = ActionUrl("CartaSiUndo");
            spMsg.mac = spMsg.TransactionStartMAC;

            try {
                Validator.ValidateObject(spMsg, new ValidationContext(spMsg), true);
            } catch (Exception ex) {
                //Log the error
                Logger.Error(T("Transaction information not valid: {0}", ex.Message).Text);
                //update the PaymentRecord for this transaction
                EndPayment(paymentId, false, null, T("Transaction information not valid: {0}", ex.Message).Text);
                //return the URL of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                return GetPaymentInfoUrl(paymentId);
            }

            return null;
        }
    }
}