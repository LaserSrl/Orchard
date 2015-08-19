using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Linq;

namespace Laser.Orchard.Sharing.Settings
{

    public class ShareBarTypePartSettings
    {
        public ShareBarMode Mode { get; set; }
        public IEnumerable<dynamic> AvailableModes { get; set; }
    }

    /// <summary>
    /// Overrides default editors to enable putting settings on Twitter Connect part.
    /// </summary>
    public class ShareBarSettingsHooks : ContentDefinitionEditorEventsBase
    {
        /// <summary>
        /// Overrides editor shown when part is attached to content type. Enables adding setting field to the content part
        /// attached.
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition)
        {
            if (definition.PartDefinition.Name != "ShareBarPart")
                yield break;
            var model = definition.Settings.GetModel<ShareBarTypePartSettings>();

            model.AvailableModes = Enum.GetValues(typeof (ShareBarMode))
                .Cast<int>()
                .Select(i => 
                    new {
                        Text = Enum.GetName(typeof (ShareBarMode), i), 
                        Value = i
                    });

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel)
        {
            if (builder.Name != "ShareBarPart")
                yield break;

            var model = new ShareBarTypePartSettings();
            updateModel.TryUpdateModel(model, "ShareBarTypePartSettings", null, null);
            builder.WithSetting("ShareBarTypePartSettings.Mode", 
                ((int)model.Mode).ToString());
            yield return DefinitionTemplate(model);
        }

    }
}
