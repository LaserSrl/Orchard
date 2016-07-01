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

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("UploadsInProgressRecord",
                table => table
                    .ContentPartRecord()
                    .Column<int>("UploadSize")
                    .Column<int>("UploadedSize")
                    .Column<string>("Uri")
                    .Column<string>("CompleteUri")
                    .Column<int>("TicketId")
                    .Column<string>("UploadLinkSecure")
                );

            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.AlterTable("UploadsInProgressRecord",
                table => table
                    .AlterColumn("TicketId",
                        col => col
                            .WithType(System.Data.DbType.String)
                        )
                );

            return 3;
        }

    }
}