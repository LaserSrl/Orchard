using Laser.Orchard.HID.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
                Credentials.AddRange(container["urn:hid:scim:api:ma:1.0:Credential"].Children().Select(jt => new HIDCredential(jt)));
            }
        }

        public void Add(JToken credential) {
            this.Add(new HIDCredential(credential));
        }
        public void Add(HIDCredential credential) {
            Credentials.Add(credential);
        }
    }
}