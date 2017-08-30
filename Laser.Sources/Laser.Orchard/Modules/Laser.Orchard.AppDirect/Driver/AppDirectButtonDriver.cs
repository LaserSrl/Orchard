using System;
using Laser.Orchard.AppDirect.Models;
using Laser.Orchard.AppDirect.Services;
using Laser.Orchard.AppDirect.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Laser.Orchard.AppDirect.Driver {
    public class AppDirectButtonDriver : ContentPartDriver<AppDirectButtonPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IAppDirectCommunication _appDirectCommunication;
        public Localizer T { get; set; }
        public AppDirectButtonDriver(IOrchardServices orchardServices,
            IAppDirectCommunication appDirectCommunication) {
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
            _appDirectCommunication = appDirectCommunication;
        }
        protected override string Prefix {
            get { return "Laser.Orchard.AppDirect.AppDirectButtonPart"; }
        }

        private ButtonVM GenerateButton(RequestState state) {
            var button = new ButtonVM();
            if (state == RequestState.ToCreate) {
                button.ButtonText = T("Confirm Subscription Order").ToString();
                button.ButtonAction = "ConfirmOrder";
            }
            return button;
        }
        protected override DriverResult Editor(AppDirectButtonPart part, dynamic shapeHelper) {
            string state = ((dynamic)part.ContentItem).AppDirectRequestPart.State.Value;
            var requestState = (RequestState)Enum.Parse(typeof(RequestState), state, true);
            var button = GenerateButton(requestState);
            return ContentShape("Parts_AppDirectButton",
                               () => shapeHelper.EditorTemplate(
                                   TemplateName: "Parts/AppDirectButton",
                                   Model: button,
                                   Prefix: Prefix));
        }
        protected override DriverResult Editor(AppDirectButtonPart part, IUpdateModel updater, dynamic shapeHelper) {
            var button = new ButtonVM();
            if (updater != null && _orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "ConfirmOrder") {
                button = GenerateButton(RequestState.ToCreate);
                var AccountIdentifier = _orchardServices.WorkContext.HttpContext.Request.Form["Laser.Orchard.AppDirect.AppDirectUserPart.AccountIdentifier"];
                if (!string.IsNullOrEmpty(AccountIdentifier)) {
                    string outresponse;
                    var data = "{\"success\":\"true\",\"accountIdentifier\":\"" + AccountIdentifier + "\"}";
                    var uri = ((dynamic)part.ContentItem).AppDirectRequestPart.Uri.Value + "/result";
                    if (_appDirectCommunication.MakeRequestToAppdirect(uri, Method.POST, data, out outresponse, "", "")) {
                        button.ButtonAction = "";
                        _appDirectCommunication.WriteEvent(EventType.Output, "Post async " + data + " " + outresponse + uri);
                        part.ContentItem.As<AppDirectUserPart>().AccountIdentifier = AccountIdentifier;
                        ((dynamic)part.ContentItem).AppDirectRequestPart.State.Value = RequestState.Created.ToString();
                        ((dynamic)part.ContentItem).AppDirectRequestPart.Action.Value = "nothing";
                    }
                }
                else {
                    updater.AddModelError("NoIdentifier", T("AccountIdentifier must not be empty"));
                }

            }
            return ContentShape("Parts_AppDirectButton",
                               () => shapeHelper.EditorTemplate(
                                   TemplateName: "Parts/AppDirectButton",
                                   Model: button,
                                   Prefix: Prefix));
        }
    }
}