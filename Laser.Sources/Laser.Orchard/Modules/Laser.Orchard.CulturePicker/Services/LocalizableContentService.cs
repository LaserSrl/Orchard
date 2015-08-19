using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Laser.Orchard.CulturePicker.Models;
using System;


namespace Laser.Orchard.CulturePicker.Services {


  public class LocalizableContentService : ILocalizableContentService {


    private readonly IContentManager _contentManager;
    private readonly ICultureManager _cultureManager;
    private readonly ILocalizationService _localizationService;
    private readonly ICulturePickerSettingsService _culturePickerSettingsService;


    public LocalizableContentService(ILocalizationService localizationService, ICultureManager cultureManager, IContentManager contentManager, ICulturePickerSettingsService culturePickerSettingsService) {
      _localizationService = localizationService;
      _cultureManager = cultureManager;
      _contentManager = contentManager;
      _culturePickerSettingsService = culturePickerSettingsService;
    }


    //Finds route part for the specified URL
    //Returns true if specified url corresponds to some content and route exists; otherwise - false

    #region ILocalizableContentService Members

    public bool TryGetRouteForUrl(string url, out AutoroutePart route) {
      //first check for route (fast, case sensitive, not precise)
      route = _contentManager.Query<AutoroutePart, AutoroutePartRecord>()
          .ForVersion(VersionOptions.Published)
          .Where(r => r.DisplayAlias == url)
          .List()
          .FirstOrDefault();

      return route != null;
    }


    //Finds localized route part for the specified content and culture
    //Returns true if localized url for content and culture exists; otherwise - false
    public bool TryFindLocalizedRoute(ContentItem routableContent, string cultureName, out AutoroutePart localizedRoute) {
      if (!routableContent.Parts.Any(p => p.Is<ILocalizableAspect>())) {
        localizedRoute = null;
        return false;
      }

      //var siteCulture = _cultureManager.GetCultureByName(_cultureManager.GetSiteCulture());
      var localizations = _localizationService.GetLocalizations(routableContent, VersionOptions.Published);

      ILocalizableAspect localizationPart = null, siteCultureLocalizationPart = null;
      foreach (var l in localizations) {

        if (l.Culture != null && l.Culture.Culture == cultureName) {
          localizationPart = l;
          break;
        }
        if (l.Culture == null && siteCultureLocalizationPart == null) {
          siteCultureLocalizationPart = l;
        }
      }

      //try get localization part for default site culture
      if (localizationPart == null) {
        localizationPart = siteCultureLocalizationPart;
      }

      if (localizationPart == null) {
        localizedRoute = null;
        return false;
      }

      var localizedContentItem = localizationPart.ContentItem;
      localizedRoute = localizedContentItem.Parts.Single(p => p is AutoroutePart).As<AutoroutePart>();
      return true;
    }

    #endregion


    /// <summary>
    /// Returns a list of cultures that have a translation of url
    /// </summary>
    /// <param name="url">the url to discover for translated cultures</param>
    /// <returns>IList of Rich Cultures defined in Culture picker settings</returns>
    public IList<ExtendedCultureRecord> AvailableTranslations(string url, bool isHomePage=false) {
      var cultureList = new List<ExtendedCultureRecord>();
      if (isHomePage || !String.IsNullOrEmpty(url)) {
        AutoroutePart currentRoutePart;
        TryGetRouteForUrl(url, out currentRoutePart);
        if (currentRoutePart != null && currentRoutePart.As<LocalizationPart>() != null) {
          try {
            var localizations = _localizationService.GetLocalizations(currentRoutePart, VersionOptions.Published);
            cultureList = _culturePickerSettingsService
              .CultureList(localizations.Select(loc => (loc.Culture == null) ? "" : loc.Culture.Culture))
              .Where(cl => cl.CultureCode.ToString(CultureInfo.InvariantCulture) != "").Distinct().ToList();
          } catch { }
        }
      }
      return (cultureList);
    }


  }
}