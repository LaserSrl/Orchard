using Laser.Orchard.MessageStore.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Core.Settings.Metadata;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Schema;
using System;
using System.Data;

namespace Laser.Orchard.MessageStore {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("MessageStorePartRecord", table => table
                .ContentPartRecord()
                .Column<string>("MessageText", column => column.Unlimited())
                .Column<int>("Gruppoid")
                .Column<string>("MessageFrom")
                .Column<string>("MessageTo")
                .Column<DateTime>("MessageDate")
                .Column<bool>("MarkRead")
                     );
            ContentDefinitionManager.AlterPartDefinition(typeof(MessageStorePart).Name,
              part => part.Attachable()
             .WithDescription("Add Message functionality")
                );

            ContentDefinitionManager.AlterTypeDefinition("MessageStore",
                t => t.WithPart(typeof(MessageStorePart).Name)
                    .WithPart("CommonPart"));

            return 1;
        }
        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("MessageStorePartRecord", table => table
       .AddColumn<string>("FilterTest")
       );
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("MessageStorePartRecord", table => table
       .AddColumn<bool>("MarkMessage")
       );
            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.AlterTable("MessageStorePartRecord", table => table
               .AddColumn<string>("MessageTextHtml", column => column.Unlimited())
               );
            SchemaBuilder.AlterTable("MessageStorePartRecord", table => table
                .AddColumn<string>("MessageObject")
                );
            return 4;
        }



    }
}