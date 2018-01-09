using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Migrations {
    public class HIDMigration : DataMigrationImpl {

        public int Create() {

            SchemaBuilder.CreateTable("HIDPartNumberSet", table => table
                .Column<int>("Id", col => col.Identity().PrimaryKey())
                .Column<string>("Name", col => col.NotNull())
                .Column<string>("StoredPartNumbers", col => col.Unlimited()));

            return 1;
        }
    }
}