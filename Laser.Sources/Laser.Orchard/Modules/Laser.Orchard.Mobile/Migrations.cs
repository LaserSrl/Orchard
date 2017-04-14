using Laser.Orchard.Mobile.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;
using Orchard.Environment.Configuration;
using System;

namespace Laser.Orchard.Mobile {

    public class Migrations : DataMigrationImpl {
        private readonly IUtilsServices _utilsServices;
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private readonly IRepository<PushNotificationRecord> _repositoryDevice;

        public Migrations(IUtilsServices utilsServices, IOrchardServices orchardServices, ShellSettings shellSettings, IRepository<PushNotificationRecord> repositoryDevice) {
            _utilsServices = utilsServices;
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;
            _repositoryDevice = repositoryDevice;
        }

        public int Create() {
            SchemaBuilder.CreateTable("PushNotificationRecord",
                         table => table
                         .Column<int>("Id", column => column.PrimaryKey().Identity())
                         .Column<string>("Device", column => column.WithLength(250))
                         .Column<string>("UUIdentifier", column => column.WithLength(400))
                         .Column<string>("Token", column => column.WithLength(400))
                         .Column<bool>("Validated", col => col.WithDefault(true))
                         .Column<DateTime>("DataInserimento", column => column.Unlimited())
                         .Column<DateTime>("DataModifica", column => column.Unlimited())
                         .Column<bool>("Produzione", col => col.WithDefault(false))
                         .Column<string>("Language", column => column.WithLength(10))
                         );
            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("PushMobileSettingsPartRecord", table => table
                        .ContentPartRecord()
                        .Column<string>("DefaultParserIdSelected")
                        .Column<string>("ApplePathCertificateFile")
                        .Column<string>("AppleCertificatePassword")
                        .Column<string>("AndroidApiKey")
                        .Column<string>("WindowsEndPoint")
                        .Column<string>("WindowsAppPackageName")
                        .Column<string>("WindowsAppSecurityIdentifier")
            );
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("PushMobileSettingsPartRecord", table => table
                        .AddColumn<string>("ApplePathCertificateFileDevelopment")
                        );
            SchemaBuilder.AlterTable("PushMobileSettingsPartRecord", table => table
                        .AddColumn<string>("AppleCertificatePasswordDevelopment")
            );
            return 3;
        }

        public int UpdateFrom3() {
            SchemaBuilder.CreateTable("UserAgentRedirectPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("AutoRedirect", col => col.WithDefault(false))
                .Column<string>("AppName", col => col.WithLength(50)));
            SchemaBuilder.CreateTable("AppStoreRedirectRecord", table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<int>("UserAgentRedirectPartRecord_Id")
                .Column<string>("AppStoreKey", col => col.WithLength(25))
                .Column<string>("RedirectUrl", col => col.WithLength(255)));

            ContentDefinitionManager.AlterPartDefinition(typeof(UserAgentRedirectPart).Name, cfg => cfg
                .Attachable());

            ContentDefinitionManager.AlterTypeDefinition("UserAgentRedirectWidget", cfg => cfg
                .WithPart(typeof(UserAgentRedirectPart).Name)
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget"));

            return 4;
        }

        public int UpdateFrom4() {
            SchemaBuilder.CreateTable("MobilePushPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("TitlePush", col => col.WithLength(255))
                .Column<string>("TextPush", col => col.WithLength(255))
                .Column<bool>("ToPush", col => col.WithDefault(false))
                .Column<bool>("TestPush", col => col.WithDefault(false))
                .Column<string>("DevicePush", col => col.WithLength(50))
                );

            ContentDefinitionManager.AlterPartDefinition(typeof(MobilePushPart).Name, cfg => cfg
                .Attachable());

            return 5;
        }

        public int UpdateFrom5() {
            ContentDefinitionManager.AlterPartDefinition(typeof(MobilePushPart).Name, cfg => cfg
                 .WithField("RelatedContent", x => x.OfType("ContentPickerField"))
                      );
            return 6;
        }

        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("PushMobileSettingsPartRecord", table => table
                .AddColumn<bool>("ShowTestOptions", col => col.WithDefault(true))
                );
            return 7;
        }

        public int UpdateFrom7() {
            SchemaBuilder.AlterTable("PushMobileSettingsPartRecord", table => table
                .AddColumn<string>("ApplePushSound", col => col.WithLength(50))
                );
            return 8;
        }

        public int UpdateFrom8() {
            SchemaBuilder.AlterTable("PushMobileSettingsPartRecord", table => table
                        .AddColumn<string>("AndroidApiKeyDevelopment")
                        );

            return 9;
        }

        public int UpdateFrom9() {
            SchemaBuilder.CreateTable("UserPushCategoryRecord",
               table => table
              .Column<int>("Id", column => column.PrimaryKey().Identity())
           .Column<int>("UserPartRecord_Id")
            .Column<string>("Category")
           );
            //  SchemaBuilder.CreateForeignKey("UserPushCategoryRecord_UserPartRecord", "UserPushCategoryRecord", new string[] { "UserPartRecord_Id" }, "UserPartRecord", new string[] { "Id" });

            return 10;
        }

        public int UpdateFrom10() {
            SchemaBuilder.DropTable("UserPushCategoryRecord");
            SchemaBuilder.CreateTable("UserDeviceRecord",
               table => table
              .Column<int>("Id", column => column.PrimaryKey().Identity())
              .Column<int>("UserPartRecord_Id")
                .Column<string>("UUIdentifier", column => column.WithLength(400))
           );
            //  SchemaBuilder.CreateForeignKey("UserPushCategoryRecord_UserPartRecord", "UserPushCategoryRecord", new string[] { "UserPartRecord_Id" }, "UserPartRecord", new string[] { "Id" });
            return 11;
        }

        public int UpdateFrom11() {
            SchemaBuilder.AlterTable("PushMobileSettingsPartRecord", table => table
                     .AddColumn<string>("TaxonomyName")
                     );
            return 12;
        }

        public int UpdateFrom12() {

            ContentDefinitionManager.AlterTypeDefinition(
            "CommunicationAdvertising",
            type => type
                .WithPart("MobilePushPart")
                );
            SchemaBuilder.AlterTable("PushNotificationRecord",
                              table => table
                              .AddColumn<int>("MobileContactPartRecord_Id")
                 );
            return 13;
        }
        public int UpdateFrom13() {
            _utilsServices.EnableFeature("Laser.Orchard.ShortLinks");
            _utilsServices.EnableFeature("Laser.Orchard.Queries");
            _utilsServices.EnableFeature("Laser.Orchard.CommunicationGateway");
            return 14;
        }
        public int UpdateFrom14() {
            SchemaBuilder.CreateTable("MobileContactPartRecord",
                             table => table
                                 .ContentPartRecord()
                );
            return 15;
        }
         public int UpdateFrom15() {
            return 16;
        }
         public int UpdateFrom16() {
             return 17;
         }
        public int UpdateFrom17() {
             ContentDefinitionManager.AlterTypeDefinition(
           "CommunicationAdvertising",
           type => type
               .RemovePart("MobilePushPart")
            );
            ContentDefinitionManager.AlterTypeDefinition(
           "CommunicationAdvertising",
           type => type
               .WithPart("MobilePushPart",y=>y
                   .WithSetting("PushMobilePartSettingVM.HideRelated", "true")
                   .WithSetting("PushMobilePartSettingVM.AcceptZeroRelated", "true")
               )
               );
            return 18;
        }
        public int UpdateFrom18()
        {
            SchemaBuilder.AlterTable("MobilePushPartRecord", table => table
                .AddColumn<bool>("PushSent", col => col.WithDefault(false)));
            SchemaBuilder.AlterTable("MobilePushPartRecord", table => table
                .AddColumn<int>("TargetDeviceNumber", col => col.WithDefault(0)));
            SchemaBuilder.AlterTable("MobilePushPartRecord", table => table
                .AddColumn<int>("PushSentNumber", col => col.WithDefault(0)));
            return 19;
        }
        public int UpdateFrom19() {
            SchemaBuilder.CreateTable("SentRecord",
                      table => table
                      .Column<int>("Id", column => column.PrimaryKey().Identity())
                      .Column<int>("PushNotificationRecord_Id")
                      .Column<int>("PushedItem")
                      .Column<DateTime>("SentDate")
                      .Column<string>("DeviceType")
                      );
            return 20;
        }
        public int UpdateFrom20() {
            SchemaBuilder.AlterTable("PushMobileSettingsPartRecord", table => table
                        .AddColumn<string>("AndroidPushServiceUrl")
                        );

            return 21;
        }
        public int UpdateFrom21() {
            SchemaBuilder.AlterTable("PushNotificationRecord",
                              table => table
                              .AddColumn<string>("RegistrationUrlHost"));
            SchemaBuilder.AlterTable("PushNotificationRecord",
                              table => table
                              .AddColumn<string>("RegistrationUrlPrefix"));

            return 22;
        }
        public int UpdateFrom22() {
            SchemaBuilder.AlterTable("PushNotificationRecord",
                              table => table
                              .AddColumn<string>("RegistrationMachineName"));

            return 23;
        }
        public int UpdateFrom23() {
            return 24;
        }
        public int UpdateFrom24() {
            // se è attiva la feature Laser.Orchard.Mobile, attiva anche Laser.Orchard.PushGateway
            if (_utilsServices.FeatureIsEnabled("Laser.Orchard.Mobile")) {
                _utilsServices.EnableFeature("Laser.Orchard.PushGateway");
            }

            // se è attiva la feature Laser.Orchard.CommunicationGateway attiva anche Laser.Orchard.Mobile
            if (_utilsServices.FeatureIsEnabled("Laser.Orchard.CommunicationGateway")) {
                _utilsServices.EnableFeature("Laser.Orchard.Mobile");
            }
            return 25;
        }
        public int UpdateFrom25() {
            // se è attiva la feature Laser.Orchard.Queues, attiva anche Laser.Orchard.PushGateway
            if (_utilsServices.FeatureIsEnabled("Laser.Orchard.Queues")) {
                _utilsServices.EnableFeature("Laser.Orchard.PushGateway");
            }
            return 26;
        }
        public int UpdateFrom26() {
            SchemaBuilder.AlterTable("PushMobileSettingsPartRecord", table => table
                        .AddColumn<string>("AndroidPushNotificationIcon")
                        );
            return 27;
        }
        public int UpdateFrom27() {
            SchemaBuilder.AlterTable("MobilePushPartRecord", table => table
                        .AddColumn<string>("RecipientList", col=> col.Unlimited())
                        );
            SchemaBuilder.AlterTable("MobilePushPartRecord", table => table
                        .AddColumn<bool>("TestPushToDevice")
                        );
            SchemaBuilder.AlterTable("MobilePushPartRecord", table => table
                        .AddColumn<bool>("UseRecipientList")
                        );
            return 28;
        }
        public int UpdateFrom28() {
            SchemaBuilder.AlterTable("SentRecord", table => table
                .AddColumn<string>("Outcome")
            );
            return 29;
        }

    }
}