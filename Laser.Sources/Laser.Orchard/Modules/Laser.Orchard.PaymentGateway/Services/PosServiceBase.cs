using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.StartupConfig.ViewModels;
using Newtonsoft.Json;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Services {
    public abstract class PosServiceBase : IPosService {
        protected readonly IOrchardServices _orchardServices;
        private readonly IRepository<PaymentRecord> _repository;
        private readonly IPaymentEventHandler _paymentEventHandler;

        public Localizer T { get; set; }

        public abstract string GetPosName();
        /// <summary>
        /// Restituisce il nome del controller utilizzato per la gestione dei settings del POS che deve ereditare da PosAdminBaseController.
        /// Il nome non deve avere il suffisso "Controller" (es. "Admin", non "AdminController").
        /// Restiuisce null o stringa vuota se non è necessario un controller per i settings.
        /// </summary>
        /// <returns></returns>
        public abstract string GetSettingsControllerName();

        /// <summary>
        /// Get the return URL passed to the virtual POS.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public abstract string GetPosActionUrl(int paymentId);
        public abstract string GetPosActionUrl(string paymentGuid);

        public abstract string GetPosUrl(int paymentId);
        public string GetPosUrl(string paymentGuid) {
            return GetPosUrl(GetPaymentInfo(paymentGuid).Id);
        }

        public PosServiceBase(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler) {
            _orchardServices = orchardServices;
            _repository = repository;
            _paymentEventHandler = paymentEventHandler;

            T = NullLocalizer.Instance;
        }
        public PaymentRecord StartPayment(PaymentRecord values) {
            // verifica che siano presenti i valori necessari
            if ((values.Amount <= 0)
                || string.IsNullOrWhiteSpace(values.Currency)) {
                throw new Exception("Parameters missing. Required parameters: Amount, Currency.");
            }
            values.PosName = GetPosName();
            if (string.IsNullOrWhiteSpace(values.PosUrl)) {
                string posUrl = GetPosActionUrl(values.Id);
                values.PosUrl = posUrl;
            }
            // salva l'eventuale informazione sull'utente
            var user = _orchardServices.WorkContext.CurrentUser;
            if (user != null) {
                values.UserId = user.Id;
            }

            values.Guid = Guid.NewGuid().ToString(); 

            int paymentId = SavePaymentInfo(values);
            values.Id = paymentId;
            return values;
        }
        public PaymentRecord GetPaymentInfo(int paymentId) {
            // verifica che siano presenti i parametri necessari
            if (paymentId <= 0) {
                throw new Exception("Invalid parameter 'Id'.");
            }
            PaymentRecord result = _repository.Get(paymentId);
            return result;
        }
        public PaymentRecord GetPaymentInfo(string guid) {
            return _repository.Table.Where(r => r.Guid == guid).SingleOrDefault();
        }
        public void EndPayment(int paymentId, bool success, string error, string info, string transactionId = "") {
            PaymentRecord paymentToSave = GetPaymentInfo(paymentId); //null;
            //PaymentRecord payment = GetPaymentInfo(paymentId);
            if (paymentToSave.PaymentTransactionComplete) {
                //this payment had completed already, so we should not be here
                //We may be here (in healthy cases) if the transactin was closed by a S2S call, and then a S2C call
                //happened. So, we do not change the success/failure of the transaction (but verify it, to be safe).
                //However, it may make sense to add eventual errors or other info to the PaymentRecord
                //
                //REALLY, THIS SHOULD NEVER HAPPEN
                //
                //bool newError = false;
                //if (!string.IsNullOrWhiteSpace(error)) {
                //    //Check that the same error has not been inserted already
                //    if (!paymentToSave.Error.Contains(error)) {
                //        paymentToSave.Error = string.Join(Environment.NewLine, new string[] { paymentToSave.Error, error });
                //        newError = true;
                //    }
                //}
                //if (!string.IsNullOrWhiteSpace(info)) {
                //    if (!paymentToSave.Info.Contains(info)) {
                //        paymentToSave.Info = string.Join(Environment.NewLine, new string[] { paymentToSave.Info, info });
                //    }
                //}
                //SavePaymentInfo(paymentToSave);
                //if (newError) {
                //    _paymentEventHandler.OnError(paymentToSave.Id, paymentToSave.ContentItemId);
                //}
            } else {
                //paymentToSave = payment;
                paymentToSave.Success = success;
                paymentToSave.Error = error;
                paymentToSave.Info = info;
                paymentToSave.TransactionId = transactionId;
                paymentToSave.PosName = GetPosName(); // forza la valorizzazione del PosName
                paymentToSave.PosUrl = GetPosActionUrl(paymentId);
                paymentToSave.PaymentTransactionComplete = true; //flag the transaction as complete
                SavePaymentInfo(paymentToSave);
                // solleva l'evento di termine della transazione
                if (success) {
                    _paymentEventHandler.OnSuccess(paymentToSave.Id, paymentToSave.ContentItemId);
                } else {
                    _paymentEventHandler.OnError(paymentToSave.Id, paymentToSave.ContentItemId);
                }
            }

        }
        /// <summary>
        /// Fornisce l'URL per consultare l'esito del pagamento
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public string GetPaymentInfoUrl(int paymentId) {
            //get the PaymentRecord corresponding to the id
            var pRecord = GetPaymentInfo(paymentId);
            if (!string.IsNullOrWhiteSpace(pRecord.CustomRedirectUrl)) {
                //Serialize a Response object in a query string
                string respQString = "";
                bool success = pRecord.Success;
                string message = (pRecord.Success) ? pRecord.Info : (string.Format("{0}: {1}", pRecord.Error, pRecord.Info));
                //ErrorCode errorCode = (pRecord.Success) ? ErrorCode.NoError : ErrorCode.GenericError;
                //ResolutionAction resolutionAction = ResolutionAction.NoAction;
                dynamic data = new ExpandoObject(); //all properties will be strings
                data.PaymentId = paymentId.ToString();
                if (pRecord.ContentItemId > 0) {
                    data.ContentItemId = pRecord.ContentItemId.ToString();
                }
                //using the ExpandoObject as IDictionary<string, object> i can add properties to data in runtime without knowing their names beforehand
                if (!string.IsNullOrWhiteSpace(pRecord.APIFilters)) {
                    IDictionary<string, object> expDic = data;
                    string[] keys = pRecord.APIFilters.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var k in keys) {
                        expDic[k] = pRecord.GetProperty(k);
                    }
                }
                
                List<string> qsFragments = new List<string>();
                qsFragments.Add(string.Format("Success={0}", success.ToString()));
                qsFragments.Add(string.Format("Message={0}", HttpUtility.UrlEncode(message)));
                foreach (KeyValuePair<string, object> kvp in data) {
                    qsFragments.Add(string.Format("Data_{0}={1}", HttpUtility.UrlEncode(kvp.Key), HttpUtility.UrlEncode((string)(kvp.Value))));
                }
                respQString = string.Join(@"&", qsFragments);
                //if the redirect url starts with neither http:// nor https://, assume http://
                string rBaseUrl = pRecord.CustomRedirectUrl;
                if (!Regex.IsMatch(rBaseUrl, @"(^https?://)")) {
                    rBaseUrl = string.Format(@"https://{0}", rBaseUrl);
                }
                //append the querystring to the return url ad return the resulting url
                return string.Format("{0}?{1}", rBaseUrl, respQString);
            } else if (!string.IsNullOrWhiteSpace(pRecord.CustomRedirectSchema)) {
                //serialize a response object as a JSON
                string jsonResponse = "";
                dynamic response = new ExpandoObject();
                response.Success = pRecord.Success;
                response.Message = (pRecord.Success) ? pRecord.Info : (string.Format("{0}: {1}", pRecord.Error, pRecord.Info));
                dynamic data = new ExpandoObject();
                data.PaymentId = paymentId.ToString();
                if (pRecord.ContentItemId > 0) {
                    data.ContentItemId = pRecord.ContentItemId.ToString();
                }
                //using the ExpandoObject as IDictionary<string, object> i can add properties to data in runtime without knowing their names beforehand
                if (!string.IsNullOrWhiteSpace(pRecord.APIFilters)) {
                    IDictionary<string, object> expDic = data;
                    string[] keys = pRecord.APIFilters.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var k in keys) {
                        expDic[k] = pRecord.GetProperty(k);
                    }
                }
                response.Data = data;
                jsonResponse = JsonConvert.SerializeObject(response);

                //List<string> qsFragments = new List<string>();
                //qsFragments.Add(string.Format("Success={0}", response.Success.ToString()));
                //qsFragments.Add(string.Format("Message={0}", HttpUtility.UrlEncode((string)(response.Message))));
                //foreach (KeyValuePair<string, object> kvp in data) {
                //    qsFragments.Add(string.Format("Data_{0}={1}", HttpUtility.UrlEncode(kvp.Key), HttpUtility.UrlEncode((string)(kvp.Value))));
                //}
                //var respQString = string.Join(@"&", qsFragments);

                //append the JSON after the schema and return it as an URL
                return string.Format("{0}:{1}", pRecord.CustomRedirectSchema, jsonResponse);
                //return string.Format("{0}:{1}", pRecord.CustomRedirectSchema, HttpUtility.UrlEncode(jsonResponse));
                //return string.Format("{0}:{1}", pRecord.CustomRedirectSchema, respQString);
            }
            return new UrlHelper(HttpContext.Current.Request.RequestContext).Action("Info", "Payment", new { area = "Laser.Orchard.PaymentGateway", paymentId = paymentId });
        }
        /// <summary>
        /// Salva il pagamento e restituisce il PaymentId.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private int SavePaymentInfo(PaymentRecord values) {
            PaymentRecord record = null;
            DateTime now = DateTime.Now;
            if (values.Id > 0) {
                record = _repository.Get(values.Id);
            }
            values.PosName = GetValidString(values.PosName, 255);
            values.Reason = GetValidString(values.Reason, 255);
            values.Error = GetValidString(values.Error, 255);
            values.TransactionId = GetValidString(values.TransactionId, 255);
            // 4000 è la massima lunghezza di stringa che nhibernate riesce a gestire
            values.PosUrl = GetValidString(values.PosUrl, 4000);
            values.Info = GetValidString(values.Info, 4000);
            values.CustomRedirectUrl = GetValidString(values.CustomRedirectUrl, 4000);
            values.CustomRedirectSchema = GetValidString(values.CustomRedirectSchema, 4000);
            if (record == null) {
                values.CreationDate = now;
                values.UpdateDate = now;
                _repository.Create(values);
            } else {
                values.UpdateDate = now;
                _repository.Update(values);
            }
            return values.Id;
        }
        private string GetValidString(string text, int maxLength) {
            string result = text;
            if ((result != null) && (result.Length > maxLength)) {
                result = result.Substring(0, maxLength);
            }
            return result;
        }

        public virtual List<string> GetAllValidCurrencies() {
            List<string> ret = new List<string>();
            ret.Add("EUR");
            return ret;
        }
    }
}