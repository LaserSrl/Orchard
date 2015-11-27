using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Twitter {

    public class TwitterMigrations : DataMigrationImpl {
        //  private readonly IUtilsServices _utilServices;

        //public Migrations(IUtilsServices utilsServices)
        //{
        //    _utilServices = utilsServices;
        //}

        /// <summary>
        /// This executes whenever this module is activated.
        /// </summary>
        public int Create() {
            SchemaBuilder.CreateTable("TwitterPostPartRecord", table => table
                .ContentPartRecord()
                .Column<string>("TwitterMessage", col => col.Unlimited())
                .Column<string>("TwitterDescription", col => col.Unlimited())
                .Column<string>("TwitterTitle")
                .Column<string>("TwitterName")
                .Column<string>("TwitterPicture")
                .Column<string>("TwitterLink")
                .Column<string>("AccountList")
                .Column<bool>("TwitterMessageSent", col => col.WithDefault(false))
            );

            ContentDefinitionManager.AlterPartDefinition(
                "TwitterPostPart",
                b => b
                .Attachable(true)
                );

            ContentDefinitionManager.AlterPartDefinition(
                "TwitterAccountPart",
                b => b
                .Attachable(false)
                );
            ContentDefinitionManager.AlterTypeDefinition(
                "SocialTwitterAccount",
                type => type
                .WithPart("TwitterAccountPart")
                .WithPart("IdentityPart")
                .WithPart("CommonPart")
                .Creatable(false)
                .Draftable(false)
          );

            return 1;
        }
        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("TwitterPostPartRecord", table => table
                .AddColumn<bool>("TwitterCurrentLink")
                );
            return 2;
        }
        public int UpdateFrom2() {
            return 3;
        }
        public int UpdateFrom3() {
            ContentDefinitionManager.AlterPartDefinition(
                   "TwitterPostPart",
                   b => b
                       
                   .WithField("TwitterImage",
                    field => field
                    .OfType("MediaLibraryPickerField")
                    .WithDisplayName("Twitter Image")
                    .WithSetting("MediaLibraryPickerFieldSettings.Multiple", "false")
                    .WithSetting("MediaLibraryPickerFieldSettings.AllowedExtensions", "jpg jpeg png gif")
                    )
            );
            return 4;
        }
        public int UpdateFrom4() {
            ContentDefinitionManager.AlterPartDefinition(
               "TwitterPostPart",
               b => b
            .RemoveField("TwitterImage"));
            ContentDefinitionManager.AlterPartDefinition(
                   "TwitterPostPart",
                   b => b
                   .WithField("TwitterImage",
                    field => field
                    .OfType("MediaLibraryPickerField")
                    .WithDisplayName("Twitter Image")
                    .WithSetting("MediaLibraryPickerFieldSettings.Multiple", "false")
                    .WithSetting("MediaLibraryPickerFieldSettings.AllowedExtensions", "jpg jpeg png gif")
                    )
            );
            return 5;
        }
    }
}