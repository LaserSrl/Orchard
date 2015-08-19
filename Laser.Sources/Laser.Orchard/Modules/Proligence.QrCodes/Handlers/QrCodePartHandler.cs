namespace Proligence.QrCodes.Handlers
{
    using System.Collections.Generic;

    using Orchard.ContentManagement.Handlers;
    using Orchard.Data;
    using Orchard.Tokens;

    using Proligence.QrCodes.Models;
    using Proligence.QrCodes.Settings;

    public class QrCodePartHandler : ContentHandler
    {
        public QrCodePartHandler(IRepository<QrCodePartRecord> repository, ITokenizer tokenizer)
        {
            this.Filters.Add(StorageFilter.For(repository));
            this.OnLoaded<QrCodePart>((ctx, part) =>
                {
                    var settings = part.Settings.GetModel<QrCodeTypePartSettings>();
                    part.Size = part.Record.Size == default(int) ? settings.Size : part.Record.Size;
                    part.Value = string.IsNullOrWhiteSpace(part.Record.Value) ? settings.Value : part.Record.Value;

                    var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };

                    part.ActualValue = tokenizer.Replace(part.Value, tokens);
                });
        }
    }
}
