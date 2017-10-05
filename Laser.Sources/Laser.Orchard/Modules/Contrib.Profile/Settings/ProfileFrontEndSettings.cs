using Orchard.ContentManagement.MetaData.Models;
using System.Globalization;

namespace Contrib.Profile.Settings {
    public class ProfileFrontEndSettings {
        public bool AllowFrontEndEdit { get; set; } 
        public bool AllowFrontEndDisplay { get; set; }

        public static void SetValues(SettingsDictionary settingsDictionary, bool allowDisplay, bool allowEdit) {
            settingsDictionary["ProfileFrontEndSettings.AllowFrontEndEdit"] =
                allowEdit.ToString(CultureInfo.InvariantCulture);
            settingsDictionary["ProfileFrontEndSettings.AllowFrontEndDisplay"] =
                allowDisplay.ToString(CultureInfo.InvariantCulture);
        }
    }
}