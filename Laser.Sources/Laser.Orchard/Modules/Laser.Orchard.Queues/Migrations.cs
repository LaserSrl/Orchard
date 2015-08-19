using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using System;

namespace Laser.Orchard.Queues
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable("QueueRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("QueueName")
                .Column<int>("TicketGap")
                .Column<int>("MaxTicketNumber")
                );

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateTable("QueueUserPartRecord",
                table => table
                    .ContentPartRecord()
                );

            SchemaBuilder.CreateTable("QueueUserRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("QueueNumber")
                .Column<int>("NumNotifications")
                .Column<DateTime>("RegistrationDate")
                .Column<int>("QueueRecord_Id")
                .Column<int>("QueueUserPartRecord_Id")
                );

            ContentDefinitionManager.AlterTypeDefinition(
                "User",
                type => type
                            .WithPart("QueueUserPart")
            );

            return 2;
        }
    }
}