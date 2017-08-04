using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.AppDirect.Models;
using Laser.Orchard.AppDirect.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Laser.Orchard.AppDirect.Driver {
    public class AppDirectButtonDriver : ContentPartDriver<AppDirectButtonPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IAppDirectCommunication _appDirectCommunication;

        public AppDirectButtonDriver(IOrchardServices orchardServices,
            IAppDirectCommunication appDirectCommunication) {
            _orchardServices = orchardServices;
            _appDirectCommunication = appDirectCommunication;
        }
        protected override string Prefix {
            get { return "Laser.Orchard.AppDirect.AppDirectButtonPart"; }
        }
        protected override DriverResult Editor(AppDirectButtonPart part, dynamic shapeHelper) {
            return ContentShape("Parts_AppDirectButton",
                               () => shapeHelper.EditorTemplate(
                                   TemplateName: "Parts/AppDirectButton",
                                   Model: part,
                                   Prefix: Prefix));
        }
        protected override DriverResult Editor(AppDirectButtonPart part, IUpdateModel updater, dynamic shapeHelper) {

            if (updater != null &&  _orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "ConfirmOrder" ) {
                //part.ContentItem
                //  _appDirectCommunication.MakeRequestToAppdirect();
                string outresponse;
                string data = "success=true&accountIdentifier=teoric";
                var uri = ((dynamic)part.ContentItem).AppDirectRequestPart.Uri.Value + "/result";
                if (_appDirectCommunication.MakeRequestToAppdirect(uri, Method.POST, data, out outresponse, "", "") && !string.IsNullOrEmpty(outresponse)) {
                }

                }
                return ContentShape("Parts_AppDirectButton",
                               () => shapeHelper.EditorTemplate(
                                   TemplateName: "Parts/AppDirectButton",
                                   Model: part,
                                   Prefix: Prefix));
        }
    }
}