using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using System.Data;

namespace Laser.Orchard.Caligoo {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("CaligooUserPartRecord", t => t
                .ContentPartRecord()
                .Column<string>("CaligooUserId", c => c.NotNull())
            );
            ContentDefinitionManager.AlterTypeDefinition("CommunicationContact", t => t
                .WithPart("CaligooUserPart")
            );
            return 2;
        }
        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("CaligooUserPartRecord", t => t
                .AlterColumn("CaligooUserId", c => c.WithType(DbType.String))
            );
            return 2;
        }
    }
}