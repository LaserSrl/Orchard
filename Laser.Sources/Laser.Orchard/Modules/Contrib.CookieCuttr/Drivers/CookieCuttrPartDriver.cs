using System;

using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Contrib.CookieCuttr.Models;
using Orchard;

namespace Contrib.CookieCuttr.Drivers {

    
    public class CookieCuttrPartDriver : ContentPartDriver<CookiecuttrPart> {

        private readonly IWorkContextAccessor _workContextAccessor;

        public CookieCuttrPartDriver(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        protected override DriverResult Display(CookiecuttrPart part, string displayType, dynamic shapeHelper) {
            var workContext = _workContextAccessor.GetContext();
            var cookieSettings = workContext.CurrentSite.As<CookiecuttrSettingsPart>().Record;

            return ContentShape("Parts_Cookiecuttr",
                () => shapeHelper.Parts_Cookiecuttr(CookieSettings: cookieSettings, CookiecuttrPart: part));
        }

        protected override void Exporting(CookiecuttrPart part, Orchard.ContentManagement.Handlers.ExportContentContext context) {

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

        protected override void Importing(CookiecuttrPart part, Orchard.ContentManagement.Handlers.ImportContentContext context) {

            var partName = part.PartDefinition.Name;

            part.Record.cookieDiscreetLinkText = GetAttribute<string>(context, partName, "cookieDiscreetLinkText");
            part.Record.cookiePolicyPageMessage = GetAttribute<string>(context, partName, "cookiePolicyPageMessage");
            part.Record.cookieErrorMessage = GetAttribute<string>(context, partName, "cookieErrorMessage");
            part.Record.cookieAcceptButtonText = GetAttribute<string>(context, partName, "cookieAcceptButtonText");
            part.Record.cookieDeclineButtonText = GetAttribute<string>(context, partName, "cookieDeclineButtonText");
            part.Record.cookieResetButtonText = GetAttribute<string>(context, partName, "cookieResetButtonText");
            part.Record.cookieWhatAreLinkText = GetAttribute<string>(context, partName, "cookieWhatAreLinkText");
            part.Record.cookieAnalyticsMessage = GetAttribute<string>(context, partName, "cookieAnalyticsMessage");
            part.Record.cookiePolicyLink = GetAttribute<string>(context, partName, "cookiePolicyLink");
            part.Record.cookieMessage = GetAttribute<string>(context, partName, "cookieMessage");
            part.Record.cookieWhatAreTheyLink = GetAttribute<string>(context, partName, "cookieWhatAreTheyLink");
        }

        protected override DriverResult Editor(CookiecuttrPart part, dynamic shapeHelper) {

            var workContext = _workContextAccessor.GetContext();
            var cookieSettings = workContext.CurrentSite.As<CookiecuttrSettingsPart>();
            var editModel = new CookieCuttr.ViewModels.CookiecuttrEditModel
            {
                Cookiecuttr = part,
                CookiecuttrSettings = cookieSettings
            };

            return ContentShape("Parts_Cookiecuttr_Edit",
                                () => shapeHelper.EditorTemplate(
                                      TemplateName: "Parts/CookiecuttrWidgetSettings",
                                      Model: editModel,
                                      Prefix: Prefix));
        }

        protected override DriverResult Editor(CookiecuttrPart part, IUpdateModel updater, dynamic shapeHelper) {

            var workContext = _workContextAccessor.GetContext();
            var cookieSettings = workContext.CurrentSite.As<CookiecuttrSettingsPart>();
            var editModel = new CookieCuttr.ViewModels.CookiecuttrEditModel
            {
                Cookiecuttr = part,
                CookiecuttrSettings = cookieSettings
            };

            updater.TryUpdateModel(editModel, Prefix, null, null);
            return Editor(editModel.Cookiecuttr, shapeHelper);
        }

        private TV GetAttribute<TV>(ImportContentContext context, string partName, string elementName) {
            string value = context.Attribute(partName, elementName);
            if (value != null) {
                return (TV)Convert.ChangeType(value, typeof(TV));
            }
            return default(TV);
        }
    }
}