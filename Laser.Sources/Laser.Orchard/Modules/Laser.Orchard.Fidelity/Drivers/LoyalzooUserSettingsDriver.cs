using Laser.Orchard.Fidelity.Models;
using Laser.Orchard.Fidelity.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Security;
using System;
using System.Text;

namespace Laser.Orchard.Fidelity.Drivers {
    public class LoyalzooUserSettingsDriver : ContentPartDriver<LoyalzooUserPart> {
        public Localizer T { get; set; }
        private const string CONTROLLER_ACTION = "account/register";

        private readonly IEncryptionService _encryptionService;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private const string TemplateName = "Parts/LoyalzooUserSettings";
        private string currentControllerAction {
            get { //MVC 4
                return (_controllerContextAccessor.Context.RouteData.Values["controller"] + "/" + _controllerContextAccessor.Context.RouteData.Values["action"]).ToLowerInvariant();
            }
        }

        public LoyalzooUserSettingsDriver(IEncryptionService encryptionService, IControllerContextAccessor controllerContextAccessor) {
            T = NullLocalizer.Instance;
            _encryptionService = encryptionService;
            _controllerContextAccessor = controllerContextAccessor;
        }

        protected override string Prefix { get { return "LoyalzooUserSettings"; } }

        protected override DriverResult Editor(LoyalzooUserPart part, dynamic shapeHelper) {
            if (currentControllerAction == CONTROLLER_ACTION) return null;// nulla deve essere mostrato in fase di registrazione

            return ContentShape("Parts_LoyalzooUserSettings_Edit", () => {
                                    var model = new LoyalzooUserSettingsViewModel {
                                        LoyalzooUsername = part.LoyalzooUsername,
                                        LoyalzooPassword = "",
                                        CustomerSessionId = part.CustomerSessionId,
                                        PartId = part.Id
                                    };

                                    return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
                                }
                    );
        }

        protected override DriverResult Editor(LoyalzooUserPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (currentControllerAction == CONTROLLER_ACTION) return null; // nulla deve essere mostrato in fase di registrazione

            string oldPwd = part.LoyalzooPassword;
            string oldId = part.CustomerSessionId;

            LoyalzooUserSettingsViewModel userVM = new LoyalzooUserSettingsViewModel();

            if (updater.TryUpdateModel(userVM, Prefix, null, null)) {
                part.LoyalzooUsername = userVM.LoyalzooUsername;
                part.CustomerSessionId = userVM.CustomerSessionId;

                if (!String.IsNullOrWhiteSpace(userVM.LoyalzooPassword)) {
                    string encryptedPwd = Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(userVM.LoyalzooPassword)));

                    if (encryptedPwd != oldPwd && userVM.CustomerSessionId == oldId) part.CustomerSessionId = "";

                    part.LoyalzooPassword = encryptedPwd;
                } else
                    part.LoyalzooPassword = oldPwd;

                // Se l'utente ha tentato di cancellare tutte le credenziali cancello anche la password
                if (string.IsNullOrWhiteSpace(userVM.LoyalzooUsername) && string.IsNullOrWhiteSpace(userVM.CustomerSessionId))
                    part.LoyalzooPassword = "";
            }

            return Editor(part, shapeHelper);
        }
    }
}