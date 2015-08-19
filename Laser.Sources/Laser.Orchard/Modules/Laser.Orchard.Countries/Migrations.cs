using System.Data;
using System.Data.SqlClient;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;
using Laser.Orchard.Countries.Services;

namespace Laser.Orchard.Countries.Migrations {
    [OrchardFeature("Laser.Orchard.Countries")]
    public class Migrations : DataMigrationImpl {
        private readonly ICountriesService _countriesService;
        public Migrations(ICountriesService countriesService) {
            _countriesService = countriesService;
        }

        public int Create() {
            SchemaBuilder.CreateTable("CountriesRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Des_en", column => column.WithLength(100))
                .Column<string>("Des_fr", column => column.WithLength(100))
                .Column<string>("Des_local", column => column.WithLength(100))
                .Column<string>("Des_Region", column => column.WithLength(100))
                );
            SchemaBuilder.CreateTable("LaserCountriesZoneRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("Descrizione", column => column.WithLength(100))
                );
            SchemaBuilder.CreateTable("LaserLinkedCountriesZoneRecord", table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<int>("LaserCountriesZoneRecord_Id")
                .Column<int>("CountriesRecord_Id")
                );
            _countriesService.Inizialize();
            return 1;
        }

       
    }
}
