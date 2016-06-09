using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Laser.Orchard.UserReactions.Models;

namespace Laser.Orchard.UserReactions.Settings {
    public class UserReactionsPartSettingsHooks : ContentDefinitionEditorEventsBase 
    {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) 
        {

            if (definition.PartDefinition.Name != "UserReactionsPartSettings") yield break;
            var model = definition.Settings.GetModel<UserReactionsPartSettings>();

            yield return DefinitionTemplate(model);
        }



        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder,
                                                                            IUpdateModel updateModel) 
        {

            if (builder.Name != "UserReactionsPartSettings") yield break;

            var model = new UserReactionsPartSettings();

            updateModel.TryUpdateModel(model, "UserReactionsPartSettings", null, null);

            builder.WithSetting("UserReactionsPartSettings.Required", ((bool)model.Filtering).ToString());

            yield return DefinitionTemplate(model);
        }


    }
}