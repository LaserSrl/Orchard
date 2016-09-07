using Laser.Orchard.PaymentGateway.Models;
using Orchard;

public interface IPosService : IDependency {
    string GetPosName();
    string GetPosUrl(PaymentRecord values);
    PaymentRecord StartPayment(PaymentRecord values);
    PaymentRecord GetPaymentInfo(int paymentId);
    void EndPayment(int paymentId, bool success, string error, string info);
}