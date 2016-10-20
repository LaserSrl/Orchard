using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using System.Linq;

namespace Laser.Orchard.StartupConfig.Migrations
{
    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class TaxonomyExtensionsMigrations : DataMigrationImpl
    {
        public int Create()
        {
            ContentTypeDefinition taxonomyDefinition = ContentDefinitionManager.GetTypeDefinition("Taxonomy");

            if (taxonomyDefinition != null)
            {
                if (taxonomyDefinition.Parts.Where(x => x.PartDefinition.Name == "LocalizationPart").Count() == 0)
                {
                    ContentDefinitionManager.AlterTypeDefinition("Taxonomy",
                        cfg => cfg
                            .WithPart("LocalizationPart")
                        );
                }
            }

            return 1;
        }
    }
}