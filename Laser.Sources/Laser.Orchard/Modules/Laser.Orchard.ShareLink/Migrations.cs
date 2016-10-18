using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.ShareLink {

    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("ShareLinkPartRecord", table => table
               .ContentPartRecord()
               .Column<string>("SharedLink")
               .Column<string>("SharedText")
                .Column<string>("SharedImage")
                );
            return 1;
        }

        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition(
             "ShareLinkPart",
             b => b
             .Attachable(true)
             );
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.CreateTable("ShareLinkModuleSettingPartRecord", table => table
             .ContentPartRecord()
             .Column<string>("SharedLink")
             .Column<string>("SharedText")
              .Column<string>("SharedImage")
              );
            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("ShareLinkPartRecord", table => table
            .AddColumn<string>("SharedIdImage")
            );
            return 4;
        }
    }
}