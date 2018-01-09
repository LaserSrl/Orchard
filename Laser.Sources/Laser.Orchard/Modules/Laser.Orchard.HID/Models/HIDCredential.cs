using Laser.Orchard.HID.Extensions;
using Laser.Orchard.HID.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace Laser.Orchard.HID.Models {
    public class HIDCredential {
        public int Id { get; set; } //id of credential in HID systems
        public string PartNumber { get; set; }
        public string PartNumberFriendlyName { get; set; }
        public string CardNumber { get; set; } //Access Control Number
        public string Status { get; set; }

        public HIDCredential() { }

        public HIDCredential(JToken credential)
            : this() {
            Id = int.Parse(credential["id"].ToString()); // No null-checks for required properties
            PartNumber = credential["partNumber"].ToString();
            PartNumberFriendlyName = credential["partnumberFriendlyName"] != null ? credential["partnumberFriendlyName"].ToString() : "";
            CardNumber = credential["cardNumber"] != null ? credential["cardNumber"].ToString() : ""; 
            Status = credential["status"].ToString().ToUpperInvariant();
        }

        private string RevokeCredentialEndpointFormat(IHIDAdminService _HIDService) {
            return string.Format(HIDAPIEndpoints.RevokeCredentialEndpointFormat, _HIDService.BaseEndpoint, @"{0}");
        }

        public CredentialErrors Revoke(IHIDAdminService _HIDService) {
            if (!_HIDService.VerifyAuthentication()) {
                return CredentialErrors.AuthorizationFailed;
            }

            HttpWebRequest wrRevoke = HttpWebRequest.CreateHttp(string.Format(RevokeCredentialEndpointFormat(_HIDService), Id));
            wrRevoke.Method = "DELETE";
            wrRevoke.Headers.Add(HttpRequestHeader.Authorization, _HIDService.AuthorizationToken);
            try {
                using (HttpWebResponse respRevoke = wrRevoke.GetResponse() as HttpWebResponse) {
                    if (respRevoke.StatusCode == HttpStatusCode.NoContent || respRevoke.StatusCode == HttpStatusCode.OK) {
                        Status = "REVOKING";
                        return CredentialErrors.NoError;
                    } else {
                        return CredentialErrors.UnknownError;
                    }
                }
            } catch (WebException ex) {
                HttpWebResponse respRevoke = (System.Net.HttpWebResponse)(ex.Response);
                if (respRevoke != null) {
                    if (respRevoke.StatusCode == HttpStatusCode.Unauthorized) {
                        // Authentication could have expired while this method was running
                        if (_HIDService.Authenticate() == AuthenticationErrors.NoError) {
                            return Revoke(_HIDService);
                        }
                        return CredentialErrors.AuthorizationFailed;
                    } else if (respRevoke.StatusCode == HttpStatusCode.PreconditionFailed) {
                        return CredentialErrors.NoError; // revoked/ing already
                    } else {
                        throw ex; // This should be handled, e.g. in the HIDUser, by analysing the status code
                    }
                } else {
                    return CredentialErrors.UnknownError;
                }
            } catch (Exception) {
                // Fallback error management.
                return CredentialErrors.UnknownError;
            }
        }

    }
}