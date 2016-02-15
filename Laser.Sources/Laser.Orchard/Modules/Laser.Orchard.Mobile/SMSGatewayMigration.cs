using Orchard.Data.Migration;
using System;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Laser.Orchard.Mobile.Models;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile {

    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SMSGatewayMigration : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("SmsGatewayPartRecord", table => table
            .ContentPartRecord()
            .Column<string>("Message", col => col.Unlimited())
            .Column<bool>("HaveAlias", col => col.WithDefault(false))
            .Column<string>("Alias", col => col.WithLength(100))
            .Column<bool>("SmsMessageSent", col => col.WithDefault(false)));

            ContentDefinitionManager.AlterPartDefinition("SmsGatewayPart", config => config.Attachable());

            ContentDefinitionManager.AlterTypeDefinition("CommunicationAdvertising", config => config
                .WithPart("SmsGatewayPart"));

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("SendToTestNumber", System.Data.DbType.Boolean, col => col.WithDefault(false)));

            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("NumberForTest", System.Data.DbType.String, col => col.WithLength(50)));

            SchemaBuilder.AlterTable("SmsGatewayPartRecord", table => table
                .AddColumn("SendOnNextPublish", System.Data.DbType.Boolean, col => col.WithDefault(false)));

            return 2;
        }
    }
}