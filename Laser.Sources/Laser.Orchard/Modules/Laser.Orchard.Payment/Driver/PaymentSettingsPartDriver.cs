using Laser.Orchard.Payment.Models;
using Laser.Orchard.Payment.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Payment.Drivers {

    public class PaymentSettingsPartDriver : ContentPartDriver<PaymentSettingsPart> {
        private readonly IOrchardServices _orchardServices;
        public PaymentSettingsPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        protected override string Prefix {
            get { return "Laser.Payment.Settings"; }
        }


        protected override DriverResult Editor(PaymentSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);

        }



        protected override DriverResult Editor(PaymentSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            return ContentShape("Parts_PaymentSettings_Edit", () => {
                var viewModel = new PaymentSettingsVM();
                var getpart = _orchardServices.WorkContext.CurrentSite.As<PaymentSettingsPart>();
                viewModel.GestpayShopLogin = getpart.GestpayShopLogin;
                viewModel.GestpayTest = getpart.GestpayTest;
                viewModel.PaymentMethodSelected = getpart.PaymentMethodSelected;
                if (updater != null) {
                    if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                        part.GestpayShopLogin = viewModel.GestpayShopLogin;
                        part.GestpayTest = viewModel.GestpayTest;
                        part.PaymentMethodSelected = viewModel.PaymentMethodSelected;
                    }
                } else {
                    viewModel.GestpayShopLogin = part.GestpayShopLogin;
                    viewModel.GestpayTest = part.GestpayTest;
                    viewModel.PaymentMethodSelected = part.PaymentMethodSelected;
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts/PaymentSettings_Edit", Model: viewModel, Prefix: Prefix);
            })
                .OnGroup("Payment");
        }

        protected override void Importing(PaymentSettingsPart part, ImportContentContext context) {
            //           context.ImportAttribute(part.PartDefinition.Name, "DefaultParserEngine", x => part.DefaultParserIdSelected = x);
        }

        protected override void Exporting(PaymentSettingsPart part, ExportContentContext context) {
            //           context.Element(part.PartDefinition.Name).SetAttributeValue("DefaultParserEngine", part.DefaultParserIdSelected);


        }

    }
}