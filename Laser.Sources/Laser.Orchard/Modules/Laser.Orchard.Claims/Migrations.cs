using Orchard.Data.Migration;
using Orchard.Core.Contents.Extensions;
using Orchard.ContentManagement.MetaData;

namespace Laser.Orchard.Claims {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("ItemClaimsPartRecord",
                table => table
                    .ContentPartVersionRecord()
                    .Column("Claims", System.Data.DbType.String, c => c.Unlimited()));
            ContentDefinitionManager.AlterPartDefinition(
                "ItemClaimsPart",
                 b => b
                    .Attachable(true)
            );
            SchemaBuilder.CreateTable("IdentityClaimsPartRecord",
                t => t.ContentPartRecord());
            SchemaBuilder.CreateTable("IdentityClaimsRecord",
                t => t
                    .Column("Id", System.Data.DbType.Int32, c => c.PrimaryKey().Identity())
                    .Column("IdentityClaims", System.Data.DbType.String, c => c.Unlimited())
                    .Column("IdentityClaimsPartRecord_id", System.Data.DbType.Int32)
                );
            ContentDefinitionManager.AlterPartDefinition(
                "IdentityClaimsPart",
                 b => b
                    .Attachable(false)
            );
            ContentDefinitionManager.AlterTypeDefinition("User",
                t => t.WithPart("IdentityClaimsPart")
            );
            return 1;
        }
    }
}