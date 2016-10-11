using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Laser.Orchard.FidelityGateway.Models;

namespace Laser.Orchard.FidelityGateway
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable("FidelityUserPartRecord", table => table
              .ContentPartRecord()
              .Column<string>("FidelityUsername")
              .Column<string>("FidelityPassword")
              .Column<string>("CustomerId")
          );

            ContentDefinitionManager.AlterPartDefinition(typeof(FidelityUserPart).Name, cfg => cfg
                .Attachable());

            ContentDefinitionManager.AlterTypeDefinition(
                "User",
                type => type
                            .WithPart("FidelityUserPart")
            );

            return 1;
        }

        public int UpdateFrom1()
        {
            ContentDefinitionManager.AlterPartDefinition(typeof(FidelityUserPart).Name, cfg => cfg
                .WithDescription("Fidelity information about the user")
                .Attachable());

            return 2;
        }
    }
}