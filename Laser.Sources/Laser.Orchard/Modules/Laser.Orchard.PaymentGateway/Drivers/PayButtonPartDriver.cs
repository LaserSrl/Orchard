using Laser.Orchard.PaymentGateway.Models;
using Laser.Orchard.PaymentGateway.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Common.Fields;
using Orchard.Fields.Fields;
using Orchard.Localization;
using Orchard.MediaLibrary.Fields;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentGateway.Drivers {
    public class PayButtonPartDriver : ContentPartDriver<PayButtonPart> {
        private Localizer L;
        private readonly ITokenizer _tokenizer;

        public PayButtonPartDriver(ITokenizer tokenizer) {
            L = NullLocalizer.Instance;
            _tokenizer = tokenizer;
        }
        protected override string Prefix {
            get {
                return "Laser.Orchard.PaymentGateway";
            }
        }
        protected override DriverResult Display(PayButtonPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "Detail") {
                var partSettings = part.Settings.GetModel<PayButtonPartSettings>();
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                dynamic ci = part.ContentItem;
                var viewModel = new PaymentVM();
                viewModel.Record.ContentItemId = part.Id;
                viewModel.ContentItem = part.ContentItem;
                viewModel.Record.Currency = partSettings.DefaultCurrency;
                if (string.IsNullOrWhiteSpace(partSettings.CurrencyField) == false) {
                    viewModel.Record.Currency = _tokenizer.Replace(partSettings.CurrencyField, tokens);
                }
                viewModel.Record.Amount = Convert.ToDecimal(_tokenizer.Replace(partSettings.AmountField, tokens), CultureInfo.InvariantCulture);
                if (part.ContentItem.Parts.SingleOrDefault(x => x.PartDefinition.Name == "TitlePart") != null) {
                    viewModel.Record.Reason = ci.TitlePart.Title;
                }
                return ContentShape("Parts_PayButton",
                    () => shapeHelper.Parts_PayButton(Payment: viewModel));
            }
            else {
                return null;
            }
        }
        //GET
        protected override DriverResult Editor(PayButtonPart part, dynamic shapeHelper) {
            var viewModel = part;
            return ContentShape("Parts_PayButton_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/PayButtonEdit",
                    Model: viewModel,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(PayButtonPart part, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, shapeHelper);
        }
    }
}