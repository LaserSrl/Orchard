using Laser.Orchard.DataProtection.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.DataProtection.Settings {
    public class DataContextPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "DataContextPart") yield break;
            var model = definition.Settings.GetModel<DataContextPartSettings>();
            yield return DefinitionTemplate(model);
        }
        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "DataContextPart") yield break;
            var model = new DataContextPartSettings();
            updateModel.TryUpdateModel(model, "DataContextPartSettings", null, null);
            builder.WithSetting("DataContextPartSettings.ContextDefault", model.ContextDefault);
            builder.WithSetting("DataContextPartSettings.ForceDefault", ((bool)model.ForceDefault).ToString());
            yield return DefinitionTemplate(model);
        }
    }
}