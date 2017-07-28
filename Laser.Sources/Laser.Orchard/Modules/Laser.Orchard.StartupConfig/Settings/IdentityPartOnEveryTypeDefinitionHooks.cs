using Orchard.ContentManagement.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

namespace Laser.Orchard.StartupConfig.Settings {
    public class IdentityPartOnEveryTypeDefinitionHooks : ContentDefinitionEditorEventsBase {
        //We want all our ContentTypes top have an identity, to allwo proper cloning/importing/exporting
        //Some parts contribute to an item's identity, so if those are in the type we don't need to add an identity part
        private readonly string[] PartsWIthIdentity = { "IdentityPart", "AutoroutePart", "UserPart" };

        private readonly IContentDefinitionManager _contentDefinitionManager;

        public IdentityPartOnEveryTypeDefinitionHooks(
            IContentDefinitionManager contentDefinitionManager) {

            _contentDefinitionManager = contentDefinitionManager;
        }

        
        public override IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition) {
            if (!definition.Parts.Any(pa =>
                    PartsWIthIdentity.Contains(pa.PartDefinition.Name))) {
                _contentDefinitionManager
                    .AlterTypeDefinition(definition.Name, builder => {
                        builder.WithPart("IdentityPart");
                    });
            }
            yield return null;
        }
    }
}