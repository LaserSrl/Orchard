using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public abstract string GetPosUrl(int paymentId);
        public abstract string GetPosUrl(int paymentId, string redirectUrl, string schema);

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
        public void EndPayment(int paymentId, bool success, string error, string info, string transactionId = "") {
            PaymentRecord paymentToSave = null;
            PaymentRecord payment = GetPaymentInfo(paymentId);
            if (string.IsNullOrWhiteSpace(payment.PosName)) {
                paymentToSave = payment;
            }
            else {
                // forza la creazione di un nuovo record perché c'è già stato un tentativo di pagamento
                paymentToSave = new PaymentRecord();
                paymentToSave.Reason = payment.Reason;
                paymentToSave.Amount = payment.Amount;
                paymentToSave.Currency = payment.Currency;
                paymentToSave.ContentItemId = payment.ContentItemId;
                paymentToSave.UserId = payment.UserId;
            }
            paymentToSave.Success = success;
            paymentToSave.Error = error;
            paymentToSave.Info = info;
            paymentToSave.TransactionId = transactionId;
            paymentToSave.PosName = GetPosName(); // forza la valorizzazione del PosName
            paymentToSave.PosUrl = GetPosActionUrl(paymentId);
            SavePaymentInfo(paymentToSave);
            // solleva l'evento di termine della transazione
            if (success) {
                _paymentEventHandler.OnSuccess(paymentToSave.Id, paymentToSave.ContentItemId);
            }
            else {
                _paymentEventHandler.OnError(paymentToSave.Id, paymentToSave.ContentItemId);
            }
        }
        /// <summary>
        /// Fornisce l'URL per consultare l'esito del pagamento
        /// </summary>
        /// <param name="paymentId"></param>
        /// <returns></returns>
        public string GetPaymentInfoUrl(int paymentId) {
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
            if (record == null) {
                values.CreationDate = now;
                values.UpdateDate = now;
                _repository.Create(values);
            }
            else {
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