using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo {
    public class VimeoMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("VimeoSettingsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("AccessToken")
                    .Column<string>("ChannelName")
                    .Column<string>("GroupName")
                    .Column<string>("AlbumName")
                );

            return 1;
        }
    }
}