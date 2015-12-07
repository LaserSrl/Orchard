using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Queries {

    public class Migrations : DataMigrationImpl {

        public int Create() {
            ContentDefinitionManager.AlterPartDefinition(
               "MyCustomQueryPart",
                b => b
                   .Attachable(false)
                   .WithField("QueryString", cfg => cfg.OfType("TextField")
                       .WithDisplayName("Text Query")
                         .WithSetting("TextFieldSettings.Flavor", "TextArea") 
                        .WithSetting("TextFieldSettings.Required", "true")
                        .WithSetting("TextFieldSettings.Hint", "Insert a query")
                        )
                   .WithField("Options", cfg => cfg.OfType("TextField").WithDisplayName("Options"))
            );
            ContentDefinitionManager.AlterTypeDefinition(
              "MyCustomQuery",
              type => type
                  .WithPart("TitlePart")
                  //.WithPart("AutoroutePart", p => p
                  //  .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
                  //  .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
                  //  .WithSetting("AutorouteSettings.PatternDefinitions", @"[{Name:'Title', Pattern:'{Content.Slug}',Description:'Title of campaign'}]")
                  //  .WithSetting("AutorouteSettings.DefaultPatternIndex", "0")
                  //     )
                .WithPart("IdentityPart")

                .WithPart("MyCustomQueryPart")

                .WithPart("CommonPart")
                .Creatable(false)
                .DisplayedAs("CustomQuery")
              );

            return 1;
        }
    }
}