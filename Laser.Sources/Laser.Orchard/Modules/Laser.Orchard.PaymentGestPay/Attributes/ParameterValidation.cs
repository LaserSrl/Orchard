using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.PaymentGestPay.Attributes {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class ValidGestPayParameter : ValidationAttribute {

        /// <summary>
        /// Verifies that the parameter string contains no invalid characters.
        /// </summary>
        /// <param name="parameter"></param>
        internal bool ValidateGestPayParameter(string parameter) {
            if (Regex.IsMatch(parameter, "[& §()*<>,;:\\[ \\]/%?=]")) {
                return false;
            }
            return true;
        }

        public override bool IsValid(object value) {
            string param = (string)value;
            return ValidateGestPayParameter(param);
        }

        public override string FormatErrorMessage(string name) {
            return string.Format("Invalid character in parameter {0}.", name);
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class ValidRedGestPayParameter : ValidationAttribute {

        /// <summary>
        /// Verifies that the parameter string contains no invalid characters.
        /// </summary>
        /// <param name="parameter"></param>
        internal bool ValidateRedParameter(string parameter) {
            if (Regex.IsMatch(parameter, "[$!^~#]")) {
                return false;
            }
            return true;
        }

        public override bool IsValid(object value) {
            string param = (string)value;
            return ValidateRedParameter(param);
        }

        public override string FormatErrorMessage(string name) {
            return string.Format("Invalid character in parameter {0}.", name);
        }
    }
}