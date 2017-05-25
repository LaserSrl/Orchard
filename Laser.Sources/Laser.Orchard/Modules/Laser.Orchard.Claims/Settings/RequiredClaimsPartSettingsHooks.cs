using Laser.Orchard.Claims.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Settings {
    public class RequiredClaimsPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "RequiredClaimsPart") yield break;
            var model = definition.Settings.GetModel<RequiredClaimsPartSettings>();
            yield return DefinitionTemplate(model);
        }
        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "RequiredClaimsPart") yield break;
            var model = new RequiredClaimsPartSettings();
            updateModel.TryUpdateModel(model, "RequiredClaimsPartSettings", null, null);
            builder.WithSetting("RequiredClaimsPartSettings.ClaimsDefault", model.ClaimsDefault);
            builder.WithSetting("RequiredClaimsPartSettings.ForceDefault", ((bool)model.ForceDefault).ToString());
            yield return DefinitionTemplate(model);
        }
    }
}