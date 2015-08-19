using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;


namespace Laser.Orchard.SEO {


  public class SeoMigrations : DataMigrationImpl {


    public int Create() {

      SchemaBuilder
        .CreateTable("SeoRecord",
                     table => table
                                .ContentPartRecord()
                                .Column<string>("TitleOverride", c => c.WithLength(255))
                                .Column<string>("Keywords", c => c.WithLength(255))
                                .Column<string>("Description", c => c.WithLength(400))
                                );

      ContentDefinitionManager
        .AlterPartDefinition("SeoPart",
                             cfg => cfg
                               .Attachable()
                               .WithDescription("Consente la personalizzazione degli attributi SEO (title, keywords, description, meta).")
                               );

      return 1;
    }


  }
}