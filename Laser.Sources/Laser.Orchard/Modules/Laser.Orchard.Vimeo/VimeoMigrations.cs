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

        public int UpdateFrom3() {
            SchemaBuilder.DropTable("UploadsInProgressRecord");

            SchemaBuilder.CreateTable("UploadsInProgressRecord",
                table => table
                    .Column<int>("Id", col => col.Identity().PrimaryKey())
                    .Column<int>("UploadSize")
                    .Column<int>("UploadedSize")
                    .Column<string>("Uri")
                    .Column<string>("CompleteUri")
                    .Column<string>("TicketId")
                    .Column<string>("UploadLinkSecure")
                );

            SchemaBuilder.CreateTable("UploadsCompleteRecord",
                table => table
                    .Column<int>("Id", col => col.Identity().PrimaryKey())
                    .Column<string>("Uri")
                    .Column<int>("ProgressId")
                );

            //update to settings: added default video settings
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("License")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("Privacy")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("Password")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<bool>("ReviewLink", col => col.WithDefault(false))
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("Locale")
                );
            SchemaBuilder.AlterTable("VimeoSettingsPartRecord",
                table => table
                    .AddColumn<string>("ContentRatings")
                );

            return 4;
        }

    }
}