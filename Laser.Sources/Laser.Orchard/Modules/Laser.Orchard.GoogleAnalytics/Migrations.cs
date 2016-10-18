using Orchard.Data.Migration;
using Orchard;
using Orchard.ContentManagement;
using Laser.Orchard.GoogleAnalytics.Models;

namespace Laser.Orchard.GoogleAnalytics {
	public class Migrations : DataMigrationImpl {
		public int Create() {
			SchemaBuilder.CreateTable("GoogleAnalyticsSettingsPartRecord", 
				table => table
					.ContentPartRecord()
					.Column<string>("GoogleAnalyticsKey")
					.Column<string>("DomainName")
					.Column<bool>("UseAsyncTracking")
					.Column<bool>("TrackOnAdmin")
				);
			return 1;
		}

        public int UpdateFrom1() {
            SchemaBuilder.AlterTable("GoogleAnalyticsSettingsPartRecord",
                table => table.AddColumn<bool>("AnonymizeIp"));
            return 2;
        }
	}
}