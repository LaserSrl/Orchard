using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.MailCommunication {
    [OrchardFeature("Laser.Orchard.MailCommunication")]
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("MailCommunicationPartRecord", table => table
                .ContentPartRecord()
                .Column<bool>("MailMessageSent", col => col.WithDefault(false))
            );

            ContentDefinitionManager.AlterPartDefinition("MailCommunicationPart", part => part
                .WithField("Message", cfg => cfg.OfType("TextField").WithSetting("TextFieldSettings.Flavor", "html"))
                .WithField("RelatedMailContent", cfg => cfg.OfType("ContentPickerField")
                    .WithSetting("ContentPickerFieldSettings.Multiple", "True"))
                .Attachable());
            ContentDefinitionManager.AlterTypeDefinition("CommunicationAdvertising", type => type
                .WithPart("MailCommunicationPart")
                .WithPart("CustomTemplatePickerPart"));
            return 1;
        }
    }
}