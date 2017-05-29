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
    public class ItemClaimsPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ItemClaimsPart") yield break;
            var model = definition.Settings.GetModel<ItemClaimsPartSettings>();
            yield return DefinitionTemplate(model);
        }
        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ItemClaimsPart") yield break;
            var model = new ItemClaimsPartSettings();
            updateModel.TryUpdateModel(model, "ItemClaimsPartSettings", null, null);
            builder.WithSetting("ItemClaimsPartSettings.ClaimsDefault", model.ClaimsDefault);
            builder.WithSetting("ItemClaimsPartSettings.ForceDefault", ((bool)model.ForceDefault).ToString());
            yield return DefinitionTemplate(model);
        }
    }
}