﻿using Laser.Orchard.MultiStepAuthentication.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication.Drivers {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class NonceTemplateSettingsPartDriver : ContentPartDriver<NonceLoginSettingsPart> {
        private const string TemplateName = "Parts/NonceTemplateSettingss";
        public Localizer T { get; set; }
        protected override string Prefix { get { return "NonceTemplateSettings"; } }

        public NonceTemplateSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }
        protected override DriverResult Editor(NonceLoginSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(NonceLoginSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_NonceTemplateSettings_Edit", () => {
                if (updater != null) {
                    updater.TryUpdateModel(part, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
            }).OnGroup("NonceLoginSettings");
        }
    }
}