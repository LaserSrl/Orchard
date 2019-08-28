using System.Linq;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Taxonomies.Services;
using Orchard.UI.Navigation;
using CorePermissions = Orchard.Core.Contents.Permissions;

namespace Orchard.Taxonomies {
    public class AdminMenu : INavigationProvider {
        private readonly IOrchardServices _services;
        private readonly ITaxonomyService _taxonomyService;

        public AdminMenu(
            IOrchardServices services,
            ITaxonomyService taxonomyService) {
            _services = services;
            _taxonomyService = taxonomyService;
        }
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder
                .AddImageSet("taxonomies")
                .Add(T("Taxonomies"), "4", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {

            var showMenu = _services.Authorizer.Authorize(Permissions.MergeTerms)
                || _services.Authorizer.Authorize(Permissions.EditTerm)
                || _services.Authorizer.Authorize(Permissions.CreateTerm)
                || _services.Authorizer.Authorize(Permissions.DeleteTerm);
            if (!showMenu) {
                // user may have permission to edit/create/delete only specific term types
                // because Securable configuration on them.
                foreach (var taxonomy in _taxonomyService.GetTaxonomies()) {
                    var fakeTerm = _taxonomyService.GetTerms(taxonomy.Id).FirstOrDefault() ??
                        _taxonomyService.NewTerm(taxonomy);
                    showMenu |= _services.Authorizer.Authorize(CorePermissions.CreateContent, fakeTerm)
                        || _services.Authorizer.Authorize(CorePermissions.EditContent, fakeTerm)
                        || _services.Authorizer.Authorize(CorePermissions.DeleteContent, fakeTerm);
                    if (showMenu) {
                        // we should display the menu so there is no need to search more
                        break;
                    }
                }
            }

            if (showMenu) {
                menu.Add(T("Manage Taxonomies"),
                    "1.0",
                    item => item.Action("Index", "Admin", new { area = "Orchard.Taxonomies" }));
            }
        }
    }

}
