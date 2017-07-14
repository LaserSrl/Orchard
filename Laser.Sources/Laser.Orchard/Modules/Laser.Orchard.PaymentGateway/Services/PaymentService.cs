using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using Orchard.Data;
using Orchard.Security;
using Orchard.ContentManagement;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.PaymentGateway.Services {
    public class PaymentService : IPaymentService {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<PaymentRecord> _repository;

        public PaymentService(IOrchardServices orchardServices, IRepository<PaymentRecord> repository) {
            _orchardServices = orchardServices;
            _repository = repository;
        }
        public List<PaymentRecord> GetPayments(int userId, bool lastToFirst = true) {
            List<PaymentRecord> result = null;
            IEnumerable<PaymentRecord> records = null;
            if (lastToFirst) {
                records = _repository.Fetch(x => x.UserId == userId, y => y.Desc(z => z.UpdateDate));
            }
            else {
                records = _repository.Fetch(x => x.UserId == userId, y => y.Asc(z => z.UpdateDate));
            }
            result = records.ToList();
            return result;
        }
        public List<PaymentRecord> GetAllPayments(bool lastToFirst = true) {
            List<PaymentRecord> result = null;
            IEnumerable<PaymentRecord> records = null;
            if (lastToFirst) {
                records = _repository.Fetch(x => true, y => y.Desc(z => z.UpdateDate));
            }
            else {
                records = _repository.Fetch(x => true, y => y.Asc(z => z.UpdateDate));
            }
            result = records.ToList();
            return result;
        }
        public PaymentRecord GetPayment(int paymentId) {
            PaymentRecord record = null;
            var list = _repository.Fetch(x => x.Id == paymentId);
            if(list != null) {
                record = list.FirstOrDefault();
            }
            return record;
        }
        public PaymentRecord GetPaymentByTransactionId(string transactionId) {
            PaymentRecord record = null;
            var list = _repository.Fetch(x => x.TransactionId == transactionId);
            if (list != null) {
                record = list.FirstOrDefault();
            }
            return record;
        }
        public PaymentRecord GetPaymentByGuid(string paymentGuid) {
            PaymentRecord record = null;
            var list = _repository.Fetch(x => x.Guid == paymentGuid);
            if (list != null) {
                record = list.FirstOrDefault();
            }
            return record;
        }
    }
}