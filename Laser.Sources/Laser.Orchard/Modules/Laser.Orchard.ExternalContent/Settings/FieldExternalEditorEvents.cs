using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;

namespace Laser.Orchard.ExternalContent.Settings {
    public class FieldExternalEditorEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "FieldExternal") {
                var model = definition.Settings.GetModel<FieldExternalSetting>();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "FieldExternal") {
                yield break;
            }
            var model = new FieldExternalSetting();
            if (updateModel.TryUpdateModel(model, "FieldExternalSetting", null, null)) {
                builder.WithSetting("FieldExternalSetting.Required", model.Required.ToString());
                builder.WithSetting("FieldExternalSetting.ExternalURL", model.ExternalURL);
                builder.WithSetting("FieldExternalSetting.NoFollow", model.NoFollow.ToString());
                builder.WithSetting("FieldExternalSetting.GenerateL", model.GenerateL.ToString());
                builder.WithSetting("FieldExternalSetting.HttpVerb", model.HttpVerb.ToString());

            }
            yield return DefinitionTemplate(model);
        }
    }
}