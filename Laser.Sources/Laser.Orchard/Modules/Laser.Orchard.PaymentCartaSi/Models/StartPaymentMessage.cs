using Laser.Orchard.PaymentCartaSi.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi.Models {
    public class StartPaymentMessage {
        [Required]
        [StringLength(30)]
        public string ailas { get; set; } //shop identifier (constant given by CartaSì)
        [StringLength(7)]
        [ValidAmount]
        public string importo { get; set; } //amount expressed in euro-cents. (50€ become 5000). No decimal separator.
        [StringLength(3)]
        public string divisa { get; set; } //alpha3 code of the currency used for transaction. CartaSì only accepts "EUR"
        [Required]
        [StringLength(30, MinimumLength = 2)]
        [NoOctothorpe]
        public string codTrans { get; set; } //unique transaction identifier
        [Required]
        [StringLength(500)]
        [IsValidUrl]
        public string url { get; set; } //url where the client will be redirected after transaction completes
        [Required]
        [StringLength(200)]
        [IsValidUrl]
        public string url_back { get; set; } //url where the client will be redirect after an error, or after abandoning the payment
        [Required]
        [StringLength(40)]
        public string mac { get; set; } //message code authentication field
        [StringLength(150)]
        public string mail { get; set; } //buyer's email address where we want to send the payment's outcome
        [StringLength(7)]
        public string languageId { get; set; } //language identifier, from the corresponding table, for the POS
        [StringLength(500)]
        [IsValidUrl]
        public string urlpost { get; set; } //url for server-to-server transaction where the POS will send the transaction result
        [StringLength(30)]
        public string num_contratto { get; set; } //unique merchant-side identifier for the POS-side archive where credit card data is stored
        [StringLength(30)]
        public string tipo_servizio { get; set; } //for recurring payments or OneClickPay
        [StringLength(2)]
        public string tipo_richiesta { get; set; } //"PP" (primo pagamento), "PR" (pagamento ricorrente), "PA" (pagamento singolo)
        [StringLength(30, MinimumLength = 5)]
        public string gruppo { get; set; }
        [StringLength(2000)]
        public string descrizione { get; set; } //Description of service. It will be in the email sent to the cardholder. max 140 characters for MyBank
        [StringLength(100)]
        public string session_id { get; set; } //session identifier
        [StringLength(200)]
        public string Note1 { get; set; } //informations about the order. Reported in CartaSì back office
        [StringLength(200)]
        public string Note2 { get; set; } //informations about the order. Reported in CartaSì back office
        [StringLength(200)]
        public string Note3 { get; set; } //informations about the order. Reported in CartaSì back office
        [StringLength(4000)]
        private string _additionalParameters; //additional custom parameters that will be reported in the outcome message. Some parameter names are reserved.
        public string AdditionalParameters {
            get { return _additionalParameters; }
            set{this._additionalParameters=value;} //TODO: parse this into the dictionary below for validation
        }
        public Dictionary<string, string> AdditionalParametersDictionary {
            get; //parse _additionalParameters into a Dictionary
            set; //parse the dictionary into _additionalParameters, taking care of invalid parameters.
        }
    }
}