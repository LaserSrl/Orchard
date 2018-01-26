using Laser.Orchard.HID.Extensions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;
using System;
using System.Linq;

namespace Laser.Orchard.HID.Models {
    public class HIDSiteSettingsPart : ContentPart {
        
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

        /// <summary>
        /// This setting tells us whether we are allowed to create additional invitation codes for a user
        /// when they already have active credential containers. There is no clear way to verify whether there
        /// are upstanding invitation codes for the user, so we will not do that check (i.e. a single user may
        /// have several "active" invitation codes, and thus may be able to create additional credential
        /// containers).
        /// </summary>
        public bool PreventMoreThanOneDevice {
            get { return this.Retrieve(x => x.PreventMoreThanOneDevice); }
            set { this.Store(x => x.PreventMoreThanOneDevice, value); }
        }

    }
}