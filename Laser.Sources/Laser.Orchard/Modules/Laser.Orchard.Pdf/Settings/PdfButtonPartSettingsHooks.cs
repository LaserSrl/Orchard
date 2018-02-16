using Laser.Orchard.Pdf.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

namespace Laser.Orchard.Pdf.Settings {
    public class PdfButtonPartSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "PdfButtonPart") yield break;
            var model = definition.Settings.GetModel<PdfButtonPartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "PdfButtonPart") yield break;
            var model = new PdfButtonPartSettings();
            updateModel.TryUpdateModel(model, "PdfButtonPartSettings", null, null);

            // carica ogni campo dei settings
            builder.WithSetting("PdfButtonPartSettings.TemplateId", model.TemplateId.ToString());
            builder.WithSetting("PdfButtonPartSettings.FileNameWithoutExtension", model.FileNameWithoutExtension);
            builder.WithSetting("PdfButtonPartSettings.Header", model.Header);
            builder.WithSetting("PdfButtonPartSettings.Footer", model.Footer);
            builder.WithSetting("PdfButtonPartSettings.HeaderHeight", model.HeaderHeight.ToString(CultureInfo.InvariantCulture));
            builder.WithSetting("PdfButtonPartSettings.FooterHeight", model.FooterHeight.ToString(CultureInfo.InvariantCulture));

            yield return DefinitionTemplate(model);
        }
    }
}