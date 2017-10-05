using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using System.Linq;

namespace Contrib.Profile {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            ContentDefinitionManager.AlterTypeDefinition("User",
                cfg => cfg
                    .WithPart("ProfilePart")
                );

            return 1;
        }

        ///// <summary>
        ///// This update is required by the fact we added the setting to choose which Parts and Fields
        ///// should be diplayed or edited on the front-end.
        ///// </summary>
        //public int UpdateFrom2() {
        //    ContentDefinitionManager.AlterPartDefinition("ProfilePart", builder => builder
        //        .Attachable(false));

        //    //All ContentTypes that contain a ProfilePart are affected by the changes
        //    var typeDefinitions = ContentDefinitionManager
        //        .ListTypeDefinitions()
        //        .Where(ctd => ctd.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart"));
        //    //By default, every part and field will be configured to not show on front-end, both 
        //    //for Display and Edit. This is a break with what was in place before, when everything
        //    //sould be shown in front-end. However, this is also safer.
        //    //We will only go and change the setting for those ContentParts that we know must be
        //    //available on the front-end.

        //    return 2;
        //}
    }
}