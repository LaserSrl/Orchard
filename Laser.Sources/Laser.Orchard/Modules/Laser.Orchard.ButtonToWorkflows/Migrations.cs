using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.ButtonToWorkflows {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition("ButtonToWorkflowsPart", part => part.Attachable());
            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("ButtonToWorkflowsSettingsPartRecord", table => table
                        .ContentPartRecord()
                        .Column<string>("ButtonsText")
                        .Column<string>("ButtonsAction")
         );
            return 2;
        }
        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("ButtonToWorkflowsSettingsPartRecord", table => table
                        .AddColumn<string>("ButtonsDescription")
                      
         );
            return 3;
        }
        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("ButtonToWorkflowsSettingsPartRecord", table => table
                        .AddColumn<string>("ButtonsMessage")

         );
            return 4;

            
        }
        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("ButtonToWorkflowsSettingsPartRecord", table => table
                        .AddColumn<string>("ButtonsAsync")
         );
            return 5;
        }
        //        public int UpdateFrom4() {
        //            SchemaBuilder.AlterTable("ButtonToWorkflowsPart", table => table
        //                        .AddColumn<string>("ActionToExecute")
        //         );
        //            SchemaBuilder.AlterTable("ButtonToWorkflowsPart", table => table
        //             .AddColumn<string>("MessageToWrite")
        //);
        //            return 5;
        //        }
    }
}