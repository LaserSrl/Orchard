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
        public override string GetPosUrl(PaymentGateway.Models.PaymentRecord values) {
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

        public void StartGestPayTransaction() {
            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<PaymentGestPaySettingsPart>();

            //get the encrypted parameter string
            if (settings.UseTestEnvironment) {
                var client = new CryptDecryptTest.WSCryptDecryptSoapClient();
                //XmlNode encryptXML = client.Encrypt(
                //    shopLogin: settings.GestPayShopLogin,
                //    uicCode:
                //);
            } else {
                var client = new CryptDecryptProd.WSCryptDecryptSoapClient();
            }
        }

    }
}