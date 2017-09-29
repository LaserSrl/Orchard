using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Contrib.Profile {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("User",
                cfg => cfg
                    .WithPart("ProfilePart")
                );

            return 1;
        }

        //public int UpdateFrom1() {
        //    ContentDefinitionManager.AlterPartDefinition("ProfilePart", builder => builder
        //        .Attachable(false));

        //    return 2;
        //}
    }
}