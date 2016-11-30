
using System.Linq;
using Orchard.ContentManagement.Drivers;
using Laser.Orchard.CulturePicker.Models;
using Orchard.Localization.Services;
using Laser.Orchard.CulturePicker.Services;
using Orchard.Environment.Configuration;
using System;
using System.Web;
using Orchard;

namespace Laser.Orchard.CulturePicker.Drivers {
    
    public class CulturePickerPartDriver : ContentPartDriver<CulturePickerPart> {
        private readonly ICultureManager _cultureManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Services.ICulturePickerSettingsService _extendedCultureService;
        private readonly Services.ILocalizableContentService _localizableContentService;

        public CulturePickerPartDriver(ICultureManager cultureManager, IWorkContextAccessor workContextAccessor, Services.ICulturePickerSettingsService extendedCultureService,ILocalizableContentService localizableContentService)
        {
            _cultureManager = cultureManager;
            _workContextAccessor = workContextAccessor;
            _extendedCultureService = extendedCultureService;
            _localizableContentService = localizableContentService;
        }

        protected override DriverResult Display(CulturePickerPart part, string displayType, dynamic shapeHelper) {
            var siteAvailableCultures = _cultureManager.ListCultures().AsQueryable();
            var context = _workContextAccessor.GetContext();
            var baseUrl = context.CurrentSite.BaseUrl;
            var cleanUrl = context.HttpContext.Request.Url.AbsoluteUri.Replace(baseUrl, "");
            cleanUrl = context.HttpContext.Server.UrlDecode(cleanUrl);
            cleanUrl = cleanUrl.StartsWith("/") ? cleanUrl.Substring(1) : cleanUrl;
            // reading settings
            var settings = _extendedCultureService.ReadSettings();
            part.AvailableCultures = settings.ExtendedCulturesList;
            part.ShowOnlyPertinentCultures = settings.Settings.ShowOnlyPertinentCultures;
            part.ShowLabel = settings.Settings.ShowLabel;
            settings = null;
            var urlPrefix = _workContextAccessor.GetContext().Resolve<ShellSettings>().RequestUrlPrefix;
            if (!String.IsNullOrWhiteSpace(urlPrefix)) {
                cleanUrl = cleanUrl.StartsWith(urlPrefix, StringComparison.OrdinalIgnoreCase) ? cleanUrl.Substring(urlPrefix.Length) : cleanUrl;
            }
            cleanUrl = HttpUtility.UrlDecode(cleanUrl);
            cleanUrl = cleanUrl.StartsWith("/") ? cleanUrl.Substring(1) : cleanUrl;
            var isHomePage = String.IsNullOrWhiteSpace(cleanUrl);
            part.TranslatedCultures = _localizableContentService.AvailableTranslations(cleanUrl, isHomePage);


            part.UserCulture = _extendedCultureService.GetExtendedCulture(_cultureManager.GetCurrentCulture(_workContextAccessor.GetContext().HttpContext));

            return ContentShape("Parts_CulturePicker", () => shapeHelper.Parts_CulturePicker(AvailableCultures: part.AvailableCultures, TranslatedCultures: part.TranslatedCultures, UserCulture: part.UserCulture, ShowOnlyPertinentCultures: part.ShowOnlyPertinentCultures, ShowLabel: part.ShowLabel));
        }
    }
}