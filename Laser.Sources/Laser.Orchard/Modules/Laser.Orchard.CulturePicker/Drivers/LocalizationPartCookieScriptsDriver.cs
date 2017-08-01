using Orchard.ContentManagement.Drivers;
using Orchard.Localization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.CompilerServices;

namespace Laser.Orchard.CulturePicker.Drivers {
    public class LocalizationPartCookieScriptsDriver : ContentPartDriver<LocalizationPart> {

        protected override DriverResult Display(LocalizationPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "Detail") {
                return ContentShape("Parts_Localization_CultureCookieScripts", () =>
                    shapeHelper.Parts_Localization_CultureCookieScripts(Culture: GetCulture(part)));
            }
            return null;
        }

        private string GetCulture(LocalizationPart part) {
            return part.Culture != null ? part.Culture.Culture : string.Empty;
        }
    }
}