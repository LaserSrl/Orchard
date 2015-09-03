using Orchard.Data.Migration;

namespace Laser.Orchard.Translator
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable("TranslationRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("ContainerName")
                .Column<string>("ContainerType", column => column.WithLength(1))
                .Column<string>("Context")
                .Column<string>("Message")
                .Column<string>("TranslatedMessage")
                .Column<string>("Language")
                );

            return 1;
        }
    }
}