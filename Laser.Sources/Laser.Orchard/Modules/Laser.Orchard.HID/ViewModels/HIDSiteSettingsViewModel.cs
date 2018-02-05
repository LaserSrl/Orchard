﻿using Laser.Orchard.HID.Attributes;
using Laser.Orchard.HID.Models;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.HID.ViewModels {
    public class HIDSiteSettingsViewModel {

        public HIDSiteSettingsViewModel() {
            PartNumberSets = new List<HIDPartNumberSetViewModel>();
        }

        public HIDSiteSettingsViewModel(HIDSiteSettingsPart settings)
            :this() {
            SettingsPart = settings;
        }

        public HIDSiteSettingsPart SettingsPart { get; set; }

        [PartNumberSetsValidation]
        public IList<HIDPartNumberSetViewModel> PartNumberSets { get; set; }

        /// <summary>
        /// Part numbers managed by the system.
        /// </summary>
        public string[] PartNumbers {
            get {
                return PartNumberSets
                    .Where(pns => !pns.Delete)
                    .SelectMany(pns => pns.PartNumbers.ToList())
                    .ToArray();
            }
        }

        public int CustomerID {
            get { return SettingsPart.CustomerID; }
            set { SettingsPart.CustomerID = value; }
        }

        public bool UseTestEnvironment {
            get { return SettingsPart.UseTestEnvironment; }
            set { SettingsPart.UseTestEnvironment = value; }
        }

        public string ClientID {
            get { return SettingsPart.ClientID; }
            set { SettingsPart.ClientID = value; }
        }

        public string ClientSecret {
            get { return SettingsPart.ClientSecret; }
            set { SettingsPart.ClientSecret = value; }
        }

        public string[] AppVersionStrings {
            get { return SettingsPart.AppVersionStrings; }
            set { SettingsPart.AppVersionStrings = value ?? new string[] { }; }
        }

        public string SerializedAppVersionStrings {
            get { return SettingsPart.SerializedAppVersionStrings; }
            set { SettingsPart.SerializedAppVersionStrings = value ?? string.Empty; }
        }

        public bool PreventMoreThanOneDevice {
            get { return SettingsPart.PreventMoreThanOneDevice; }
            set { SettingsPart.PreventMoreThanOneDevice = value; }
        }
    }
}