using System.Data;
using Laser.Orchard.Maps.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;

namespace Laser.Orchard.Maps
{
    public class Migrations : DataMigrationImpl
    {
        public int Create()
        {
            // Creating table MapRecord
            SchemaBuilder.CreateTable("MapRecord", table => table
                .ContentPartRecord()
                .Column("Latitude", DbType.Single)
                .Column("Longitude", DbType.Single)
            );

            ContentDefinitionManager.AlterPartDefinition(typeof(MapPart).Name, cfg => cfg
                .Attachable());

            return 1;
        }

        public int UpdateFrom1()
        {
            // Create a new widget content type with our map
            ContentDefinitionManager.AlterTypeDefinition("MapWidget", cfg => cfg
                .WithPart("MapPart")
                .WithPart("WidgetPart")
                .WithPart("CommonPart")
                .WithSetting("Stereotype", "Widget"));

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("MapRecord", table => table.AddColumn("LocationInfo",DbType.String, column=>column.WithLength(100)));
            SchemaBuilder.AlterTable("MapRecord", table => table.AddColumn("LocationAddress", DbType.String, column => column.WithLength(255)));
            return 3;
        }

        public int UpdateFrom3() { 
            ContentDefinitionManager.AlterPartDefinition(typeof(MapPart).Name, cfg => cfg
                .WithField("MapSourceFile",fieldBuilder => fieldBuilder
                    .WithDisplayName("Map Source")
                    .OfType("MediaLibraryPickerField")
                    .WithSetting("MediaLibraryPickerFieldSettings.Required","False")
                    .WithSetting(" MediaLibraryPickerFieldSettings.Multiple","False"))
                .Attachable());
            return 4;
        }
    }
}
