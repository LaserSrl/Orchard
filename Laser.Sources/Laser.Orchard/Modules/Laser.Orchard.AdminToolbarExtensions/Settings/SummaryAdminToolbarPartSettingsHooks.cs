using Laser.Orchard.AdminToolbarExtensions.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.AdminToolbarExtensions.Settings {
    [OrchardFeature("Laser.Orchard.AdminToolbarExtensions.SummaryAdminToolbar")]
    public class SummaryAdminToolbarPartSettingsHooks : ContentDefinitionEditorEventsBase  {

        public override IEnumerable<TemplateViewModel> TypePartEditor(
            ContentTypePartDefinition definition){
                
            if (definition.PartDefinition.Name != "SummaryAdminToolbarPart") yield break;
            var model = definition.Settings.GetModel<SummaryAdminToolbarSettings>();

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(
            ContentTypePartDefinitionBuilder builder,
            IUpdateModel updateModel) {

            if (builder.Name != "SummaryAdminToolbarPart") yield break;

            var model = new SummaryAdminToolbarSettings();
            updateModel.TryUpdateModel(model, "SummaryAdminToolbarSettings", null, null);
            //put actual values in the MapPartSettings

            yield return DefinitionTemplate(model);
        }
    }
}