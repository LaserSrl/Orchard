using Laser.Orchard.HID.Attributes;
using Laser.Orchard.HID.Extensions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace Laser.Orchard.HID.Models {
    public class HIDSiteSettingsPart : ContentPart {

        public HIDSiteSettingsPart() {
            PartNumberSets = new List<HIDPartNumberSet>();
        }

        /// <summary>
        /// Numeric identifier of the company in HID's systems
        /// </summary>
        public int CustomerID {
            get { return this.Retrieve(x => x.CustomerID); }
            set { this.Store(x => x.CustomerID, value); }
        }

        public bool UseTestEnvironment {
            get { return this.Retrieve(x => x.UseTestEnvironment); }
            set { this.Store(x => x.UseTestEnvironment, value); }
        }

        /// <summary>
        /// Username of the account responsible to manage credentials.
        /// </summary>
        public string ClientID {
            get { return this.Retrieve(x => x.ClientID); }
            set { this.Store(x => x.ClientID, value); }
        }

        private readonly ComputedField<string> _clientSecret = new ComputedField<string>();

        public ComputedField<string> ClientSecretField {
            get { return _clientSecret; }
        }

        /// <summary>
        /// Password for the account responsible to manage credentials.
        /// </summary>
        public string ClientSecret {
            get { return _clientSecret.Value; }
            set { _clientSecret.Value = value; }
        }
        
        [PartNumberSetsValidation]
        public IList<HIDPartNumberSet> PartNumberSets { get; set; }

        /// <summary>
        /// Part numbers managed by the system.
        /// </summary>
        public string[] PartNumbers {
            get {
                return PartNumberSets
                    .Where(pns => !pns.Delete)
                    .Select(pns => pns.PartNumbers.ToList())
                    .Aggregate((first, second) => { first.AddRange(second); return first; })
                    .ToArray();
            }
        }

        public string _appVersionStrings { get; set; }
        
        /// <summary>
        /// These strings identify the apps connected to the system, and are used in order to avoid doing something
        /// to credential containers that may belong to a user we are managing for our application
        /// </summary>
        public string[] AppVersionStrings {
            get { return Helpers.NumbersStringToArray(this.Retrieve(x => x._appVersionStrings)).ToArray(); }
            set { this.Store(x => x._appVersionStrings, Helpers.NumbersArrayToString(value)); }
        }

        public string SerializedAppVersionStrings {
            get { return (AppVersionStrings != null && AppVersionStrings.Length > 0) ? String.Join(Environment.NewLine, AppVersionStrings) : ""; }
            set { AppVersionStrings = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(avs => avs.Trim()).ToArray(); }
        }
        
    }
}