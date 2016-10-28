using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Laser.Orchard.Policy.Drivers {
    public class UserPolicyPartDriver : ContentPartDriver<UserPolicyPart> {
        private const string CONTROLLER_ACTION = "account/register";
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private string currentControllerAction {
            get { //MVC 4
                return (_controllerContextAccessor.Context.RouteData.Values["controller"] + "/" + _controllerContextAccessor.Context.RouteData.Values["action"]).ToLowerInvariant();
            }
        }

        public UserPolicyPartDriver(IControllerContextAccessor controllerContextAccessor) {
            T = NullLocalizer.Instance;
            _controllerContextAccessor = controllerContextAccessor;

        }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "UserPolicy"; }
        }

        protected override DriverResult Editor(UserPolicyPart part, dynamic shapeHelper) {
            if (currentControllerAction == CONTROLLER_ACTION) return null; // nulla deve essere mostrato in fase di registrazione

            return ContentShape("Parts_UserPolicy_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/UserPolicy_Edit",
                                 Model: part,
                                 Prefix: Prefix));
        }
        protected override DriverResult Editor(UserPolicyPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (currentControllerAction == CONTROLLER_ACTION) return null;// nulla deve essere mostrato in fase di registrazione
            if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                updater.AddModelError("UserPolicyPartError", T("UserPolicyPart Error"));
            }
            return Editor(part, shapeHelper);
        }

    }
}