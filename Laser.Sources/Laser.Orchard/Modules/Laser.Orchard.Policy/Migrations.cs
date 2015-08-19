using System;
using System.Data;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using OrchardFields = Orchard.Fields;

namespace Laser.Orchard.Policy {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("PolicyTextInfoPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("UserHaveToAccept")
                .Column<int>("Priority"));

            SchemaBuilder.CreateTable("UserPolicyPartRecord", table => table
                .ContentPartRecord());

            SchemaBuilder.CreateTable("UserPolicyAnswersRecord", table => table
                .Column<int>("Id", col => col.Identity().PrimaryKey())
                .Column<int>("UserPolicyPartRecord_Id")
                .Column<int>("PolicyTextInfoPartRecord_Id")
                .Column<DateTime>("AnswerDate")
                .Column<bool>("Accepted")
                );

            // Creating Policy ContentPart
            ContentDefinitionManager.AlterPartDefinition("PolicyPart", part => part
                .Attachable(true)
                );
            // Creating PolicyText ContentPart
            ContentDefinitionManager.AlterPartDefinition("PolicyTextInfoPart", part => part
                .Attachable(false)
                );

            // Creating PolicyText ContentItem
            ContentDefinitionManager.AlterTypeDefinition("PolicyText", content => content
                .Draftable(false)
                .Creatable()
                .WithPart("CommonPart")
                .WithPart("TitlePart")
                .WithPart("AutoroutePart", part => part.WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Policy', Pattern: 'policy/{Content.Slug}', Description: 'policy/my-page'}]"))
                .WithPart("BodyPart")
                .WithPart("LocalizationPart")
                .WithPart("PolicyTextInfoPart")
                );

            ContentDefinitionManager.AlterTypeDefinition("User", content => content
                .WithPart("UserPolicyPart"));

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("PolicyTextInfoPartRecord", table => table
                .AddColumn<string>("PolicyType", col => col.WithLength(25)));
            return 2;
        }

    }
}
