using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Laser.Orchard.PaymentGateway.Controllers {
    [WebApiKeyFilter(true)]
    public class PaymentGatewayAPIController : ApiController {

        private readonly IEnumerable<IPosService> _posServices;

        public Localizer T { get; set; }

        public PaymentGatewayAPIController(IEnumerable<IPosService> posServices) {
            _posServices = posServices;

            T = NullLocalizer.Instance;
        }

        public PaymentGatewayResponse GetPosNames() {
            PaymentGatewayResponse res = new PaymentGatewayResponse() {
                Success = true,
                ErrorCode = PaymentGatewayErrorCode.NoError,
                Data = new { posNames = AllPosNames() },
                Message = ""
            };
            return res;
        }

        public PaymentGatewayResponse GetValidCurrencies(string posName) {
            var vc = ValidCurrencies(posName);
            if (vc == null || vc.Count == 0) {
                return new PaymentGatewayResponse() {
                    Success = false,
                    Message = T("There are no valid currencies for the POS, or the pos is not valid. See this response's data object for a list of valid POS names").Text,
                    Data = new { posNames = AllPosNames() },
                    ErrorCode = PaymentGatewayErrorCode.PosNotFound,
                    ResolutionAction = PaymentGatewayResolutionAction.UpdatePosNames
                };
            }
            return new PaymentGatewayResponse() {
                Success = true,
                Message = "",
                Data = new { validCurrencies = vc },
                ErrorCode = PaymentGatewayErrorCode.NoError,
                ResolutionAction = PaymentGatewayResolutionAction.NoAction
            };
        }

        /// <summary>
        /// Get the Url of the virtual pos, based on the parameters passed in the call.
        /// </summary>
        /// <param name="posName">Name of the payment gateway whose POS we are trying to reach.</param>
        /// <param name="amount">Amount to be payed.</param>
        /// <param name="currency">Currency of payment.</param>
        /// <param name="itemId">Id of Content Item associated with payment.</param>
        /// <param name="reason">Description of reason for payment.</param>
        /// <returns></returns>
        public PaymentGatewayResponse GetVirtualPosUrl(string posName, decimal amount, string currency, int? itemId = 0, string reason = "") {
            bool success = false;
            string msg = "";
            dynamic data = new System.Dynamic.ExpandoObject();
            PaymentGatewayErrorCode error = PaymentGatewayErrorCode.NoError;
            PaymentGatewayResolutionAction action = PaymentGatewayResolutionAction.NoAction;
            //get pos from posName
            var pos = _posServices.Where(ps => ps.GetPosName() == posName).SingleOrDefault();
            if (pos == null) {
                //ERROR: no pos with that name
                success = false;
                error = PaymentGatewayErrorCode.PosNotFound;
                action = PaymentGatewayResolutionAction.UpdatePosNames;
                data.posNames = AllPosNames();
                msg = T("Could not find a POS called \"{0}\": you may find a list of available POS names in this response's data object.", posName).Text;
            } else {
                //check whether currency is valid
                var vc = ValidCurrencies(pos);
                if (string.IsNullOrWhiteSpace(currency) || !vc.Contains(currency)) {
                    //currency is required
                    success = false;
                    error = PaymentGatewayErrorCode.InvalidCurrency;
                    action = PaymentGatewayResolutionAction.VerifyInformation;
                    data.validCurrencies = vc;
                    msg = T("The currency is required. You may find a list of valid currencies in this response's data object.").Text;
                } else {
                    //create PaymentRecord (using startPayment)
                    PaymentRecord record = null;
                    try {
                        record = pos.StartPayment(new PaymentRecord() {
                            Reason = reason,
                            Amount = amount,
                            Currency = currency,
                            ContentItemId = itemId.Value
                        });
                    } catch (Exception ex) {
                        success = false;
                        error = PaymentGatewayErrorCode.ImpossibleToCreateRecord;
                        action = PaymentGatewayResolutionAction.VerifyInformation;
                        msg = ex.Message;
                    }
                    int paymentId = record.Id;
                    //get the redirect url for the pos
                    try {
                        data.redirectUrl = pos.GetPosUrl(paymentId);
                        success = true;
                    } catch (Exception ex) {
                        //TODO: some payment services may not return a redirect url (e.g. Braintree)
                        //handle this case with an error
                    }
                }
            }

            return new PaymentGatewayResponse() {
                Success = success,
                Message = msg,
                Data = data,
                ErrorCode = error,
                ResolutionAction = action
            };
        }

        #region Private methods
        private List<string> AllPosNames() {
            List<string> posNames = new List<string>();
            foreach (var service in _posServices) {
                posNames.Add(service.GetPosName());
            }
            return posNames;
        }
        private List<string> ValidCurrencies(string posName) {
            //get the pos from the posName
            var pos = _posServices.Where(ps => ps.GetPosName() == posName).SingleOrDefault();
            if (pos == null)
                return null;
            return ValidCurrencies(pos);
        }
        private List<string> ValidCurrencies(IPosService pos) {
            return pos.GetAllValidCurrencies();
        }
        #endregion

        #region Responses
        //we extend Laser.Orchard.StartupConfig.ViewModels.Response for error codes specific to PaymentGateway
        public enum PaymentGatewayErrorCode {
            NoError = 0, GenericError = 1,
            PosNotFound = 5001, //No POS was found with the given name
            InvalidCurrency = 5002, //the selected POS does not support this currency
            ImpossibleToCreateRecord = 5003 //attempt to create the record failed
        }
        public enum PaymentGatewayResolutionAction {
            NoAction = 0,
            TryAgain = 5001, //it might have been a temporary error
            UpdatePosNames = 5002, //Update the list of available pos names
            VerifyInformation = 5003 //some parameter was not valid. 

        }
        public class PaymentGatewayResponse : Response {
            new public PaymentGatewayErrorCode ErrorCode { get; set; }
            new public PaymentGatewayResolutionAction ResolutionAction { get; set; }

            public PaymentGatewayResponse() {
                this.ErrorCode = PaymentGatewayErrorCode.GenericError;
                this.Success = false;
                this.Message = "Generic Error";
                this.ResolutionAction = PaymentGatewayResolutionAction.NoAction;
            }
        }
        #endregion
    }
}