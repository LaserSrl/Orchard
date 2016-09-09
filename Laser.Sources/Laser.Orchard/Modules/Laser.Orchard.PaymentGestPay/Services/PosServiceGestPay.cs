using Laser.Orchard.PaymentGateway;
using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.Services;
using Laser.Orchard.PaymentGestPay.CryptDecryptProd;
using Laser.Orchard.PaymentGestPay.CryptDecryptTest;
using Laser.Orchard.PaymentGestPay.Models;
using Laser.Orchard.PaymentGestPay.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using System;
using System.Collections.Generic;
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
        public override string GetPosUrl(int paymentId) {
            return "GestPay";
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

        public void StartGestPayTransaction(GestPayTransaction gpt) {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<PaymentGestPaySettingsPart>();

            //get the encrypted parameter string
            if (settings.UseTestEnvironment) {
                var client = new CryptDecryptTest.WSCryptDecryptSoapClient();
                XmlNode encryptXML = client.Encrypt(
                    shopLogin: settings.GestPayShopLogin,
                    uicCode:gpt.uicCode,
                    amount:gpt.amount,
                    shopTransactionId:gpt.shopTransactionID,
                    cardNumber: gpt.cardNumber,
                    expiryMonth:gpt.expiryMonth,
                    expiryYear:gpt.expiryYear,
                    buyerName:gpt.buyerName,
                    buyerEmail:gpt.buyerEmail,
                    languageId:gpt.languageId,
                    cvv:gpt.cvv,
                    customInfo:gpt.customInfo,
                    requestToken:gpt.requestToken,
                    ppSellerProtection:gpt.ppSellerProtection,
                    shippingDetails:gpt.shippingDetails.TestVersion(),
                    paymentTypes:gpt.paymentTypes,
                    paymentTypeDetail: gpt.paymentTypeDetail.TestVersion(),
                    redFraudPrevention:gpt.redFraudPrevention,
                    Red_CustomerInfo:gpt.Red_CustomerInfo.TestVersion(),
                    Red_ShippingInfo:gpt.Red_ShippingInfo.TestVersion(),
                    Red_BillingInfo:gpt.Red_BillingInfo.TestVersion(),
                    Red_CustomerData: gpt.Red_CustomerData.TestVersion(),
                    Red_CustomInfo:gpt.Red_CustomInfo,
                    Red_Items:gpt.Red_Items.TestVersion(),
                    Consel_MerchantPro:gpt.Consel_MerchantPro,
                    Consel_CustomerInfo:gpt.Consel_CustomerInfo.TestVersion(),
                    payPalBillingAgreementDescription:gpt.payPalBillingAgreementDescription,
                    OrderDetails:gpt.OrderDetails.TestVersion()
                );
            } else {
                var client = new CryptDecryptProd.WSCryptDecryptSoapClient();
            }
        }

    }
}