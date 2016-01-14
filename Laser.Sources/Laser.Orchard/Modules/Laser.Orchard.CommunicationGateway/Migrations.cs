using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using System;

namespace Laser.Orchard.CommunicationGateway {

    public class CoomunicationMigrations : DataMigrationImpl {
        //  private readonly IUtilsServices _utilServices;

        //public Migrations(IUtilsServices utilsServices)
        //{
        //    _utilServices = utilsServices;
        //}

        /// <summary>
        /// This executes whenever this module is activated.
        /// </summary>
        public int Create() {
            ContentDefinitionManager.AlterPartDefinition(
              "QueryFilterPart",
              b => b
              .Attachable(false)
              );

            ContentDefinitionManager.AlterPartDefinition(
                "CommunicationCampaignPart",
                 b => b
                    .Attachable(false)
                    .WithField("FromDate", cfg => cfg.OfType("DateTimeField").WithDisplayName("From Date"))
                    .WithField("ToDate", cfg => cfg.OfType("DateTimeField").WithDisplayName("To Date"))
            );
            ContentDefinitionManager.AlterTypeDefinition(
              "CommunicationCampaign",
              type => type
                  .WithPart("TitlePart")
                  //.WithPart("AutoroutePart", p => p
                  //  .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                  //  .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                  //  .WithSetting("AutorouteSettings.PatternDefinitions", @"[{Name:'Title', Pattern:'{Content.Slug}',Description:'Title of campaign'}]")
                  //  .WithSetting("AutorouteSettings.DefaultPatternIndex", "0")
                  //     )
                .WithPart("IdentityPart")
                .WithPart("CommunicationCampaignPart")
                  // .WithPart("LocalizationPart")
                .WithPart("CommonPart")
                .Creatable(false)
                .DisplayedAs("Campaign")
              );
            ContentDefinitionManager.AlterPartDefinition(
   "CommunicationAdvertisingPart",
    b => b
       .Attachable(false)
       .WithField("ContentLinked", cfg => cfg
           .OfType("ContentPickerField")
               .WithSetting("ContentPickerFieldSettings.Hint", "Select a ContentItem.")
               .WithSetting("ContentPickerFieldSettings.Required", "False")
               .WithSetting("ContentPickerFieldSettings.Multiple", "False")
               .WithSetting("ContentPickerFieldSettings.ShowContentTab", "True")
               .WithSetting("ContentPickerFieldSettings.ShowSearchTab", "True")
               .WithSetting("ContentPickerFieldSettings.DisplayedContentTypes", "")
               .WithDisplayName("Content")
               .WithSetting("ContentPartSettings.Attachable", "True")
           )
       .WithField("Gallery", cfg => cfg
           .OfType("MediaLibraryPickerField")
            .WithDisplayName("Gallery")
            .WithSetting("MediaLibraryPickerFieldSettings.Required", "false")
            .WithSetting("MediaLibraryPickerFieldSettings.Multiple", "false")
            .WithSetting("MediaLibraryPickerFieldSettings.DisplayedContentTypes", "Image")
            .WithSetting("MediaLibraryPickerFieldSettings.AllowedExtensions", "jpg jpeg png gif")
            .WithSetting("MediaLibraryPickerFieldSettings.Hint", "Insert Image")
           )
        );
            ContentDefinitionManager.AlterTypeDefinition(
            "CommunicationAdvertising",
            type => type
               .WithPart("TitlePart")
                //      .WithPart("BodyPart")
                .WithPart("AutoroutePart", p => p
                   .WithSetting("AutorouteSettings.AllowCustomPattern", "false")
                   .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                   .WithSetting("AutorouteSettings.PatternDefinitions", @"[{Name:'Title', Pattern:'{Content.Slug}',Description:'Title of Advertising'}]")
                   .WithSetting("AutorouteSettings.DefaultPatternIndex", "0")
                      )
              .WithPart("IdentityPart")
              .WithPart("CommunicationAdvertisingPart")
              .WithPart("LocalizationPart")
              .WithPart("CommonPart")
              .WithPart("PublishLaterPart")
              .WithPart("QueryFilterPart")
                //  .WithPart("FacebookPostPart")
                //  .WithPart("TwitterPostPart")
              .Creatable(false)
               .Draftable(true)
              .DisplayedAs("Advertising")

            );
            return 1;
        }

        public int UpdateFrom1() {
            return 2;
        }

        public int UpdateFrom2() {
            ContentDefinitionManager.AlterTypeDefinition(
            "CommunicationAdvertising",
            type => type
                .WithPart("TagsPart")
                );
            return 3;
        }

        public int UpdateFrom3() {
            ContentDefinitionManager.AlterPartDefinition(
                "CommunicationAdvertisingPart",
                b => b
                .WithField("UrlLinked", cfg => cfg
                    .WithSetting("LinkFieldSettings.LinkTextMode", "Static")
                .OfType("LinkField"))
                );
            return 4;
        }

        public int UpdateFrom4() {
            ContentDefinitionManager.AlterPartDefinition(
              "CommunicationContactPart",
               b => b
               .Attachable(false)
            );
            SchemaBuilder.CreateTable("CommunicationContactPartRecord",
                table => table
                    .ContentPartRecord()
            );

            ContentDefinitionManager.AlterTypeDefinition(
                   "CommunicationContact", type => type
                       .WithPart("TitlePart")
                       .WithPart("CommonPart")
                       .WithPart("IdentityPart")
                       .WithPart("CommunicationContactPart")
                       .WithPart("ProfilePart")
                       .Creatable(false)
                       .Draftable(false)
                   );
            return 5;
        }

        public int UpdateFrom5() {
            SchemaBuilder.AlterTable("CommunicationContactPartRecord",
               table => table
               .AddColumn<int>("UserPartRecord_Id")
             );
            return 6;
        }
        public int UpdateFrom6() {
            SchemaBuilder.AlterTable("CommunicationContactPartRecord",
               table => table
               .AddColumn<bool>("Master", col => col.WithDefault(false))
             );
            return 7;
        }
        public int UpdateFrom7() {
            SchemaBuilder.CreateTable("CommunicationEmailRecord",
            table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("EmailContactPartRecord_Id", column => column.WithDefault(0))
                .Column<string>("Language", column => column.WithLength(10))
                .Column<bool>("Validated", col => col.WithDefault(true))
                .Column<DateTime>("DataInserimento", c => c.NotNull())
                .Column<DateTime>("DataModifica", c => c.NotNull())
                .Column<bool>("Produzione", col => col.WithDefault(false))
                .Column<string>("Email", column => column.WithLength(400))
             );
            return 8;
        }
        public int UpdateFrom8() {
            SchemaBuilder.CreateTable("CommunicationSmsRecord",
            table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("SmsContactPartRecord_Id", column => column.WithDefault(0))
                .Column<string>("Language", column => column.WithLength(10))
                .Column<bool>("Validated", col => col.WithDefault(true))
                .Column<DateTime>("DataInserimento", c => c.NotNull())
                .Column<DateTime>("DataModifica", c => c.NotNull())
                .Column<bool>("Produzione", col => col.WithDefault(false))
                .Column<string>("Sms", column => column.WithLength(400))
                 .Column<string>("Prefix", column => column.WithLength(400))
             );
            return 9;
        }
        public int UpdateFrom9() {
            ContentDefinitionManager.AlterPartDefinition("CommunicationAdvertisingPart",
                alt => alt.RemoveField("Campaign"));
            SchemaBuilder.CreateTable("CommunicationAdvertisingPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("CampaignId"));
            SchemaBuilder.AlterTable("CommunicationAdvertisingPartRecord",
                table => table.CreateIndex("IX_CampaignId", "CampaignId"));
            SchemaBuilder.AlterTable("CommunicationContactPartRecord",
               table => table
               .CreateIndex("IX_UserPartRecord_Id", "UserPartRecord_Id")
             );
            return 10;
        }

        public int UpdateFrom10() {
             SchemaBuilder.CreateTable("EmailContactPartRecord",
                table => table
                    .ContentPartRecord()
            );
                return 11;
        }
        public int UpdateFrom11() {
            SchemaBuilder.CreateTable("SmsContactPartRecord",
               table => table
                   .ContentPartRecord()
           );
            return 12;
        }

        public int UpdateFrom12() {
            SchemaBuilder.AlterTable("CommunicationEmailRecord",
                table => table
                    .CreateIndex("IX_CommunicationEmailRecord_EmailContactPartRecord_Id", "EmailContactPartRecord_Id"));
            SchemaBuilder.AlterTable("CommunicationSmsRecord",
                table => table
                    .CreateIndex("IX_CommunicationEmailRecord_SmsContactPartRecord_Id", "SmsContactPartRecord_Id"));
            return 13;
        }
        public int UpdateFrom13() {
            ContentDefinitionManager.AlterTypeDefinition(
            "CommunicationAdvertising",
            type => type
                .RemovePart("TagsPart")
                );
            return 14;
        }
                public int UpdateFrom14() {
            ContentDefinitionManager.AlterTypeDefinition(
            "CommunicationAdvertising",
            type => type
                .RemovePart("QueryFilterPart")
                );
            return 15;
        }

    }
}