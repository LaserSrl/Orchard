namespace Proligence.QrCodes.Drivers
{
    using Orchard.ContentManagement;
    using Orchard.ContentManagement.Drivers;
    using Orchard.Localization;
    using Orchard.UI.Notify;

    using Proligence.QrCodes.Models;
    using Proligence.QrCodes.Settings;

    public class QrCodePartDriver : ContentPartDriver<QrCodePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/QrCode";

        public Localizer T { get; set; }

        public QrCodePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(QrCodePart part, string displayType, dynamic shapeHelper)
        {
            if (string.IsNullOrWhiteSpace(part.ActualValue)) return null;

            return ContentShape("Parts_QrCode",
                () => shapeHelper.Parts_QrCode(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(QrCodePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_QrCode",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(QrCodePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                var settings = part.Settings.GetModel<QrCodeTypePartSettings>();

                if (part.Size != settings.Size) part.Record.Size = part.Size;
                if (part.Value != settings.Value) part.Record.Value = part.Value;

                _notifier.Information(T("QR Code edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during QR Code update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}