using Laser.Orchard.AppDirect.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Schema;
using System;
using Laser.Orchard.ButtonToWorkflows.Models;

namespace Laser.Orchard.AppDirect
{
  public class Migration : DataMigrationImpl
  {
    public Migration()
    {
     }

    public int Create()
    {
      return 1;
    }

    public int UpdateFrom1()
    {
      return 2;
    }

    public int UpdateFrom2()
    {
            SchemaBuilder.CreateTable(typeof (LogEventsRecord).Name, (Action<CreateTableCommand>) (table => table.Column<DateTime>("TimeStamp", (Action<CreateColumnCommand>) null).Column<string>("EventType", (Action<CreateColumnCommand>) null).Column<string>("Method", (Action<CreateColumnCommand>) null).Column<string>("Log", (Action<CreateColumnCommand>) (column => column.Unlimited()))));
      return 3;
    }

    public int UpdateFrom3()
    {
            SchemaBuilder.AlterTable(typeof (LogEventsRecord).Name, (Action<AlterTableCommand>) (table => table.AddColumn<int>("Id", (Action<AddColumnCommand>) (column => ((CreateColumnCommand) column).PrimaryKey().Identity()))));
      return 4;
    }

    public int UpdateFrom4()
    {
            SchemaBuilder.CreateTable(typeof (AppDirectUserPartRecord).Name, (Action<CreateTableCommand>) (table => table.ContentPartRecord().Column<string>("Email", (Action<CreateColumnCommand>) null).Column<string>("FirstName", (Action<CreateColumnCommand>) null).Column<string>("Language", (Action<CreateColumnCommand>) null).Column<string>("LastName", (Action<CreateColumnCommand>) null).Column<string>("Locale", (Action<CreateColumnCommand>) null)));
      ContentDefinitionManager.AlterPartDefinition( typeof (AppDirectUserPart).Name, (Action<ContentPartDefinitionBuilder>) (cfg => MetaDataExtensions.Attachable(cfg, false)));
      ContentDefinitionManager.AlterPartDefinition( "AppDirectRequestPart", (Action<ContentPartDefinitionBuilder>) (b => MetaDataExtensions.Attachable(b, false)
      .WithField("Request", (Action<ContentPartFieldDefinitionBuilder>) (cfg => cfg.OfType("TextField").WithDisplayName("Json Response")
      .WithSetting("TextFieldSettings.Flavor", "TextArea")))
      .WithField("Action", (Action<ContentPartFieldDefinitionBuilder>) (x => x.OfType("TextField").WithDisplayName("Action To Execute").WithSetting("TextFieldSettings.Flavor", "TextArea")))));
      ContentDefinitionManager.AlterTypeDefinition( "AppDirectRequest", (Action<ContentTypeDefinitionBuilder>) (cfg => cfg.WithPart(typeof (AppDirectUserPart).Name).WithPart("CommonPart").WithPart("AppDirectRequestPart")));
      return 5;
    }

    public int UpdateFrom5()
    {
            SchemaBuilder.AlterTable(typeof (AppDirectUserPartRecord).Name, (Action<AlterTableCommand>) (table => table.AddColumn<string>("CompanyCountry", (Action<AddColumnCommand>) null)));
            SchemaBuilder.AlterTable(typeof (AppDirectUserPartRecord).Name, (Action<AlterTableCommand>) (table => table.AddColumn<string>("CompanyName", (Action<AddColumnCommand>) null)));
            SchemaBuilder.AlterTable(typeof (AppDirectUserPartRecord).Name, (Action<AlterTableCommand>) (table => table.AddColumn<string>("CompanyUuidCreator", (Action<AddColumnCommand>) null)));
            SchemaBuilder.AlterTable(typeof (AppDirectUserPartRecord).Name, (Action<AlterTableCommand>) (table => table.AddColumn<string>("CompanyWebSite", (Action<AddColumnCommand>) null)));
            SchemaBuilder.AlterTable(typeof (AppDirectUserPartRecord).Name, (Action<AlterTableCommand>) (table => table.AddColumn<string>("OpenIdCreator", (Action<AddColumnCommand>) null)));
            SchemaBuilder.AlterTable(typeof (AppDirectUserPartRecord).Name, (Action<AlterTableCommand>) (table => table.AddColumn<string>("UuidCreator", (Action<AddColumnCommand>) null)));
            ContentDefinitionManager.AlterPartDefinition( "AppDirectRequestPart", (Action<ContentPartDefinitionBuilder>) (b => MetaDataExtensions.Attachable(b, false).WithField("Request", (Action<ContentPartFieldDefinitionBuilder>) (cfg => cfg.OfType("TextField").WithDisplayName("Json Response").WithSetting("TextFieldSettings.Flavor", "TextArea"))).WithField("Action", (Action<ContentPartFieldDefinitionBuilder>) (x => x.OfType("TextField").WithDisplayName("Action To Execute").WithSetting("TextFieldSettings.Flavor", "TextArea"))).WithField("Edition", (Action<ContentPartFieldDefinitionBuilder>) (x => x.OfType("TextField").WithDisplayName("Edition")))));
      return 6;
    }
        public int UpdateFrom6() {
            //ContentDefinitionManager.AlterTypeDefinition("AppDirectRequest", x => x
            //     .WithPart(typeof(DynamicButtonToWorkflowsPart).Name)
            //    );
            return 7;
        }
        public int UpdateFrom7() {
            SchemaBuilder.CreateTable(typeof(AppDirectSettingsPartRecord).Name, table => table
                        .ContentPartRecord()
                        .Column<string>("ConsumerKey")
                        .Column<string>("ConsumerSecret")
         );
            return 8;
        }
        public int UpdateFrom8() {
            ContentDefinitionManager.AlterPartDefinition("AppDirectRequestPart", b => b
            .WithField("State", cfg => cfg.OfType("TextField")
            .WithDisplayName("Order State")
            ));
            return 9;
        }
        public int UpdateFrom9() {
            ContentDefinitionManager.AlterTypeDefinition("AppDirectRequest", x => x
                 .WithPart(typeof(AppDirectButtonPart).Name)
                );
            return 10;
        }
    }
}
