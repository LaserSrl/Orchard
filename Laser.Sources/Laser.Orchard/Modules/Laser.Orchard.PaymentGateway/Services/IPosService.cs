using Laser.Orchard.PaymentGateway.Models;
using Orchard;
using System.Collections.Generic;

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
    /// <summary>
    /// This method is used to retrieve directly the URL of the virtual pos of the paymen gateway, i.e. the address 
    /// towards which a client would be redirected to actually pay.
    /// </summary>
    /// <param name="paymentId">The id of the record that contains the information about the payment.</param>
    /// <returns>The url of the virtual POS where the payment should happen.</returns>
    string GetPosUrl(int paymentId);
    /// <summary>
    /// This method is used to retrieve directly the URL of the virtual pos of the paymen gateway, i.e. the address 
    /// towards which a client would be redirected to actually pay.
    /// </summary>
    /// <param name="paymentId">The id of the record that contains the information about the payment.</param>
    /// <param name="redirectUrl">We may want, after the virtual POS returns to us with a call to an Action to
    /// notify us that its job is done, to redirect the browser to a different Url. This paramater tells us that URL,
    /// where we will place a description of the transaction results. This is validated as starting with either http://
    /// or https://</param>
    /// <param name="schema">Similarly to the redirectUrl, we may want to use a different schema than an http URL,
    /// e.g. when we are redirecting inside a mobile app. This parameter tells us that schema, to which we'll append
    /// a description of the transaction result, serialized into a JSON. If there is a redirectUrl, the schema is ignored.</param>
    /// <returns>The url of the virtual POS where the payment should happen.</returns>
    string GetPosUrl(int paymentId, string redirectUrl, string schema);
    #endregion

    #region Methods implemented in the abstract service base
    string GetPaymentInfoUrl(int paymentId);
    PaymentRecord StartPayment(PaymentRecord values);
    PaymentRecord GetPaymentInfo(int paymentId);
    void EndPayment(int paymentId, bool success, string error, string info, string transactionId = "");
    /// <summary>
    /// Gets a list of all available currencies for a POS, in ISO alpha-3 format
    /// </summary>
    /// <returns></returns>
    List<string> GetAllValidCurrencies();
    #endregion
}