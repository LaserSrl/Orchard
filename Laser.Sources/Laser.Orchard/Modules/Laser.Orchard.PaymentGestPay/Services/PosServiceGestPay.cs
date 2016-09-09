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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml;

namespace Laser.Orchard.PaymentGestPay.Services {
    public class PosServiceGestPay : PosServiceBase, IGestPayAdminServices {

        public PosServiceGestPay(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler) :
            base(orchardServices, repository, paymentEventHandler) {

        }

        #region Implementation of abstract class
        //implement abstract methods from base class to avoid compilation errors while I do something else
        public override string GetPosName() {
            return "GestPay";
        }
        /// <summary>
        /// THis gets called by the "general" payment services.
        /// </summary>
        /// <param name="paymentId">The id corresponding to a <type>PaymentRecord</type> for the transaction we want to start.</param>
        /// <returns>The url of a page to which we redirect the client's browser to complete the payment.</returns>
        public override string GetPosUrl(int paymentId) {
            //Get the PaymentRecord corresponding to the ID
            IRepository<PaymentRecord> _repository = null; //TODO: this here as a placeholder just to allow compilation
            PaymentRecord record = _repository.Get(paymentId);
            //Use the payment record to create a new GestPayTransaction
            //Call StartGestPayTransaction to start the payment process
            return StartGestPayTransaction(new GestPayTransaction(record));
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

        public string StartGestPayTransaction(GestPayTransaction gpt) {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<PaymentGestPaySettingsPart>();

            //parameter validation
            if (gpt == null) {
                //TODO: manage this case
                //Log the error
                //update the PaymentRecord for this transaction
                //return the url of a suitable error page
            }
            try {
                Validator.ValidateObject(gpt, new ValidationContext(gpt), true);
            } catch (Exception ex) {
                //TODO: manage validation failure
                //Log the error
                //update the PaymentRecord for this transaction
                //return the URL of a suitable error page
            }

            //get the encrypted parameter string
            EncryptDecryptTransactionResult res = null;
            string urlFormat = "";
            XmlNode encryptXML = null;
            if (settings.UseTestEnvironment) {
                using (var client = new CryptDecryptTest.WSCryptDecryptSoapClient()) {
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
                using (var client = new CryptDecryptProd.WSCryptDecryptSoapClient()) {
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
                //update the PaymentRecord for this transaction
                //return the url of a suitable error page
            }

            if (res.TransactionResult.ToUpperInvariant() == "OK") {
                return string.Format(urlFormat, settings.GestPayShopLogin, res.CryptDecryptString);
            } else {
                //TODO: manage errors received 
                //Log the error
                //update the PaymentRecord for this transaction
                //return the url of a suitable error page
            }

            return null; //TODO: this is just a placeholder to allow compilation
        }

    }
}