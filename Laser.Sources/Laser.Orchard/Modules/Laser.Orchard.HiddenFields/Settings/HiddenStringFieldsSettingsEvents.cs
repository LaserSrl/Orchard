using Laser.Orchard.HiddenFields.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.Settings {
    public class HiddenStringFieldsSettingsEvents : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> PartFieldEditor(
            ContentPartFieldDefinition definition){

                if (definition.FieldDefinition.Name == "HiddenStringField") {
                    var model = new HiddenStringFieldSettingsEventsViewModel {
                        Settings = definition.Settings.GetModel<HiddenStringFieldSettings>()
                    };
                    yield return DefinitionTemplate(model);
                } else {
                    yield break;
                }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(
            ContentPartFieldDefinitionBuilder builder,IUpdateModel updateModel) {
                
            if (builder.FieldType == "HiddenStringField") {
                var model = new HiddenStringFieldSettingsEventsViewModel {
                    Settings = new HiddenStringFieldSettings()
                };
                if (updateModel.TryUpdateModel(model, "HiddenStringFieldSettingsEventsViewModel", null, null)) {
                    builder.WithSetting("HiddenStringFieldSettings.Tokenized", model.Settings.Tokenized.ToString());
                    builder.WithSetting("HiddenStringFieldSettings.TemplateString", model.Settings.TemplateString);

                    yield return DefinitionTemplate(model);
                }
            } else {
                yield break;
            }
        }
    }
}