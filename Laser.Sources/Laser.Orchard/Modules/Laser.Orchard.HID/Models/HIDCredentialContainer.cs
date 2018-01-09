using Laser.Orchard.HID.Extensions;
using Laser.Orchard.HID.Services;
using Newtonsoft.Json.Linq;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Laser.Orchard.HID.Models {
    public class HIDCredentialContainer {
        public int Id { get; set; } //id of container in HID systems
        public string Status { get; set; }
        public string OsVersion { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string ApplicationVersion { get; set; }
        public string SimOperator { get; set; }
        public string BluetoothCapability { get; set; }
        public string NfcCapability { get; set; }
        public List<HIDCredential> Credentials { get; set; }
        public CredentialErrors Error { get; set; }

        public ILogger Logger { get; set; }

        public HIDCredentialContainer() {
            Credentials = new List<HIDCredential>();
            Error = CredentialErrors.NoError;
            Logger = NullLogger.Instance;
        }

        public HIDCredentialContainer(JToken container, IHIDAdminService _HIDService)
            : this() {
            Id = int.Parse(container["id"].ToString()); //no null-check for required properties
            Status = container["status"].ToString().ToUpperInvariant();
            OsVersion = container["osVersion"]  != null ? container["osVersion"].ToString() : "";
            Manufacturer = container["manufacturer"].ToString();
            Model = container["model"].ToString();
            ApplicationVersion = container["applicationVersion"].ToString();
            SimOperator = container["simOperator"] != null ? container["simOperator"].ToString() : "";
            BluetoothCapability = container["bluetoothCapability"] != null ? container["bluetoothCapability"].ToString() : "";
            NfcCapability = container["nfcCapability"] != null ? container["nfcCapability"].ToString() : "";
            if (container["urn:hid:scim:api:ma:1.0:Credential"] != null) { //The Container contains credentials
                var pNums = _HIDService.GetSiteSettings().PartNumbers;
                Credentials.AddRange(
                    container["urn:hid:scim:api:ma:1.0:Credential"]
                    .Children()
                    .Select(jt => new HIDCredential(jt))
                    .Where(cred => pNums.Any(pn => pn == cred.PartNumber)) //only the credentials that we may be responsible for
                    );
            }
        }

        public void UpdateContainer(JToken container, IHIDAdminService _HIDService) {
            Id = int.Parse(container["id"].ToString()); //no null-check for required properties
            Status = container["status"].ToString();
            OsVersion = container["osVersion"] != null ? container["osVersion"].ToString() : "";
            Manufacturer = container["manufacturer"].ToString();
            Model = container["model"].ToString();
            ApplicationVersion = container["applicationVersion"].ToString();
            SimOperator = container["simOperator"] != null ? container["simOperator"].ToString() : "";
            BluetoothCapability = container["bluetoothCapability"] != null ? container["bluetoothCapability"].ToString() : "";
            NfcCapability = container["nfcCapability"] != null ? container["nfcCapability"].ToString() : "";
            Credentials.Clear();
            if (container["urn:hid:scim:api:ma:1.0:Credential"] != null) {
                var pNums = _HIDService.GetSiteSettings().PartNumbers;
                Credentials.AddRange(
                    container["urn:hid:scim:api:ma:1.0:Credential"]
                    .Children()
                    .Select(jt => new HIDCredential(jt))
                    .Where(cred => pNums.Any(pn => pn == cred.PartNumber)) //only the credentials that we may be responsible for
                    );
            }
        }

        /// <summary>
        /// Adds the serialized credential to the Container
        /// </summary>
        /// <param name="credential">A JTOken representing the serialized Credential.</param>
        public void Add(JToken credential) {
            this.Add(new HIDCredential(credential));
        }
        /// <summary>
        /// Adds the credential to the Container
        /// </summary>
        /// <param name="credential">The credential to add.</param>
        public void Add(HIDCredential credential) {
            Credentials.Add(credential);
        }

        /// <summary>
        /// Get the format string for the Endpoint to use when issuing credentials.
        /// </summary>
        /// <param name="_HIDService">IHIDAPIService implementation to be used to get the base endpoints.</param>
        /// <returns>A string to use as format, where the only missing parameter is the Container's Id</returns>
        public string IssueCredentialEndpointFormat(IHIDAdminService _HIDService) {
            return string.Format(HIDAPIEndpoints.IssueCredentialEndpointFormat, 
                _HIDService.BaseEndpoint, @"{0}");
        }
        private const string IssueCredentialBodyFormat = 
            @"{{ 'schemas':[ 'urn:hid:scim:api:ma:1.0:UserAction' ], 'urn:hid:scim:api:ma:1.0:UserAction':{{ 'assignCredential':'Y', 'partNumber':'{0}', 'credential':'' }} }}";
        private string IssueCredentialBody(string pn) {
            return JObject.Parse(string.Format(IssueCredentialBodyFormat, pn)).ToString();
        }

        public HIDCredentialContainer IssueCredential(string partNumber, IHIDAdminService _HIDService) {
            if (!_HIDService.VerifyAuthentication()) {
                Error = CredentialErrors.AuthorizationFailed;
                return this;
            }

            // Configure call
            HttpWebRequest wr = HttpWebRequest.CreateHttp(
                string.Format(IssueCredentialEndpointFormat(_HIDService), Id));
            wr.Method = WebRequestMethods.Http.Post;
            wr.ContentType = Constants.DefaultContentType;
            wr.Headers.Add(HttpRequestHeader.Authorization, _HIDService.AuthorizationToken);
            byte[] bodyData = Encoding.UTF8.GetBytes(IssueCredentialBody(partNumber));
            using (Stream reqStream = wr.GetRequestStream()) {
                reqStream.Write(bodyData, 0, bodyData.Length);
            }
            //handle call
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        // We trust that the HID API responds as documented
                        using (var reader = new StreamReader(resp.GetResponseStream())) {
                            string respJson = reader.ReadToEnd();
                            JObject json = JObject.Parse(respJson);
                            this.Add(json);
                        }
                        Error = CredentialErrors.NoError;
                    }
                }
            } catch (WebException ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)(ex.Response);
                if (resp != null) {
                    if (resp.StatusCode == HttpStatusCode.Unauthorized) {
                        Error = CredentialErrors.AuthorizationFailed;
                    } else if (resp.StatusCode == HttpStatusCode.PreconditionFailed) {
                        var rBody = (new StreamReader(resp.GetResponseStream())).ReadToEnd();
                        if (JObject.Parse(rBody)["detail"].ToString().Trim().ToUpperInvariant() == "THIS CREDENTIAL IS ALREADY DELIVERED TO THIS CREDENTIALCONTAINER.") {
                            Error = CredentialErrors.CredentialDeliveredAlready; // This is barely an error
                        } else {
                            Error = CredentialErrors.UnknownError;
                        }
                    } else {
                        Error = CredentialErrors.UnknownError;
                    }
                } else {
                    Error = CredentialErrors.UnknownError;
                }
            } catch (Exception ex) {
                Error = CredentialErrors.UnknownError;
                Logger.Error(ex, "Fallback error management.");
            }

            return this;
        }

        private string GetCredentialContainerEndpointFormat(IHIDAdminService _HIDService) {
            return String.Format(HIDAPIEndpoints.GetCredentialContainerEndpointFormat, _HIDService.BaseEndpoint, @"{0}");
        }

        public HIDCredentialContainer RevokeCredentials(string partNumber, IHIDAdminService _HIDService) {
            if (!_HIDService.VerifyAuthentication()) {
                Error = CredentialErrors.AuthorizationFailed;
                return this;
            }

            // Update this container to ensure all its information is accurate
            HttpWebRequest wr = HttpWebRequest.CreateHttp(string.Format(GetCredentialContainerEndpointFormat(_HIDService), Id));
            wr.Method = WebRequestMethods.Http.Get;
            wr.ContentType = Constants.DefaultContentType;
            wr.Headers.Add(HttpRequestHeader.Authorization, _HIDService.AuthorizationToken);

            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        using (var reader = new StreamReader(resp.GetResponseStream())) {
                            string respJson = reader.ReadToEnd();
                            JObject json = JObject.Parse(respJson);
                            // Update this container to ensure all its information is accurate
                            UpdateContainer(json["urn:hid:scim:api:ma:1.0:CredentialContainer"]
                                .Children().First(), _HIDService);
                            // Select the credentials we should actually revoke
                            var credentialsToRevoke = Credentials
                                .Where(cred => 
                                    cred.Status.ToUpperInvariant() != "REVOKING" 
                                    && cred.Status.ToUpperInvariant() != "REVOKE_INITIATED");
                            if (!string.IsNullOrWhiteSpace(partNumber)) {
                                credentialsToRevoke = Credentials.Where(cred => cred.PartNumber == partNumber);
                            }
                            Error = CredentialErrors.NoError;
                            foreach (var credential in credentialsToRevoke) {
                                var cerror = credential.Revoke(_HIDService);
                                if (cerror != CredentialErrors.NoError) {
                                    Error = cerror;
                                }
                            }
                        }
                    }
                }
            } catch (WebException ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)(ex.Response);
                Error = CredentialErrors.UnknownError;
                if (resp != null) {
                    if (resp.StatusCode == HttpStatusCode.Unauthorized) {
                        // Authentication could have expired while this method was running
                        if (_HIDService.Authenticate() == AuthenticationErrors.NoError) {
                            return RevokeCredentials(partNumber, _HIDService);
                        }
                        Error = CredentialErrors.AuthorizationFailed;
                    } else {
                        throw ex; // This should be handled, e.g. in the HIDUser, by analysing the status code
                    }
                }
            } catch (Exception ex) {
                Error = CredentialErrors.UnknownError;
                Logger.Error(ex, "Fallback error management.");
            }

            return this;
        }

    }

}