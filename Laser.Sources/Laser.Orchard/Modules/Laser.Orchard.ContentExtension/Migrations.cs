using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
namespace Laser.Orchard.ContentExtension {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("ContentTypePermissionRecord", table => table
                     .Column<int>("Id", column => column.PrimaryKey().Identity())
                     .Column<string>("ContentType")
                     .Column<string>("PostPermission")
                     .Column<string>("GetPermission")
                     .Column<string>("DeletePermission")
                     .Column<string>("PublishPermission")
                  );
            return 1;
        }
        public int UpdateFrom1() {
 
            return 2;
        }
       public int  UpdateFrom2(){
        SchemaBuilder.AlterTable("ContentTypePermissionRecord", table => table 
           .AddColumn<string>("PublishPermission")
           );
            return 3;
        }
    }
}