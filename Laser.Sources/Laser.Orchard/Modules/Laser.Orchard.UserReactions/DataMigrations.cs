using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions {
    public class DataMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("UserReactionsTypesRecord", table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<string>("TypeName", col => col.WithLength(50))
                .Column<string>("TypeCssClass", col => col.WithLength(50))
                .Column<int>("Priority")
                );

            return 1;
        }
    }
}