using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using System.Collections.Generic;
using Laser.Orchard.CulturePicker.Models;
using Orchard;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;
using Orchard.Localization.Models;
using System.Linq;
using Orchard.UI.Navigation;
using Orchard.Core.Navigation.Models;
using Orchard.Data;
using NHibernate.Criterion;
using Orchard.Core.Navigation.Services;

namespace Laser.Orchard.CulturePicker.Services {
    [OrchardFeature("Laser.Orchard.CulturePicker.TranslateMenuItems")]
    public class TranslateMenuItemServices : ITranslateMenuItemsServices {

        private readonly IContentManager _contentManager;
        private readonly ILocalizationService _localizationService;
        private readonly ISessionLocator _sessionLocator;
        private readonly IMenuService _menuService;

        public TranslateMenuItemServices(
            ILocalizationService localizationService, 
            IContentManager contentManager,
            ISessionLocator sessionLocator,
            IMenuService menuService) {

            _localizationService = localizationService;
            _contentManager = contentManager;
            _sessionLocator = sessionLocator;
            _menuService = menuService;
        }
        public bool TryTranslateAllSubmenus(TranslateMenuItemsPart part) {
            var menu = part.ContentItem;
            //get the Id of the translated menu
            int tMenuId = menu.Id;
            //get the target culture from the localization part in the menu
            var locPart = menu.As<LocalizationPart>(); //this contains the localization info
            string targetCulture = locPart.Culture.Culture;
            //find the master menu item
            var locInfo = _localizationService.GetLocalizations(menu, VersionOptions.Published);
            int masterId = 0;
            foreach (var loc in locInfo) {
                if (loc.MasterContentItem == null) { //found master
                    masterId = loc.Id;
                    part.FromLocale = loc.Culture.Culture;
                    break;
                }
            }
            var masterMenu = _menuService.GetMenu(masterId);//_contentManager.Get(masterId);
            var thisMenu = _menuService.GetMenu(tMenuId);

            var masterParts = _menuService.GetMenuParts(masterId);

            //duplicate the parts into the new menu
            foreach (var origPart in masterParts) {
                var menuPart = _contentManager.New<MenuPart>(origPart.ContentItem.ContentType);
                menuPart.MenuPosition = origPart.MenuPosition;
                menuPart.Menu = thisMenu;
                menuPart.MenuText = origPart.MenuText + " " + targetCulture;
                //Up to this point, we have replicated the menu structure (for this item)
                //We should see to relocalize the content of the menu item IF it has a translation already

                _contentManager.Create(menuPart);

            }

            return true;
        }

    }
}