using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.UserProfiler {
    public class Migrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("UserProfilingPartRecord",
                table => table
                    .ContentPartRecord()
                );
            SchemaBuilder.CreateTable("UserProfilingSummaryRecord",
             table => table
                 .Column<int>("Id", column => column.PrimaryKey().Identity())
                 .Column<int>("UserProfilingPartRecord_Id")
                 .Column<string>("Text")
                 .Column<string>("SourceType")
                 .Column<int>("Count")
             );
            ContentDefinitionManager.AlterPartDefinition("UserProfilingPart", builder => builder
                .Attachable(false)
                );
            ContentDefinitionManager.AlterTypeDefinition("User", builder => builder
                .WithPart("UserProfilingPart")
                );
            return 1;
        }
        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("TrackingPartRecord",
              table => table
                  .ContentPartRecord()
              );
     
            return 2;
        }
        public int UpdateFrom2() {

            ContentDefinitionManager.AlterPartDefinition("TrackingPart", builder => builder
               .Attachable()
               .WithDescription("Allows tracking User preference"));
            return 3;
        }
    }
}