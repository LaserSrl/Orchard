using Orchard.Data.Migration;
using Orchard.Core.Contents.Extensions;
using Orchard.ContentManagement.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("RequiredClaimsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column("Claims", System.Data.DbType.String, c => c.Unlimited()));
            ContentDefinitionManager.AlterPartDefinition(
                "RequiredClaimsPart",
                 b => b
                    .Attachable(true)
                    .WithField("Claims", cfg => cfg.OfType("TextField"))
            );
            return 1;
        }
        public int UpdateFrom1() {
            ContentDefinitionManager.DeletePartDefinition("RequiredClaimsPart");
            ContentDefinitionManager.AlterPartDefinition(
                "RequiredClaimsPart",
                 b => b
                    .Attachable(true)
            );
            return 2;
        }
        public int UpdateFrom2() {
            SchemaBuilder.DropTable("RequiredClaimsPartRecord");
            SchemaBuilder.CreateTable("RequiredClaimsPartRecord",
                table => table
                    .ContentPartVersionRecord()
                    .Column("Claims", System.Data.DbType.String, c => c.Unlimited()));
            return 3;
        }
        public int UpdateFrom3() {
            SchemaBuilder.CreateTable("IdentityClaimsPartRecord",
                t => t.ContentPartRecord());
            SchemaBuilder.CreateTable("IdentityClaimsRecord",
                t => t
                    .Column("Id", System.Data.DbType.Int32, c => c.PrimaryKey().Identity())
                    .Column("IdentityClaims", System.Data.DbType.String, c => c.Unlimited())
                    .Column("UserId", System.Data.DbType.Int32)
                );
            ContentDefinitionManager.AlterPartDefinition(
                "IdentityClaimsPart",
                 b => b
                    .Attachable(false)
            );
            ContentDefinitionManager.AlterTypeDefinition("User",
                t => t.WithPart("IdentityClaimsPart")
            );
            return 4;
        }
        public int UpdateFrom4() {
            SchemaBuilder.AlterTable("IdentityClaimsRecord",
                t => t
                .AddColumn("IdentityClaimsPartRecord_id", System.Data.DbType.Int32)
            );
            return 5;
        }
        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("IdentityClaimsRecord",
                t => t
                .DropColumn("UserId")
            );
            return 6;
        }
    }
}