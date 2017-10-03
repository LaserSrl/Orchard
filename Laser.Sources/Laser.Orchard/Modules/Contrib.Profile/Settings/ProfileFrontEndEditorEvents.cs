using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Contrib.Profile.Settings {
    public class ProfileFrontEndEditorEvents : ContentDefinitionEditorEventsBase {
        //The settings are attached to all parts and fields
        //They are attached only if there is a ProfilePart
        private readonly IContentDefinitionManager _contentDefinitionManager;

        //This event hendler is instanced once per transaction, so I can use a bool to save checks
        private bool _typeHasProfilePart { get; set; }

        public ProfileFrontEndEditorEvents(
            IContentDefinitionManager contentDefinitionManager) {

            _contentDefinitionManager = contentDefinitionManager;
        }

        #region Fields
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (_typeHasProfilePart) {
                var settings = definition.Settings.GetModel<ProfileFrontEndSettings>();
                yield return DefinitionTemplate(settings);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            //var typeDefinition = _contentDefinitionManager.GetTypeDefinition(builder.PartName);
            if (_typeHasProfilePart) {
                //_typeHasProfilePart = true;
                var settings = new ProfileFrontEndSettings();
                if (updateModel.TryUpdateModel(settings, "ProfileFrontEndSettings", null, null)) {
                    builder.WithSetting("ProfileFrontEndSettings.AllowFrontEndEdit", settings.AllowFrontEndEdit.ToString(CultureInfo.InvariantCulture));
                    builder.WithSetting("ProfileFrontEndSettings.AllowFrontEndDisplay", settings.AllowFrontEndDisplay.ToString(CultureInfo.InvariantCulture));
                }
                yield return DefinitionTemplate(settings);
            }
        }
        #endregion

        #region Parts
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (_typeHasProfilePart ||
                definition.ContentTypeDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart")) {
                _typeHasProfilePart = true;
                var settings = definition.Settings.GetModel<ProfileFrontEndSettings>();
                yield return DefinitionTemplate(settings);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(builder.TypeName);
            if (_typeHasProfilePart ||
                typeDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart")) {
                _typeHasProfilePart = true;
                var settings = new ProfileFrontEndSettings();
                if (updateModel.TryUpdateModel(settings, "ProfileFrontEndSettings", null, null)) {
                    builder.WithSetting("ProfileFrontEndSettings.AllowFrontEndEdit", settings.AllowFrontEndEdit.ToString(CultureInfo.InvariantCulture));
                    builder.WithSetting("ProfileFrontEndSettings.AllowFrontEndDisplay", settings.AllowFrontEndDisplay.ToString(CultureInfo.InvariantCulture));
                }
                yield return DefinitionTemplate(settings);
            }
        }
        #endregion
    }
}