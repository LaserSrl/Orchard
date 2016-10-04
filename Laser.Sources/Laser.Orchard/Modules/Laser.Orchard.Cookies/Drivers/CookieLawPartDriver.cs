
using Laser.Orchard.Cookies.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using System;

namespace Laser.Orchard.Cookies.Drivers {

    
    public class CookieLawPartDriver : ContentPartDriver<CookieLawPart> {

        private readonly IWorkContextAccessor _workContextAccessor;

        public CookieLawPartDriver(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        protected override DriverResult Display(CookieLawPart part, string displayType, dynamic shapeHelper) {
            var workContext = _workContextAccessor.GetContext();
            var cookieSettings = workContext.CurrentSite.As<CookieSettingsPart>();

            return ContentShape("Parts_CookieLaw",
                () => shapeHelper.Parts_CookieLaw(CookieSettings: cookieSettings, CookieLawPart: part));
        }

        protected override void Exporting(CookieLawPart part, ExportContentContext context) {

            var element = context.Element(part.PartDefinition.Name);

            element.SetAttributeValue("cookieDiscreetLinkText", part.cookieDiscreetLinkText);
            element.SetAttributeValue("cookiePolicyPageMessage", part.cookiePolicyPageMessage);
            element.SetAttributeValue("cookieErrorMessage", part.cookieErrorMessage);
            element.SetAttributeValue("cookieAcceptButtonText", part.cookieAcceptButtonText);
            element.SetAttributeValue("cookieDeclineButtonText", part.cookieDeclineButtonText);
            element.SetAttributeValue("cookieResetButtonText", part.cookieResetButtonText);
            element.SetAttributeValue("cookieWhatAreLinkText", part.cookieWhatAreLinkText);
            element.SetAttributeValue("cookieAnalyticsMessage", part.cookieAnalyticsMessage);
            element.SetAttributeValue("cookiePolicyLink", part.cookiePolicyLink);
            element.SetAttributeValue("cookieMessage", part.cookieMessage);
            element.SetAttributeValue("cookieWhatAreTheyLink", part.cookieWhatAreTheyLink);
        }

        protected override void Importing(CookieLawPart part, ImportContentContext context) {

            var partName = part.PartDefinition.Name;

            part.cookieDiscreetLinkText = GetAttribute<string>(context, partName, "cookieDiscreetLinkText");
            part.cookiePolicyPageMessage = GetAttribute<string>(context, partName, "cookiePolicyPageMessage");
            part.cookieErrorMessage = GetAttribute<string>(context, partName, "cookieErrorMessage");
            part.cookieAcceptButtonText = GetAttribute<string>(context, partName, "cookieAcceptButtonText");
            part.cookieDeclineButtonText = GetAttribute<string>(context, partName, "cookieDeclineButtonText");
            part.cookieResetButtonText = GetAttribute<string>(context, partName, "cookieResetButtonText");
            part.cookieWhatAreLinkText = GetAttribute<string>(context, partName, "cookieWhatAreLinkText");
            part.cookieAnalyticsMessage = GetAttribute<string>(context, partName, "cookieAnalyticsMessage");
            part.cookiePolicyLink = GetAttribute<string>(context, partName, "cookiePolicyLink");
            part.cookieMessage = GetAttribute<string>(context, partName, "cookieMessage");
            part.cookieWhatAreTheyLink = GetAttribute<string>(context, partName, "cookieWhatAreTheyLink");
        }

        protected override DriverResult Editor(CookieLawPart part, dynamic shapeHelper) {

            var workContext = _workContextAccessor.GetContext();
            var cookieSettings = workContext.CurrentSite.As<CookieSettingsPart>();
            var editModel = new Laser.Orchard.Cookies.ViewModels.CookieLawEditModel
            {
                CookieLaw = part,
                CookieSettings = cookieSettings
            };

            return ContentShape("Parts_CookieLaw_Edit",
                                () => shapeHelper.EditorTemplate(
                                      TemplateName: "Parts/CookieLawWidgetSettings",
                                      Model: editModel,
                                      Prefix: Prefix));
        }

        protected override DriverResult Editor(CookieLawPart part, IUpdateModel updater, dynamic shapeHelper) {

            var workContext = _workContextAccessor.GetContext();
            var cookieSettings = workContext.CurrentSite.As<CookieSettingsPart>();
            var editModel = new Laser.Orchard.Cookies.ViewModels.CookieLawEditModel
            {
                CookieLaw = part,
                CookieSettings = cookieSettings
            };

            updater.TryUpdateModel(editModel, Prefix, null, null);
            return Editor(editModel.CookieLaw, shapeHelper);
        }


        //protected override void Importing(CookieLawPart part, ImportContentContext context) {
           
        //    var importedcookieDiscreetLinkText = context.Attribute(part.PartDefinition.Name, "cookieDiscreetLinkText");
        //    if (importedcookieDiscreetLinkText != null) {
        //        part.cookieDiscreetLinkText = importedcookieDiscreetLinkText;
        //    }

        //    var importedcookiePolicyPageMessage = context.Attribute(part.PartDefinition.Name, "cookiePolicyPageMessage");
        //    if (importedcookiePolicyPageMessage != null) {
        //        part.cookiePolicyPageMessage = importedcookiePolicyPageMessage;
        //    }

        //    var importedcookieErrorMessage = context.Attribute(part.PartDefinition.Name, "cookieErrorMessage");
        //    if (importedcookieErrorMessage != null) {
        //        part.cookieErrorMessage = importedcookieErrorMessage;
        //    }

        //    var importedcookieAcceptButtonText = context.Attribute(part.PartDefinition.Name, "cookieAcceptButtonText");
        //    if (importedcookieAcceptButtonText != null) {
        //        part.cookieAcceptButtonText = importedcookieAcceptButtonText;
        //    }

        //    var importedcookieDeclineButtonText = context.Attribute(part.PartDefinition.Name, "cookieDeclineButtonText");
        //    if (importedcookieDeclineButtonText != null) {
        //        part.cookieDeclineButtonText = importedcookieDeclineButtonText;
        //    }

        //    var importedcookieResetButtonText = context.Attribute(part.PartDefinition.Name, "cookieResetButtonText");
        //    if (importedcookieResetButtonText != null) {
        //        part.cookieResetButtonText = importedcookieResetButtonText;
        //    }

        //    var importedcookieWhatAreLinkText = context.Attribute(part.PartDefinition.Name, "cookieWhatAreLinkText");
        //    if (importedcookieWhatAreLinkText != null) {
        //        part.cookieWhatAreLinkText = importedcookieWhatAreLinkText;
        //    }

        //    var importedcookieAnalyticsMessage = context.Attribute(part.PartDefinition.Name, "cookieAnalyticsMessage");
        //    if (importedcookieAnalyticsMessage != null) {
        //        part.cookieAnalyticsMessage = importedcookieAnalyticsMessage;
        //    }

        //    var importedcookiePolicyLink = context.Attribute(part.PartDefinition.Name, "cookiePolicyLink");
        //    if (importedcookiePolicyLink != null) {
        //        part.cookiePolicyLink = importedcookiePolicyLink;
        //    }

        //    var importedcookieMessage = context.Attribute(part.PartDefinition.Name, "cookieMessage");
        //    if (importedcookieMessage != null) {
        //        part.cookieMessage = importedcookieMessage;
        //    }

        //    var importedcookieWhatAreTheyLink = context.Attribute(part.PartDefinition.Name, "cookieWhatAreTheyLink");
        //    if (importedcookieWhatAreTheyLink != null) {
        //        part.cookieWhatAreTheyLink = importedcookieWhatAreTheyLink;
        //    }

        //}


        //protected override void Exporting(CookieLawPart part, ExportContentContext context) {
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("cookieDiscreetLinkText", part.cookieDiscreetLinkText);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("cookiePolicyPageMessage", part.cookieDiscreetLinkText);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("cookieErrorMessage", part.cookieErrorMessage);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("cookieAcceptButtonText", part.cookieAcceptButtonText);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("cookieDeclineButtonText", part.cookieDeclineButtonText);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("cookieResetButtonText", part.cookieResetButtonText);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("cookieWhatAreLinkText", part.cookieWhatAreLinkText);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("cookieAnalyticsMessage", part.cookieAnalyticsMessage);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("cookiePolicyLink", part.cookiePolicyLink);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("cookieMessage", part.cookieMessage);
        //    context.Element(part.PartDefinition.Name).SetAttributeValue("cookieWhatAreTheyLink", part.cookieWhatAreTheyLink);
        //}




        private TV GetAttribute<TV>(ImportContentContext context, string partName, string elementName) {
            string value = context.Attribute(partName, elementName);
            if (value != null) {
                return (TV)Convert.ChangeType(value, typeof(TV));
            }
            return default(TV);
        }

       
      

    }
}