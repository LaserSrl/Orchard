using Laser.Orchard.PaymentCartaSi.Attributes;
using Laser.Orchard.PaymentGateway.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi.Models {
    public class StartPaymentMessage {
        [Required]
        [StringLength(30)]
        public string alias { get; set; } //shop identifier (constant given by CartaSì)
        [StringLength(7)]
        [ValidAmount]
        public string importo { get; set; } //amount expressed in euro-cents. (50€ become 5000). No decimal separator.
        [StringLength(3, MinimumLength = 3)]
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
        [StringLength(40, MinimumLength = 40)]
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
        [StringLength(2, MinimumLength = 2)]
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
        public string AdditionalParameters { //additional custom parameters that will be reported in the outcome message. Some parameter names are reserved.
            get {
                List<string> intermediate = new List<string>();
                foreach (var item in AdditionalParametersDictionary) {
                    intermediate.Add(string.Format("{0}={1}", item.Key, item.Value));
                }
                return string.Join(@"&", intermediate);
            }
            set { //parse this into the dictionary below for validation
                //this string should be formatted as a portion of a query string
                AdditionalParametersDictionary = new Dictionary<string, string>();
                string[] elems = value.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in elems) {
                    //each element should be in the form key=value
                    string[] pair = item.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    //pair should have two elements.
                    if (pair.Length == 1) {
                        //consider null value
                        AdditionalParametersDictionary.Add(pair[0], null);
                    } else if (pair.Length == 2) {
                        //healthy case
                        AdditionalParametersDictionary.Add(pair[0], pair[1]);
                    } else {
                        //too much stuff, so the string is probably messed up. We still add the first as key and the second as value.
                        AdditionalParametersDictionary.Add(pair[0], pair[1]);
                    }
                }
            }
        }
        [IsValidParametersDictionary]
        public Dictionary<string, string> AdditionalParametersDictionary { get; set; }
        [StringLength(16, MinimumLength = 16)]
        public string OPTION_CF { get; set; } //fiscal code of user. Required if check between fiscal code and PAN number is active.
        [StringLength(25)]
        public string selectedcard { get; set; }
        [StringLength(20, MinimumLength = 20)]
        public string TCONTAB { get; set; } //identifies how the transaction has to be managed in terms of payments to the merchant
        [StringLength(35)]
        public string infoc { get; set; } //additional information related to the single payment
        [StringLength(20)]
        public string infob { get; set; } //additional information related to the single payment
        [StringLength(40)]
        public string modo_gestione_consegna { get; set; } //only for payments using MySi wallets

        private string TransactionStartSignature {
            get { return string.Format("codTrans={0}divisa={1}importo={2}{3}", codTrans, divisa, importo, alias); }
        }
        private string MACFromSignature(string sig) {
            byte[] sigBytes = System.Text.Encoding.UTF8.GetBytes(sig);
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] macBytes = sha.ComputeHash(sigBytes);
            return BitConverter.ToString(macBytes).Replace("-", string.Empty);
        }
        public string TransactionStartMAC {
            get {
                return MACFromSignature(TransactionStartSignature);
            }
        }

        public StartPaymentMessage() {
            AdditionalParametersDictionary = new Dictionary<string, string>();
        }
        public StartPaymentMessage(string al)
            : this() {
            alias = al;
        }
        public StartPaymentMessage(string al, PaymentRecord pr)
            : this(al) {
            importo = (pr.Amount * 100).ToString("0");
            divisa = pr.Currency;
            codTrans = pr.Id.ToString();
        }
    }
}