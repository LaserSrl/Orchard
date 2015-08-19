using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using System;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Calendarizer {
  //  [OrchardFeature("Laser.Orchard.Calendarizer")]
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("SchedulerPartRecord", table => table

                // The following method will create an "Id" column for us and set it is the primary key for the table
                .ContentPartRecord()
                // Next fields
                .Column<bool>("AllDay")
                .Column<DateTime>("FromDate")
                .Column<DateTime>("ToDate")
               
                );

            // Create (or alter) a part called "SchedulerPart" and configure it to be "attachable".
            ContentDefinitionManager.AlterPartDefinition("SchedulerPart", part => part
                //.WithField("FromDate", fieldBuilder => fieldBuilder.OfType("DateTimeField")
                //    .WithDisplayName("From Date")
                //    .WithSetting("Required", "1")
                //    .WithSetting("Hint", "Schedule starting at"))
                //.WithField("ToDate", fieldBuilder => fieldBuilder.OfType("DateTimeField")
                //    .WithDisplayName("To Date")
                //    .WithSetting("Required", "1")
                //    .WithSetting("Hint", "Schedule ending at"))
                .Attachable());
            return 1;
        }
    }
}