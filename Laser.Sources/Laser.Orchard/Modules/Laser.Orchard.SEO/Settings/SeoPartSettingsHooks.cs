using Laser.Orchard.SEO.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.SEO.Settings {
    public class SeoPartSettingsHooks : ContentDefinitionEditorEventsBase {

        public override IEnumerable<TemplateViewModel> TypePartEditor(
            ContentTypePartDefinition definition) {

            if (definition.PartDefinition.Name != "SeoPart") yield break;

            var model = definition.Settings.GetModel<SeoPartSettings>();

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name == "SeoPart") {
                var model = new SeoPartSettings();
                updateModel.TryUpdateModel(model, "SeoPartSettings", null, null);
                builder.WithSetting("SeoPartSettings.RobotsNoIndex", model.RobotsNoIndex.ToString());
                builder.WithSetting("SeoPartSettings.RobotsNoFollow", model.RobotsNoFollow.ToString());
                builder.WithSetting("SeoPartSettings.RobotsNoSnippet", model.RobotsNoSnippet.ToString());
                builder.WithSetting("SeoPartSettings.RobotsNoOdp", model.RobotsNoOdp.ToString());
                builder.WithSetting("SeoPartSettings.RobotsNoArchive", model.RobotsNoArchive.ToString());
                builder.WithSetting("SeoPartSettings.RobotsUnavailableAfter", model.RobotsUnavailableAfter.ToString());
                builder.WithSetting("SeoPartSettings.RobotsNoImageIndex", model.RobotsNoImageIndex.ToString());
                builder.WithSetting("SeoPartSettings.GoogleNoSiteLinkSearchBox", model.GoogleNoSiteLinkSearchBox.ToString());
                builder.WithSetting("SeoPartSettings.GoogleNoTranslate", model.GoogleNoTranslate.ToString());

                yield return DefinitionTemplate(model);
            }

            yield break;
        }
    }
}