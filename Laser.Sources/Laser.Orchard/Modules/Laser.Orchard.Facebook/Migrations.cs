using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Facebook {

    public class FacebookMigrations : DataMigrationImpl {
        //  private readonly IUtilsServices _utilServices;

        //public Migrations(IUtilsServices utilsServices)
        //{
        //    _utilServices = utilsServices;
        //}

        /// <summary>
        /// This executes whenever this module is activated.
        /// </summary>
        public int Create() {
            SchemaBuilder.CreateTable("FacebookPostPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("FacebookMessage", col => col.Unlimited())
                .Column<string>("FacebookDescription", col => col.Unlimited())
                .Column<string>("FacebookCaption")
                .Column<string>("FacebookName")
                .Column<string>("FacebookPicture")
                .Column<string>("FacebookLink")
                .Column<string>("AccountList")
                .Column<bool>("FacebookMessageSent", col => col.WithDefault(false))
            );

            ContentDefinitionManager.AlterPartDefinition(
                "FacebookPostPart",
                b => b
                .Attachable(true)
                );

            ContentDefinitionManager.AlterPartDefinition(
                "FacebookAccountPart",
                b => b
                .Attachable(false)
                );
            ContentDefinitionManager.AlterTypeDefinition(
                "SocialFacebookAccount",
                type => type
                .WithPart("FacebookAccountPart")
                .WithPart("IdentityPart")
                .WithPart("CommonPart")
                .Creatable(false)
                .Draftable(false)
          );

            return 1;
        }
    }
}