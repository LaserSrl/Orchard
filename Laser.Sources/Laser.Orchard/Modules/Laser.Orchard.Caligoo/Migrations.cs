using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Layouts.Helpers;

namespace Laser.Orchard.Caligoo {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("CaligooUserPartRecord", t => t
                .ContentPartRecord()
                .Column<string>("CaligooUserId", c => c.NotNull())
            );
            ContentDefinitionManager.AlterPartDefinition("CaligooUserPart", p => p
                .Placeable(true)
            );
            ContentDefinitionManager.AlterTypeDefinition("CommunicationContact", t => t
                .WithPart("CaligooUserPart")
            );
            SchemaBuilder.CreateTable("CaligooLocationPartRecord", t => t
                .ContentPartRecord()
                .Column<string>("CaligooLocationId", c => c.Nullable())
                .Column<string>("DisplayName", c => c.Nullable())
                .Column<string>("Address", c => c.Nullable())
                .Column<string>("PostalCode", c => c.Nullable())
                .Column<string>("City", c => c.Nullable())
                .Column<string>("Country", c => c.Nullable())
                .Column<decimal>("Latitude", c => c.Nullable().WithPrecision(12).WithScale(9))
                .Column<decimal>("Longitude", c => c.Nullable().WithPrecision(12).WithScale(9))
            );
            ContentDefinitionManager.AlterPartDefinition("CaligooLocationPart", p => p
                .Attachable(true)
                .Placeable(true)
            );
            return 1;
        }
    }
}