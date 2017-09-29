using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KrakeDefaultTheme.Settings {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("ThemeSettingsPart", part => part
                    .WithField("Logo", fieldBuilder => fieldBuilder
                    .WithDisplayName("Logo")
                    .OfType("MediaLibraryPickerField")
                    .WithSetting("MediaLibraryPickerFieldSettings.Required", "False")
                    .WithSetting(" MediaLibraryPickerFieldSettings.Multiple", "False"))
            );
            return 1;
        }
    }
}