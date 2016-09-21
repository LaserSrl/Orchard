using Laser.Orchard.PaymentGateway.Models;
using Orchard;

/// <summary>
/// This interface describes the methods that will be available in the controllers. Usually, payment gateway
/// implementations will inherit from PosServiceBase rather than from the interface, so that they will have
/// access to default implementations of some of the methods.
/// </summary>
public interface IPosService : IDependency {
    #region Methods to be implemented in each payment gateway
    /// <summary>
    /// Implemented in the services of each payment gateway, to extract the name of the specific payment gateway provider.
    /// </summary>
    /// <returns>A <type>string</type> with the name of the payment gateway.</returns>
    string GetPosName();
    /// <summary>
    /// This method is used (e.g. in web clients) to get the url of an action that will start the payment using a specific
    /// payment gateway. 
    /// </summary>
    /// <param name="paymentId">The id of the record that contains the information about the payment.</param>
    /// <returns>The url of the action that will actually start the payment.</returns>
    string GetPosActionUrl(int paymentId);
    /// <summary>
    /// This method is used (e.g. in web clients) to get the name of the controller responsible for the implementation
    /// of the PosAdminBaseController in the specific payment gateway module. The method is called in the creation of
    /// the admin navigation menu.
    /// </summary>
    /// <returns>A <type>string</type> that is the name of the controller.</returns>
    string GetSettingsControllerName();
    #endregion

    #region Methods implemented in the abstract service base
    string GetPaymentInfoUrl(int paymentId);
    PaymentRecord StartPayment(PaymentRecord values);
    PaymentRecord GetPaymentInfo(int paymentId);
    void EndPayment(int paymentId, bool success, string error, string info, string transactionId = "");
    #endregion
}