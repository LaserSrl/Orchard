namespace Laser.Orchard.HID.Models {
    public class PartNumberValidationResult {

        public string Message { get; set; }
        public bool Success { get; set; }
        public PartNumberError Error {get;set;}

        public static PartNumberValidationResult SuccessResult(string message = null) {
            return new PartNumberValidationResult {
                Success = true,
                Error = PartNumberError.NoError,
                Message = message ?? string.Empty
            };
        }
    }
}