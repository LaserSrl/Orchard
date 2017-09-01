using System;
using System.Web.Security;
using Laser.Orchard.AppDirect.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Data.Migration.Schema;
using Orchard.Security;

namespace Laser.Orchard.AppDirect {
    public class Migration : DataMigrationImpl {
        private readonly IMembershipService _membershipService;
        public Migration(IMembershipService membershipService) {
            _membershipService = membershipService;
        }
        public int Create() {
            SchemaBuilder.CreateTable(typeof(LogEventsRecord).Name, table => table
                .Column<int>("Id", t => t.PrimaryKey().Identity())
                .Column<DateTime>("TimeStamp")
                .Column<string>("EventType")
                .Column<string>("Method")
                .Column<string>("Log", column => column.Unlimited())
            );
            SchemaBuilder.CreateTable(typeof(AppDirectUserPartRecord).Name, table => table
                 .ContentPartRecord()
                 .Column<string>("Email")
                 .Column<string>("FirstName")
                 .Column<string>("Language")
                 .Column<string>("LastName")
                 .Column<string>("Locale")
                 .Column<string>("CompanyCountry")
                 .Column<string>("CompanyName")
                 .Column<string>("CompanyUuidCreator")
                 .Column<string>("CompanyWebSite")
                 .Column<string>("OpenIdCreator")
                 .Column<string>("UuidCreator")
                 .Column<string>("AccountIdentifier")
            );
            ContentDefinitionManager.AlterPartDefinition(typeof(AppDirectUserPart).Name, cfg => cfg
                 .Attachable(false)
            );
            ContentDefinitionManager.AlterPartDefinition("AppDirectRequestPart", cfg => cfg
                 .Attachable(false)
                 .WithField("Request", b => b.OfType("TextField")
                    .WithDisplayName("Json Response")
                    .WithSetting("TextFieldSettings.Flavor", "TextArea"))
                 .WithField("Action", x => x.OfType("TextField")
                    .WithDisplayName("Action To Execute")
                    .WithSetting("TextFieldSettings.Flavor", "TextArea"))
                 .WithField("Edition", x => x.OfType("TextField")
                    .WithDisplayName("Edition"))
                .WithField("State", x => x.OfType("TextField")
                    .WithDisplayName("Order State"))
                .WithField("Uri", x => x.OfType("TextField")
                    .WithDisplayName("Uri to Call")
                    .WithSetting("TextFieldSettings.Flavor", "Large"))
                .WithField("ProductKey", x => x.OfType("TextField")
                    .WithDisplayName("ProductKey"))
            );
            ContentDefinitionManager.AlterTypeDefinition("AppDirectRequest", cfg => cfg
                 .WithPart(typeof(AppDirectUserPart).Name)
                 .WithPart("CommonPart")
                 .WithPart("AppDirectRequestPart")
                 .WithPart(typeof(AppDirectButtonPart).Name)
            );
            SchemaBuilder.CreateTable(typeof(AppDirectSettingsRecord).Name, table => table
                 .Column<int>("Id", column => column.PrimaryKey().Identity())
                 .Column<string>("TheKey")
                 .Column<string>("ConsumerKey")
                 .Column<string>("ConsumerSecret")
             );
            var user = _membershipService.GetUser("Market_AppDirect");
            if (user == null) {
                var password = Membership.GeneratePassword(10, 5);
                _membershipService.CreateUser(new CreateUserParams("Market_AppDirect", password, "Market_AppDirect@laser-group.com", "Auto Registered User", password, true));
            }
            return 1;        }
    }
}