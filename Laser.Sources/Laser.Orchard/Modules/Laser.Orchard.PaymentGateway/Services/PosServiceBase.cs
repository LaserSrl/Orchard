using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Services {
    public abstract class PosServiceBase : IPosService {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<PaymentRecord> _repository;
        private readonly IPaymentEventHandler _paymentEventHandler;

        public PosServiceBase(IOrchardServices orchardServices, IRepository<PaymentRecord> repository, IPaymentEventHandler paymentEventHandler) {
            _orchardServices = orchardServices;
            _repository = repository;
            _paymentEventHandler = paymentEventHandler;
        }
        public abstract string GetPosName();
        public PaymentRecord StartPayment(PaymentRecord values) {
            // verifica che siano presenti i valori necessari
            if (string.IsNullOrWhiteSpace(values.Reason)
                || (values.Amount <= 0)
                || string.IsNullOrWhiteSpace(values.ReturnUrl)
                || string.IsNullOrWhiteSpace(values.Currency)) {
                throw new Exception("Parameters missing. Required parameters: Reason, Amount, Currency, ReturnUrl.");
            }
            int paymentId = SavePaymentInfo(values);
            values.Id = paymentId;
            string posUrl = GetPosUrl(values);
            values.PosUrl = posUrl;
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
        public void EndPayment(int paymentId, bool success, string error, string info) {
            PaymentRecord payment = GetPaymentInfo(paymentId);
            payment.Success = success;
            payment.Error = error;
            payment.Info = info;
            SavePaymentInfo(payment);
            // solleva l'evento di termine della transazione
            if (success) {
                _paymentEventHandler.OnSuccess(payment.ContentItemId);
            }
            else {
                _paymentEventHandler.OnError(payment.ContentItemId);
            }
        }
        /// <summary>
        /// Get the return URL passed to the virtual POS.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        protected abstract string GetPosUrl(PaymentRecord values);
        /// <summary>
        /// Salva il pagamento e eestituisce il PaymentId.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private int SavePaymentInfo(PaymentRecord values) {
            bool create = false;
            if (values.Id == 0) {
                create = true;
            }
            else {
                int num = _repository.Count(x => x.Id == values.Id);
                if (num == 0) {
                    create = true;
                }
            }
            if (create) {
                _repository.Create(values);
            }
            else {
                _repository.Update(values);
            }
            return values.Id;
        }
    }
}