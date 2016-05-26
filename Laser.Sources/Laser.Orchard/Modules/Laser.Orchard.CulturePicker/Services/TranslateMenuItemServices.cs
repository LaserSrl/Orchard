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

namespace Laser.Orchard.CulturePicker.Services {
    [OrchardFeature("Laser.Orchard.CulturePicker.TranslateMenuItems")]
    public class TranslateMenuItemServices : ITranslateMenuItemsServices {

        private readonly IContentManager _contentManager;
        private readonly ILocalizationService _localizationService;

        public TranslateMenuItemServices(
            ILocalizationService localizationService, 
            IContentManager contentManager) {

            _localizationService = localizationService;
            _contentManager = contentManager;
        }
        public bool TryTranslateAllSubmenus(TranslateMenuItemsPart part) {
            var menu = part.ContentItem;
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
            var masterMenuItem = _contentManager.Get(masterId).As<MenuPart>();

            return false;
        }
    }
}