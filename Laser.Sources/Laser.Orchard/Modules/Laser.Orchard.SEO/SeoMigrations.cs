using Laser.Orchard.SEO.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data;
using Orchard.Data.Migration;


namespace Laser.Orchard.SEO {


    public class SeoMigrations : DataMigrationImpl {
        private readonly IRepository<SeoRecord> _record;
        private readonly IRepository<SeoVersionRecord> _versionRecord;

        public SeoMigrations(IRepository<SeoRecord> record, IRepository<SeoVersionRecord> versionRecord) {
            _record = record;
            _versionRecord = versionRecord;
        }


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

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("SeoVersionRecord",
             table => table
              .ContentPartVersionRecord()
                                    .Column<string>("TitleOverride", c => c.WithLength(255))
                                    .Column<string>("Keywords", c => c.WithLength(255))
                                    .Column<string>("Description", c => c.WithLength(400))
            );

            foreach (var row in _record.Table) {
                foreach (var version in row.ContentItemRecord.Versions) {
                    var newItem = new SeoVersionRecord() {
                        ContentItemRecord = row.ContentItemRecord,
                        ContentItemVersionRecord = version,
                        TitleOverride = row.TitleOverride,
                        Keywords = row.Keywords,
                        Description = row.Description,
                    };
                    _versionRecord.Create(newItem);
                }
            }
            return 2;
        }

    }
}