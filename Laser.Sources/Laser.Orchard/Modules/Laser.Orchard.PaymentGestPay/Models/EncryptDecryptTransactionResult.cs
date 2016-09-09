using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml;

namespace Laser.Orchard.PaymentGestPay.Models {
    public class EncryptDecryptTransactionResult {
        [StringLength(7)]
        public string TransactionType { get; set; } //"DECRYPT" or "ENCRYPT"
        [StringLength(2)]
        [Required]
        public string TransactionResult { get; set; } //"OK", or "KO"
        [Required]
        public string CryptDecryptString { get; set; } //encrypted string
        [StringLength(9)]
        public string ErrorCode { get; set; } //error code
        [StringLength(255)]
        public string ErrorDescription { get; set; } //error description

        public EncryptDecryptTransactionResult(XmlNode xml) {
            TransactionType = xml.SelectSingleNode("TransactionType").InnerText;
            TransactionResult = xml.SelectSingleNode("TransactionResult").InnerText;
            CryptDecryptString = xml.SelectSingleNode("CryptDecryptString").InnerText;
            ErrorCode = xml.SelectSingleNode("ErrorCode").InnerText;
            ErrorDescription = xml.SelectSingleNode("ErrorDescription").InnerText;
        }
    }
}