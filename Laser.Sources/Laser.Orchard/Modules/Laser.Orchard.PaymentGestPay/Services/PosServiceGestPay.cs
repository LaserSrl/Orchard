using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Laser.Orchard.PaymentGestPay.CryptDecryptProd;
using Laser.Orchard.PaymentGestPay.CryptDecryptTest;
using Laser.Orchard.PaymentGestPay.Extensions;
using Laser.Orchard.PaymentGestPay.Models;
using Laser.Orchard.PaymentGestPay.ViewModels;
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
using System.Xml;

using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel;
using System.Text.RegularExpressions;

namespace Laser.Orchard.PaymentGestPay.Services {
    public class PosServiceGestPay : PosServiceBase, IGestPayAdminServices, IGestPayTransactionServices {

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public PosServiceGestPay(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler) :
            base(orchardServices, repository, paymentEventHandler) {

                T = NullLocalizer.Instance;
                Logger = NullLogger.Instance;

            ////update THe instances Web.Config with the settings required to connect to the GestPay services
            //    var wConf = WebConfigurationManager.OpenWebConfiguration("~");
            //    ServicesSection sSection = wConf.GetSection("system.serviceModel") as ServicesSection;
            //    if (sSection == null) {
            //        //section is not there yet
            //        sSection = new ServicesSection();
            //    } else {
            //        //the section is there already. We must check whether we have to add the GestPay services.

            //    }
        }

        #region Implementation of abstract class
        //implement abstract methods from base class to avoid compilation errors while I do something else
        public override string GetPosName() {
            return "GestPay";
        }
        /// <summary>
        /// This gets called by the "general" payment services.
        /// </summary>
        /// <param name="paymentId">The id corresponding to a <type>PaymentRecord</type> for the transaction we want to start.</param>
        /// <returns>The url corresponding to an action that will start the GestPay transaction </returns>
        public override string GetPosUrl(int paymentId) {
            //create the url for the controller action that takes care of the redirect, passing the id as parameter
            //Controller: Transactions
            //Action: RedirectToGestPayPage
            //Area: Laser.Orchard.PaymentGestPay
            var hp = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            var ub = new UriBuilder(_orchardServices.WorkContext.HttpContext.Request.Url.AbsoluteUri) {
                Path = hp.Action("RedirectToGestPayPage", "Transactions", new { Area = Constants.LocalArea, Id = paymentId }),
                //Query= "Id=" + paymentId.ToString()
            };
            //var ub = new UrlHelper().Action("RedirectToGestPayPage", "Transactions", new { Id = paymentId });
            return ub.Uri.ToString();
        }
        #endregion

        #region Admin Methods
        public GestPaySettingsViewModel GetSettingsVM() {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<PaymentGestPaySettingsPart>();

            return new GestPaySettingsViewModel(settings);
        }

        public void UpdateSettings(GestPaySettingsViewModel vm) {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<PaymentGestPaySettingsPart>();

            settings.GestPayShopLogin = vm.GestPayShopLogin;
            settings.UseTestEnvironment = vm.UseTestEnvironment;
        }
        #endregion
        
        /// <summary>
        /// This gets called by the Action actually starting the transaction.
        /// </summary>
        /// <param name="paymentId">The id corresponding to a <type>PaymentRecord</type> for the transaction we want to start.</param>
        /// <returns>The url of a page to which we redirect the client's browser to complete the payment.</returns>
        public string StartGestPayTransaction(int paymentId) {
        //    return StartGestPayTransaction(new GestPayTransaction(GetPaymentInfo(paymentId)));
        //}

        //public string StartGestPayTransaction(GestPayTransaction gpt) {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<PaymentGestPaySettingsPart>();

            var gpt = new GestPayTransaction(GetPaymentInfo(paymentId));
            //parameter validation
            if (gpt == null) {
                //TODO: manage this case
                //Log the error
                Logger.Error(T("Transaction object cannot be null.").Text);
                //update the PaymentRecord for this transaction
                EndPayment(paymentId, false, T("Failed to create a transaction object based on the PaymentRecord").Text, null);
                //return the url of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                return GetPaymentInfoUrl(paymentId);
            }
            try {
                Validator.ValidateObject(gpt, new ValidationContext(gpt), true);
            } catch (Exception ex) {
                //TODO: manage validation failure
                //Log the error
                Logger.Error(T("Transaction information not valid: {0}", ex.Message).Text);
                //update the PaymentRecord for this transaction
                EndPayment(paymentId, false, T("Transaction information not valid: {0}", ex.Message).Text, null);
                //return the URL of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                return GetPaymentInfoUrl(paymentId);
            }

            //get the encrypted parameter string
            EncryptDecryptTransactionResult res = null;
            string urlFormat = "";
            XmlNode encryptXML = null;
            if (settings.UseTestEnvironment) {
                string endpoint = string.Format(Endpoints.TestWSEntry, Endpoints.CryptDecryptEndPoint);
                endpoint = endpoint.Substring(0, endpoint.Length - 4);
                //WSHttpBinding binding = new WSHttpBinding();
                //binding.Security.Mode = SecurityMode.Transport;
                //binding.MessageEncoding = WSMessageEncoding.Text;
                BasicHttpBinding binding = new BasicHttpBinding();
                endpoint = Regex.Replace(endpoint, "(https)", "http"); //https gives errors
                EndpointAddress address = new EndpointAddress(endpoint);

                using (var client = new CryptDecryptTest.WSCryptDecryptSoapClient(binding, address)) {

                    encryptXML = client.Encrypt(
                        shopLogin: settings.GestPayShopLogin,
                        uicCode: gpt.uicCode,
                        amount: gpt.amount,
                        shopTransactionId: gpt.shopTransactionID,
                        cardNumber: gpt.cardNumber,
                        expiryMonth: gpt.expiryMonth,
                        expiryYear: gpt.expiryYear,
                        buyerName: gpt.buyerName,
                        buyerEmail: gpt.buyerEmail,
                        languageId: gpt.languageId,
                        cvv: gpt.cvv,
                        customInfo: gpt.customInfo,
                        requestToken: gpt.requestToken,
                        ppSellerProtection: gpt.ppSellerProtection,
                        shippingDetails: gpt.shippingDetails.TestVersion(),
                        paymentTypes: gpt.paymentTypes.ToArray(),
                        paymentTypeDetail: gpt.paymentTypeDetail.TestVersion(),
                        redFraudPrevention: gpt.redFraudPrevention,
                        Red_CustomerInfo: gpt.Red_CustomerInfo.TestVersion(),
                        Red_ShippingInfo: gpt.Red_ShippingInfo.TestVersion(),
                        Red_BillingInfo: gpt.Red_BillingInfo.TestVersion(),
                        Red_CustomerData: gpt.Red_CustomerData.TestVersion(),
                        Red_CustomInfo: gpt.Red_CustomInfo.ToArray(),
                        Red_Items: gpt.Red_Items.TestVersion(),
                        Consel_MerchantPro: gpt.Consel_MerchantPro,
                        Consel_CustomerInfo: gpt.Consel_CustomerInfo.TestVersion(),
                        payPalBillingAgreementDescription: gpt.payPalBillingAgreementDescription,
                        OrderDetails: gpt.OrderDetails.TestVersion()
                    );
                    urlFormat = string.Format(Endpoints.TestPayEntry, Endpoints.PaymentPage);
                }
            } else {
                string endpoint = string.Format(Endpoints.ProdWSEntry, Endpoints.CryptDecryptEndPoint);
                endpoint = endpoint.Substring(0, endpoint.Length - 4);
                //WSHttpBinding binding = new WSHttpBinding();
                //binding.Security.Mode = SecurityMode.Transport;
                //binding.MessageEncoding = WSMessageEncoding.Text;
                BasicHttpBinding binding = new BasicHttpBinding();
                endpoint = Regex.Replace(endpoint, "(https)", "http"); //https gives errors
                EndpointAddress address = new EndpointAddress(endpoint);

                using (var client = new CryptDecryptProd.WSCryptDecryptSoapClient(binding, address)){
                    encryptXML = client.Encrypt(
                        shopLogin: settings.GestPayShopLogin,
                        uicCode: gpt.uicCode,
                        amount: gpt.amount,
                        shopTransactionId: gpt.shopTransactionID,
                        cardNumber: gpt.cardNumber,
                        expiryMonth: gpt.expiryMonth,
                        expiryYear: gpt.expiryYear,
                        buyerName: gpt.buyerName,
                        buyerEmail: gpt.buyerEmail,
                        languageId: gpt.languageId,
                        cvv: gpt.cvv,
                        customInfo: gpt.customInfo,
                        requestToken: gpt.requestToken,
                        ppSellerProtection: gpt.ppSellerProtection,
                        shippingDetails: gpt.shippingDetails.ProdVersion(),
                        paymentTypes: gpt.paymentTypes.ToArray(),
                        paymentTypeDetail: gpt.paymentTypeDetail.ProdVersion(),
                        redFraudPrevention: gpt.redFraudPrevention,
                        Red_CustomerInfo: gpt.Red_CustomerInfo.ProdVersion(),
                        Red_ShippingInfo: gpt.Red_ShippingInfo.ProdVersion(),
                        Red_BillingInfo: gpt.Red_BillingInfo.ProdVersion(),
                        Red_CustomerData: gpt.Red_CustomerData.ProdVersion(),
                        Red_CustomInfo: gpt.Red_CustomInfo.ToArray(),
                        Red_Items: gpt.Red_Items.ProdVersion(),
                        Consel_MerchantPro: gpt.Consel_MerchantPro,
                        Consel_CustomerInfo: gpt.Consel_CustomerInfo.ProdVersion(),
                        payPalBillingAgreementDescription: gpt.payPalBillingAgreementDescription,
                        OrderDetails: gpt.OrderDetails.ProdVersion()
                    );
                    urlFormat = string.Format(Endpoints.ProdPayEntry, Endpoints.PaymentPage);
                }
            }

            try {
                res = new EncryptDecryptTransactionResult(encryptXML);
                Validator.ValidateObject(res, new ValidationContext(res), true);
            } catch (Exception ex) {
                //TODO: manage validation errors in the results received from the GestPay servers
                //Log the error
                Logger.Error(T("Validation problems on the response received: {0}", ex.Message).Text);
                //update the PaymentRecord for this transaction
                EndPayment(paymentId, false, T("Validation problems on the response received: {0}", ex.Message).Text, null);
                //return the URL of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                return GetPaymentInfoUrl(paymentId);
            }

            if (res.TransactionResult.ToUpperInvariant() == "OK") {
                return string.Format(urlFormat, settings.GestPayShopLogin, res.CryptDecryptString);
            } else {
                //TODO: manage errors received 
                //Log the error
                Logger.Error(T("Remote service replied with an error. Error {0}: {1}", res.ErrorCode, res.ErrorDescription).Text);
                //update the PaymentRecord for this transaction
                EndPayment(paymentId, false, T("Remote service replied with an error. Error {0}: {1}", res.ErrorCode, res.ErrorDescription).Text, null);
                //return the URL of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
                return GetPaymentInfoUrl(paymentId);
            }
            //If we are here, something went really wrong
            //Log the error
            Logger.Error(T("Unknown critical error.").Text);
            //update the PaymentRecord for this transaction
            EndPayment(paymentId, false, T("Unknown critical error.").Text, null);
            //return the URL of a suitable error page (call this.GetPaymentInfoUrl after inserting the error in the PaymentRecord)
            return GetPaymentInfoUrl(paymentId);
        }

    }
}