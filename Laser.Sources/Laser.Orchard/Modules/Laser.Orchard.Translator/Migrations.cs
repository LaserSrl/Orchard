using Orchard.Data.Migration;

namespace Laser.Orchard.Translator {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("TranslationRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("ContainerName")
                .Column<string>("ContainerType", column => column.WithLength(1))
                .Column<string>("Context", col => col.Unlimited())
                .Column<string>("Message", col => col.Unlimited())
                .Column<string>("TranslatedMessage", col => col.Unlimited())
                .Column<string>("Language")
                );

            return 1;
        }
    }
}