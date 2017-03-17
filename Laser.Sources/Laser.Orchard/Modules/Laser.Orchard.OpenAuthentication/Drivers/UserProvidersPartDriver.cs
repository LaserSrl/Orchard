
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.ViewModels;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard;
using Orchard.ContentManagement.Handlers;
using System.Xml.Linq;

namespace Laser.Orchard.OpenAuthentication.Drivers {
    
    public class UserProvidersPartDriver : ContentPartDriver<UserProvidersPart> {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private const string TemplateName = "Parts/Providers.UserProviders";

        public UserProvidersPartDriver(
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService) {
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
        }

        protected override string Prefix {
            get {
                return "UserProviders";
            }
        }

        protected override DriverResult Editor(UserProvidersPart userProvidersPart, dynamic shapeHelper) {
            if (!_authorizationService.TryCheckAccess(StandardPermissions.SiteOwner, _authenticationService.GetAuthenticatedUser(), userProvidersPart))
                return null;

            return ContentShape("Parts_Providers_UserProviders_Edit", () => {
                        var model = new UserProvidersViewModel {
                            Providers = userProvidersPart.Providers,
                        };
                        return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
                    });
        }

        protected override DriverResult Editor(UserProvidersPart userProvidersPart, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(userProvidersPart, shapeHelper);
        }


        ///////////////////////////
        //protected override void Exporting(UserProvidersPart part, ExportContentContext context) {

        //    var root = context.Element(part.PartDefinition.Name);
        //    if (part.Providers != null) {

        //        foreach (UserProviderEntry recProvEntry in part.Providers) {
        //            XElement avCult = new XElement("Providers");
        //            avCult.SetAttributeValue("ProviderName", recProvEntry.ProviderName);
                    
        //            //avCult.SetAttributeValue("DisplayName", recProvEntry.ProviderUserId);
                   
        //            root.Add(avCult);
        //        }
        //    }
        //}


    }
}