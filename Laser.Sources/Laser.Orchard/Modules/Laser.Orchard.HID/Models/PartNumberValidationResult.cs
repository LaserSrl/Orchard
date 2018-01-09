using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Models {
    public class PartNumberValidationResult {

        public string Message { get; set; }
        public bool Success { get; set; }
        public PartNumberError Error {get;set;}

        public static PartNumberValidationResult SuccessResult() {
            return new PartNumberValidationResult {
                Success = true,
                Error = PartNumberError.NoError,
                Message = string.Empty
            };
        }
    }
}