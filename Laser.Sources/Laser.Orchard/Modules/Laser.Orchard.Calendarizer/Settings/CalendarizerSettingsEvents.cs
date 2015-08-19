using Laser.Orchard.Calendarizer.Models;
using Laser.Orchard.Calendarizer.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Calendarizer.Settings {
    public class CalendarizerSettingsEvents : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "SchedulerPart")
                yield break;
            var settings = definition.Settings.GetModel<CalendarizerSettings>();

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "SchedulerPart")
                yield break;

            var settings = new CalendarizerSettings {
                //     Patterns = new List<SchedulerPart>()

            };


            if (updateModel.TryUpdateModel(settings, "CalendarizerSettings", null, null)) {
                // update the settings builder
                settings.Build(builder);
            }
            yield return DefinitionTemplate(settings);
        }
    }
}