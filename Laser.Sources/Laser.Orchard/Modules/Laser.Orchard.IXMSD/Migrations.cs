using Laser.Orchard.IXMSD.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Laser.Orchard.IXMSD {

    public class Migrations : DataMigrationImpl {

        public int Create() {
            //ContentDefinitionManager.AlterPartDefinition(typeof(UserAgentRedirectPart).Name, cfg => cfg
            //    .Attachable());
            ContentDefinitionManager.AlterTypeDefinition("Video", cfg => cfg
                  .WithPart(typeof(IXMSDPart).Name));
            return 1;
        }
    }
}