using Laser.Orchard.Payment.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;

namespace Laser.Orchard.Payment.Handlers {
    public class PaymentSettingsHandler : ContentHandler {
        public PaymentSettingsHandler(IRepository<PaymentSettingsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<PaymentSettingsPart>("Site"));
            T = NullLocalizer.Instance;
            OnGetContentItemMetadata<PaymentSettingsPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Payment"))));
        }

        public Localizer T { get; set; }
    }
}