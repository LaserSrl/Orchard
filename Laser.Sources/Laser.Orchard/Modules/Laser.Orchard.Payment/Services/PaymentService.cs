using Laser.Orchard.Payment.Models;
using Orchard;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;




namespace Laser.Orchard.Payment.Services {
    public class PaymentService : IPaymentService {
        private bool test;
        private string id_transazione;
        private string ShopLogin;
      // private readonly LocalWebConfigManager _localConfigurationManager;
        
        private PaymentMethod PaymentToUse;
        private readonly IOrchardServices _orchardServices;

        public PaymentService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            id_transazione = "";
            PaymentToUse = PaymentMethod.None;
             Enum.TryParse( _orchardServices.WorkContext.CurrentSite.As<PaymentSettingsPart>().PaymentMethodSelected,out PaymentToUse);
            //_localConfigurationManager = new LocalWebConfigManager();

            //var conf = _localConfigurationManager.GetConfiguration("~/Modules/Laser.Orchard.Payment");
            //PaymentToUse = PaymentMethod.None;
            //var paymentsettings = conf.GetSection("Payment.appSettings") as AppSettingsSection;
            //if (paymentsettings != null) {
            //    Enum.TryParse(paymentsettings.Settings["PaymentToUse"].Value, out PaymentToUse);

            //}

            // Enum.TryParse(conf.AppSettings.Settings["PaymentToUse"].Value, out PaymentToUse);
            if (PaymentToUse == PaymentMethod.GestPay) {
                ShopLogin = _orchardServices.WorkContext.CurrentSite.As<PaymentSettingsPart>().GestpayShopLogin;
                test = _orchardServices.WorkContext.CurrentSite.As<PaymentSettingsPart>().GestpayTest;
               //var settings = conf.GetSection("GestPay.appSettings") as AppSettingsSection;
               // if (settings != null) {
               //     ShopLogin = settings.Settings["ShopLogin"].Value;
               //     test = Convert.ToBoolean(settings.Settings["Test"].Value);
               // }
            }
        }


        public TransazioneRicevuta GestPayRiceviTranzazioneS2S(string a, string b) {
            if (a == ShopLogin) {
                TransazioneRicevuta tr = new TransazioneRicevuta();
                var wstest = new it.sella.testecomm.WSCryptDecrypt();
                var wsOfficial = new it.sella.ecomms2s.WSCryptDecrypt();
                XmlNode res;
                if (test) {
                    res = wstest.Decrypt(a, b);
                } else {
                    res = wsOfficial.Decrypt(a, b);
                }
                string esito = res.SelectSingleNode("TransactionResult").InnerText;
                if (esito == "OK") {
                    tr.CodiceDivisa = (ListaCodiciDivisa)Enum.Parse(typeof(ListaCodiciDivisa), res.SelectSingleNode("Currency").InnerText);
                    tr.Amount = Convert.ToDouble(res.SelectSingleNode("Amount").InnerText.Replace('.', ','));
                    tr.ShopTransactionID = res.SelectSingleNode("ShopTransactionID").InnerText;
                    tr.AuthorizationCode = res.SelectSingleNode("AuthorizationCode").InnerText;
                    tr.ErrorCode = Convert.ToInt32(res.SelectSingleNode("ErrorCode").InnerText);
                    tr.ErrorDescription = res.SelectSingleNode("ErrorDescription").InnerText;
                    tr.BankTransactionID = Convert.ToInt32(res.SelectSingleNode("BankTransactionID").InnerText);
                    string elencocustominfo = res.SelectSingleNode("CustomInfo").InnerText;
                    if (!string.IsNullOrEmpty(elencocustominfo)) {
                        Dictionary<string, string> newdic = new Dictionary<string, string>();
                        var listcustominfo = elencocustominfo.Split(new string[] { "*P1*" }, StringSplitOptions.None);
                        foreach (var ele in listcustominfo) {
                            newdic.Add(ele.Split('=')[0], ele.Split('=')[1]);
                        }
                        tr.CustomInfo = newdic;
                    }
                    return tr;
                } else
                    return null;
            } else
                return null;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentToCrypt">
        /// Transazione paymentToCrypt 
        /// campi obbligatori: 
        /// Amount > 0
        /// ShopLogin prelevato dal config
        /// CodiceDivisa se non inserito viene valorizzato euro
        /// ShopTransactionID se non valorizzato viene generato in modo casuale
        /// 
        /// 
        /// </param>
        /// <returns></returns>
        public string GestPayAvviaTransazione(Transazione paymentToCrypt) {
            #region [Charge default value]
            paymentToCrypt.ShopLogin = ShopLogin;
            if (paymentToCrypt.CodiceDivisa == null || paymentToCrypt.CodiceDivisa == 0)
                paymentToCrypt.CodiceDivisa = ListaCodiciDivisa.EUR;
            if (string.IsNullOrEmpty(paymentToCrypt.ShopTransactionID))
                paymentToCrypt.ShopTransactionID = GeneraLaIDTransazione();
            if (string.IsNullOrEmpty(paymentToCrypt.BuyerName))
                paymentToCrypt.BuyerName = "";
            if (string.IsNullOrEmpty(paymentToCrypt.BuyerEmail))
                paymentToCrypt.BuyerEmail = "";
            #endregion
            if (!paymentToCrypt.valid) {
                throw new Exception("Oggetto transazione non valido dettaglio errore:");
                // TODO gestire gli errori nel model in modo da avere l'errore specifico
                return null;
            }


            #region [Test Requisiti minimi per Transazione]
            if (!(paymentToCrypt.ShopLogin.ToString().Length > 0 && paymentToCrypt.CodiceDivisa.ToString().Length > 0 && paymentToCrypt.Amount > 0 && paymentToCrypt.ShopTransactionID != "")) {
                throw new Exception("Uno dei campi obbligatoti è mancante");
                // TODO gestire gli errori in base al campo mancante
                return null;
            }
            #endregion

            var wstest = new it.sella.testecomm.WSCryptDecrypt();
            var wsOfficial = new it.sella.ecomms2s.WSCryptDecrypt();
            XmlNode res;
            if (test) {
                res = wstest.Encrypt(paymentToCrypt.ShopLogin, paymentToCrypt.CodiceDivisaCorrect.ToString(), paymentToCrypt.AmountCorrect, paymentToCrypt.ShopTransactionID, "", "", "", paymentToCrypt.BuyerName, paymentToCrypt.BuyerEmail, paymentToCrypt.LanguageCorrect.ToString(), "", paymentToCrypt.CustomInfo, "", "", null, null, null, null, null, null, null, null, null);
            } else {
                res = wsOfficial.Encrypt(paymentToCrypt.ShopLogin, paymentToCrypt.CodiceDivisaCorrect.ToString(), paymentToCrypt.AmountCorrect, paymentToCrypt.ShopTransactionID, "", "", "", paymentToCrypt.BuyerName, paymentToCrypt.BuyerEmail, paymentToCrypt.LanguageCorrect.ToString(), "", paymentToCrypt.CustomInfo, "", "", null, null, null, null, null, null, null, null, null);
            }
            string esito = res.SelectSingleNode("TransactionResult").InnerText;
            if (esito == "OK") {
                string string_crypt = res.SelectSingleNode("CryptDecryptString").InnerText;

                if (test) {
                    return "https://testecomm.sella.it/pagam/pagam.aspx?a=" + ShopLogin + "&b=" + string_crypt;
                } else {
                    return "https://ecomm.sella.it/pagam/pagam.aspx?a=" + ShopLogin + "&b=" + string_crypt;
                }
            } else {
                throw new Exception("KO: error code " + res.SelectSingleNode("ErrorCode").InnerText + " " + res.SelectSingleNode("ErrorDescription").InnerText);
                return null;
            }
        }

        private string GeneraLaIDTransazione() {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 20)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }
    }
}