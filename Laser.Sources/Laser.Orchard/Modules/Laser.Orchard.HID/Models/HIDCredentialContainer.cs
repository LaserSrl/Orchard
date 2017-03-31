using Laser.Orchard.HID.Extensions;
using Laser.Orchard.HID.Services;
using Newtonsoft.Json.Linq;
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

        public HIDCredentialContainer() {
            Credentials = new List<HIDCredential>();
            Error = CredentialErrors.NoError;
        }

        public HIDCredentialContainer(JToken container, IHIDAPIService _HIDService)
            : this() {
            Id = int.Parse(container["id"].ToString());
            Status = container["status"].ToString().ToUpperInvariant();
            OsVersion = container["osVersion"].ToString();
            Manufacturer = container["manufacturer"].ToString();
            Model = container["model"].ToString();
            ApplicationVersion = container["applicationVersion"].ToString();
            SimOperator = container["simOperator"].ToString();
            BluetoothCapability = container["bluetoothCapability"].ToString();
            NfcCapability = container["nfcCapability"].ToString();
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

        public void UpdateContainer(JToken container, IHIDAPIService _HIDService) {
            Id = int.Parse(container["id"].ToString());
            Status = container["status"].ToString();
            OsVersion = container["osVersion"].ToString();
            Manufacturer = container["manufacturer"].ToString();
            Model = container["model"].ToString();
            ApplicationVersion = container["applicationVersion"].ToString();
            SimOperator = container["simOperator"].ToString();
            BluetoothCapability = container["bluetoothCapability"].ToString();
            NfcCapability = container["nfcCapability"].ToString();
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

        public void Add(JToken credential) {
            this.Add(new HIDCredential(credential));
        }
        public void Add(HIDCredential credential) {
            Credentials.Add(credential);
        }


        public string IssueCredentialEndpointFormat(IHIDAPIService _HIDService) {
            return string.Format(HIDAPIEndpoints.IssueCredentialEndpointFormat, _HIDService.BaseEndpoint, @"{0}");
        }
        private const string IssueCredentialBodyFormat = @"{{ 'schemas':[ 'urn:hid:scim:api:ma:1.0:UserAction' ], 'urn:hid:scim:api:ma:1.0:UserAction':{{ 'assignCredential':'Y', 'partNumber':'{0}', 'credential':'' }} }}";
        private string IssueCredentialBody(string pn) {
            return JObject.Parse(string.Format(IssueCredentialBodyFormat, pn)).ToString();
        }

        public HIDCredentialContainer IssueCredential(string partNumber, HIDUser user, IHIDAPIService _HIDService) {
            if (string.IsNullOrWhiteSpace(_HIDService.AuthorizationToken)) {
                if (_HIDService.Authenticate() != AuthenticationErrors.NoError) {
                    Error = CredentialErrors.AuthorizationFailed;
                    return this;
                }
            }
            // Configure call
            HttpWebRequest wr = HttpWebRequest.CreateHttp(
                string.Format(IssueCredentialEndpointFormat(_HIDService), Id));
            wr.Method = WebRequestMethods.Http.Post;
            wr.ContentType = "application/vnd.assaabloy.ma.credential-management-1.0+json";
            wr.Headers.Add(HttpRequestHeader.Authorization, _HIDService.AuthorizationToken);
            byte[] bodyData = Encoding.UTF8.GetBytes(IssueCredentialBody(partNumber));
            using (Stream reqStream = wr.GetRequestStream()) {
                reqStream.Write(bodyData, 0, bodyData.Length);
            }
            //handle call
            try {
                using (HttpWebResponse resp = wr.GetResponse() as HttpWebResponse) {
                    if (resp.StatusCode == HttpStatusCode.OK) {
                        using (var reader = new StreamReader(resp.GetResponseStream())) {
                            string respJson = reader.ReadToEnd();
                            JObject json = JObject.Parse(respJson);
                            this.Add(json);
                        }
                        Error = CredentialErrors.NoError;
                    }
                }
            } catch (Exception ex) {
                HttpWebResponse resp = (System.Net.HttpWebResponse)((System.Net.WebException)ex).Response;
                if (resp != null) {
                    if (resp.StatusCode == HttpStatusCode.Unauthorized) {
                        Error = CredentialErrors.AuthorizationFailed;
                    } else if (resp.StatusCode == HttpStatusCode.PreconditionFailed) {
                        var rBody = (new StreamReader(resp.GetResponseStream())).ReadToEnd();
                        if (JObject.Parse(rBody)["detail"].ToString().Trim().ToUpperInvariant() == "THIS CREDENTIAL IS ALREADY DELIVERED TO THIS CREDENTIALCONTAINER.") {
                            Error = CredentialErrors.CredentialDeliveredAlready;
                        } else {
                            Error = CredentialErrors.UnknownError;
                        }
                    } else {
                        Error = CredentialErrors.UnknownError;
                    }
                } else {
                    Error = CredentialErrors.UnknownError;
                }
            }

            return this;
        }
    }

    public class HIDCredentialContainerEqualityComparer : IEqualityComparer<HIDCredentialContainer> {
        public bool Equals(HIDCredentialContainer cc1, HIDCredentialContainer cc2) {
            if (Object.ReferenceEquals(cc1, cc2)) return true;
            if (Object.ReferenceEquals(cc1, null) || Object.ReferenceEquals(cc2, null))
                return false;

            return (cc1.Manufacturer == cc2.Manufacturer && cc1.Model == cc2.Model);
        }
        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public int GetHashCode(HIDCredentialContainer cc) {
            if (Object.ReferenceEquals(cc, null)) return 0;
            
            int manufacturerCode = cc.Manufacturer == null ? 0 : cc.Manufacturer.GetHashCode();
            int modelCode = cc.Model == null ? 0 : cc.Model.GetHashCode();

            return manufacturerCode ^ modelCode;
        }
    }

    public class HIDCredentialContainerModelEqualityComparer : IEqualityComparer<HIDCredentialContainer> {
        public bool Equals(HIDCredentialContainer cc1, HIDCredentialContainer cc2) {
            if (Object.ReferenceEquals(cc1, cc2)) return true;
            if (Object.ReferenceEquals(cc1, null) || Object.ReferenceEquals(cc2, null))
                return false;

            return cc1.Model == cc2.Model;
        }
        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public int GetHashCode(HIDCredentialContainer cc) {
            if (Object.ReferenceEquals(cc, null)) return 0;

            int modelCode = cc.Model == null ? 0 : cc.Model.GetHashCode();

            return modelCode;
        }
    }
}