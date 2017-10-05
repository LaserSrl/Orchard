using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.MetaData.Models;
using System.Globalization;

namespace Contrib.Profile.Services {
    /// <summary>
    /// The front end settings are all true for profile part and any field in it.
    /// </summary>
    public class ProfilePartDefaultSettingsProvider : IDefaultFrontEndSettingsProvider {
        public void ConfigureDefaultValues(ContentTypeDefinition definition) {
            var typePartDefinition = definition.Parts
                .FirstOrDefault(ctpd => ctpd.PartDefinition.Name == "ProfilePart");
            if (typePartDefinition != null) {
                SetDefaultValues(typePartDefinition.Settings); //defaultSettings for part
                foreach (var fieldDefinition in typePartDefinition.PartDefinition.Fields) {
                    SetDefaultValues(fieldDefinition.Settings); //default setting for fields
                }
            }
        }

        public IEnumerable<string> ForParts() {
            return new string[] { "ProfilePart" };
        }

        /// <summary>
        /// Set the settings to true
        /// </summary>
        /// <param name="settings"></param>
        private void SetDefaultValues(SettingsDictionary settings) {
            settings["ProfileFrontEndSettings.AllowFrontEndEdit"] =
                true.ToString(CultureInfo.InvariantCulture);
            settings["ProfileFrontEndSettings.AllowFrontEndDisplay"] =
                true.ToString(CultureInfo.InvariantCulture);
        }
    }
}