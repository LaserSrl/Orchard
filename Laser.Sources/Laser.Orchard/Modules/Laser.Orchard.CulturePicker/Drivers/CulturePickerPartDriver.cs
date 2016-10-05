
using System.Linq;
using Orchard.ContentManagement.Drivers;
using Laser.Orchard.CulturePicker.Models;
using Orchard.Localization.Services;
using Laser.Orchard.CulturePicker.Services;
using Orchard.Environment.Configuration;
using System;
using System.Web;
using Orchard;
using Orchard.ContentManagement.Handlers;

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



        protected override void Exporting(CulturePickerPart part, ExportContentContext context) {

           if (part.AvailableCultures != null) 
            {
                context.Element(part.PartDefinition.Name).SetAttributeValue("AvailableCultures", part.AvailableCultures);
                var avCult = context.Element(part.PartDefinition.Name).Element("AvailableCultures");
                foreach (ExtendedCultureRecord recAvCulture in part.AvailableCultures) 
                {
                    avCult.Element("Id").SetAttributeValue("Id", recAvCulture.Id);
                    avCult.Element("CultureCode").SetAttributeValue("CultureCode", recAvCulture.CultureCode);
                    avCult.Element("DisplayName").SetAttributeValue("DisplayName", recAvCulture.DisplayName);
                    avCult.Element("Priority").SetAttributeValue("Priority", recAvCulture.Priority);               
                }
            }

            if (part.TranslatedCultures != null) 
            {
                context.Element(part.PartDefinition.Name).SetAttributeValue("TranslatedCultures", part.TranslatedCultures);
                var transCult =context.Element(part.PartDefinition.Name).Element("TranslatedCultures");

                foreach (ExtendedCultureRecord recTranslCulture in part.TranslatedCultures) 
                {
                    transCult.Element("Id").SetAttributeValue("Id", recTranslCulture.Id);
                    transCult.Element("CultureCode").SetAttributeValue("CultureCode", recTranslCulture.CultureCode);
                    transCult.Element("DisplayName").SetAttributeValue("DisplayName", recTranslCulture.DisplayName);
                    transCult.Element("Priority").SetAttributeValue("Priority", recTranslCulture.Priority);
                }
            }

            context.Element(part.PartDefinition.Name).SetAttributeValue("ShowOnlyPertinentCultures", part.ShowOnlyPertinentCultures);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ShowLabel", part.ShowLabel);
                          
            if (part.UserCulture !=null)
            {
                ExtendedCultureRecord userCulture= part.UserCulture;
                context.Element(part.PartDefinition.Name).SetAttributeValue("UserCulture", part.UserCulture);
                var userCult = context.Element(part.PartDefinition.Name).Element("UserCulture");
                userCult.Element("Id").SetAttributeValue("Id", userCulture.Id);
                userCult.Element("CultureCode").SetAttributeValue("CultureCode", userCulture.CultureCode);
                userCult.Element("DisplayName").SetAttributeValue("DisplayName", userCulture.DisplayName);
                userCult.Element("Priority").SetAttributeValue("Priority", userCulture.Priority);
            }

       }




        protected override void Importing(CulturePickerPart part, ImportContentContext context) 
        {
            var root = context.Data.Element(part.PartDefinition.Name);
            var importedAvailableCultures = context.Attribute("AvailableCultures", "AvailableCultures");
           
            if (importedAvailableCultures != null) 
            {
                foreach (ExtendedCultureRecord rec in part.AvailableCultures) 
                {
                    rec.Id = int.Parse(root.Attribute("AvailableCultures").Parent.Element("Id").Value);
                    rec.CultureCode =root.Attribute("AvailableCultures").Parent.Element("CultureCode").Value;
                    rec.DisplayName = root.Attribute("AvailableCultures").Parent.Element("DisplayName").Value;
                    rec.Priority = int.Parse(root.Attribute("AvailableCultures").Parent.Element("Priority").Value);
                    part.AvailableCultures.Add(rec);
                }
            }

            var importedTranslatedCultures = context.Attribute("TranslatedCultures", "TranslatedCultures");
            if (importedTranslatedCultures != null) {
                foreach (ExtendedCultureRecord rec in part.TranslatedCultures) {
                    rec.Id = int.Parse(root.Attribute("TranslatedCultures").Parent.Element("Id").Value);
                    rec.CultureCode = root.Attribute("TranslatedCultures").Parent.Element("CultureCode").Value;
                    rec.DisplayName = root.Attribute("TranslatedCultures").Parent.Element("DisplayName").Value;
                    rec.Priority = int.Parse(root.Attribute("TranslatedCultures").Parent.Element("Priority").Value);
                    part.TranslatedCultures.Add(rec);
                }
            }

            var importedShowOnlyPertinentCultures = context.Attribute("ShowOnlyPertinentCultures", "ShowOnlyPertinentCultures");
            if (importedShowOnlyPertinentCultures != null)
            {
                part.ShowOnlyPertinentCultures = bool.Parse(root.Attribute("ShowOnlyPertinentCultures").Value);                
            }

            var importedShowLabel = context.Attribute("ShowLabel", "ShowLabel");
            if (importedShowLabel != null) {
                part.ShowLabel = bool.Parse(root.Attribute("ShowLabel").Value);
            }
     
            var importedUserCulture = context.Attribute("UserCulture", "UserCulture");            
            if (importedUserCulture != null) 
            {
                    part.UserCulture.Id= int.Parse(root.Attribute("UserCulture").Parent.Element("Id").Value);
                    part.UserCulture.CultureCode = root.Attribute("UserCulture").Parent.Element("CultureCode").Value;
                    part.UserCulture.DisplayName = root.Attribute("UserCulture").Parent.Element("DisplayName").Value;
                    part.UserCulture.Priority = int.Parse(root.Attribute("UserCulture").Parent.Element("Priority").Value);  
                }
            }

    }
}