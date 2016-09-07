using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.PaymentGateway.Models;

namespace Laser.Orchard.PaymentGateway {
    public class PaymentMigrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("PaymentRecord",
                table => table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<string>("PosName")
                    .Column<string>("Reason")
                    .Column<DateTime>("CreationDate")
                    .Column<DateTime>("UpdateDate")
                    .Column<string>("PosUrl")
                    .Column<decimal>("Amount")
                    .Column<string>("Currency")
                    .Column<bool>("Success")
                    .Column<string>("Error")
                    .Column<string>("TransactionId")
                    .Column<string>("ReturnUrl")
                    .Column("Info", System.Data.DbType.String, x => x.Unlimited())
                    .Column<int>("ContentItemId")
                );
            return 1;
        }
        public int UpdateFrom1() {
            ContentDefinitionManager.AlterPartDefinition(
                typeof(PayButtonPart).Name,
                p => p.Attachable()
            );
            return 2;
        }
    }
}
