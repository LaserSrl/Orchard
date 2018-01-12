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
                .Column<string>("StoredPartNumbers", col => col.Unlimited())
                .Column<bool>("IssueCredentialsAutomatically"));
            
            // set up many to many relationship between PartNumberSets and users

            SchemaBuilder.CreateTable("PartNumberSetsUserPartRecord", table => table
                .ContentPartRecord());
            
            SchemaBuilder.CreateTable("PartNumberSetUserPartJunctionRecord", table => table
                .Column<int>("Id", col => col.Identity().PrimaryKey())
                .Column<int>("HIDPartNumberSet_Id")
                .Column<int>("PartNumberSetsUserPartRecord_Id"));

            SchemaBuilder.CreateForeignKey(
                "FK_PartNumberSetUserSet",
                "PartNumberSetUserPartJunctionRecord", // Table
                    new[] { "HIDPartNumberSet_Id" }, // Column
                "HIDPartNumberSet", // Table
                    new[] { "Id" }); // Column

            SchemaBuilder.CreateForeignKey(
                "FK_PartNumberSetUserUser",
                "PartNumberSetUserPartJunctionRecord", // Table
                    new[] { "PartNumberSetsUserPartRecord_Id" }, // Column
                "PartNumberSetsUserPartRecord", // Table
                    new[] { "Id" }); // Column

            return 1;
        }

        public int UpdateFrom1() {

            SchemaBuilder.CreateTable("BulkCredentialsOperationsRecord", table => table
                .Column<int>("Id", col=> col.Identity().PrimaryKey())
                .Column<int>("TaskId")
                .Column<int>("UserId")
                .Column<string>("SerializedRevokeList", col => col.Unlimited())
                .Column<string>("SerializedIssueList", col => col.Unlimited()));

            return 2;
        }

    }
}