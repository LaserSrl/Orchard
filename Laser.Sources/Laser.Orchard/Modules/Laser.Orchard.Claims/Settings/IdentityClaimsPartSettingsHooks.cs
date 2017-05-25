using Laser.Orchard.Claims.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;

namespace Laser.Orchard.Claims.Settings {
    public class IdentityClaimsPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "IdentityClaimsPart") yield break;
            var model = definition.Settings.GetModel<IdentityClaimsPartSettings>();
            yield return DefinitionTemplate(model);
        }
        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "IdentityClaimsPart") yield break;
            var model = new IdentityClaimsPartSettings();
            updateModel.TryUpdateModel(model, "IdentityClaimsPartSettings", null, null);
            builder.WithSetting("IdentityClaimsPartSettings.ClaimsDefault", model.ClaimsDefault);
            builder.WithSetting("IdentityClaimsPartSettings.ForceDefault", ((bool)model.ForceDefault).ToString());
            yield return DefinitionTemplate(model);
        }
    }
}