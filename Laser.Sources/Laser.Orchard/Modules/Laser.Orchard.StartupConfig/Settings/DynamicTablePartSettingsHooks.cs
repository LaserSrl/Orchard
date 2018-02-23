using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;
using System.Globalization;

namespace Laser.Orchard.StartupConfig.Settings {
    public class DynamicTablePartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "DynamicTablePart") yield break;
            var model = definition.Settings.GetModel<DynamicTablePartSettings>();
            yield return DefinitionTemplate(model);
        }
        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "DynamicTablePart") yield break;
            var model = new DynamicTablePartSettings();
            updateModel.TryUpdateModel(model, "DynamicTablePartSettings", null, null);
            // carica ogni campo dei settings
            builder.WithSetting("DynamicTablePartSettings.ColumnsDefinition", model.ColumnsDefinition);
            builder.WithSetting("DynamicTablePartSettings.UniqueId", model.UniqueId);
            builder.WithSetting("DynamicTablePartSettings.CardView", model.CardView.ToString(CultureInfo.InvariantCulture));
            yield return DefinitionTemplate(model);
        }
    }
}