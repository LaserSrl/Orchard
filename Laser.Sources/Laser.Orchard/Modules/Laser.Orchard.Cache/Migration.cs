using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Cache {
    public class Migration : DataMigrationImpl {
        public int Create() {
            return 1;
        }
        public int UpdateFrom1() {

            return 2;
        }
        public int UpdateFrom2() {
            SchemaBuilder.CreateTable("CacheUrlRecord",
               table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("CacheDuration", column => column.WithDefault(0))
                .Column<int>("CacheGraceTime", column => column.WithDefault(0))
                .Column<int>("Priority", column => column.WithDefault(0))
                .Column<string>("CacheURL", column => column.WithLength(500))
                .Column<string>("CacheToken", column => column.WithLength(500))
           );
            return 3;
        }
    }
}