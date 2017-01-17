﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Models {
    public class HIDCredentialContainer {
        public int Id { get; private set; } //id of container in HID systems
        public string Status { get; private set; }
        public string OsVersion { get; private set; }
        public string Manufacturer { get; private set; }
        public string Model { get; private set; }
        public string ApplicationVersion { get; private set; }
        public string SimOperator { get; private set; }
        public string BluetoothCapability { get; private set; }
        public string NfcCapability { get; private set; }
        public List<HIDCredential> Credentials { get; private set; }

        private HIDCredentialContainer() {
            Credentials = new List<HIDCredential>();
        }

        public HIDCredentialContainer(JToken container)
            : this() {
            Id = int.Parse(container["id"].ToString());
            Status = container["status"].ToString();
            OsVersion = container["osVersion"].ToString();
            Manufacturer = container["manufacturer"].ToString();
            Model = container["model"].ToString();
            ApplicationVersion = container["applicationVersion"].ToString();
            SimOperator = container["simOperator"].ToString();
            BluetoothCapability = container["bluetoothCapability"].ToString();
            NfcCapability = container["nfcCapability"].ToString();
            if (container["urn:hid:scim:api:ma:1.0:Credential"] != null) {
                Credentials.AddRange(container["urn:hid:scim:api:ma:1.0:Credential"].Children().Select(jt => new HIDCredential(jt)));
            }
        }

        public void UpdateContainer(JToken container){
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