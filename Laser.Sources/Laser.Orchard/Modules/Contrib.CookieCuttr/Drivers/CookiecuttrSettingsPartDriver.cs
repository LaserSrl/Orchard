using Contrib.CookieCuttr.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Environment.Extensions;
using System;

namespace Contrib.CookieCuttr.Drivers
{
    public class CookiecuttrSettingsPartDriver : ContentPartDriver<CookiecuttrSettingsPart>
    {
        public CookiecuttrSettingsPartDriver()
        {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        protected override string Prefix { get { return "CookiecuttrSettings"; } }
        protected override DriverResult Editor(CookiecuttrSettingsPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(CookiecuttrSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape("Parts_Cookiecuttr_Settings", () =>
            {
                if (updater != null)
                {
                    updater.TryUpdateModel(part.Record, Prefix, null, null);
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts.Cookiecuttr.Settings", Model: part.Record, Prefix: Prefix);
            })
                .OnGroup("cookies");
        }

        protected override void Exporting(CookiecuttrSettingsPart part, ExportContentContext context)
        {
            var element = context.Element(part.PartDefinition.Name);

            element.SetAttributeValue("cookieDiscreetPosition", part.cookieDiscreetPosition);
            element.SetAttributeValue("cookieDomain", part.cookieDomain);
            element.SetAttributeValue("cookieDiscreetLink", part.cookieDiscreetLink);
            element.SetAttributeValue("cookieDiscreetReset", part.cookieDiscreetReset);
            element.SetAttributeValue("cookieDisable", part.cookieDisable);
            element.SetAttributeValue("cookieAnalytics", part.cookieAnalytics);
            element.SetAttributeValue("cookieNotificationLocationBottom", part.cookieNotificationLocationBottom);
            element.SetAttributeValue("showCookieDeclineButton", part.showCookieDeclineButton);
            element.SetAttributeValue("showCookieAcceptButton", part.showCookieAcceptButton);
            element.SetAttributeValue("showCookieResetButton", part.showCookieResetButton);
            element.SetAttributeValue("cookieOverlayEnabled", part.cookieOverlayEnabled);
            element.SetAttributeValue("cookieCutter", part.cookieCutter);
        }

        protected override void Importing(CookiecuttrSettingsPart part, ImportContentContext context)
        {
            var partName = part.PartDefinition.Name;

            part.Record.cookieDiscreetPosition = GetAttribute<string>(context, partName, "cookieDiscreetPosition");
            part.Record.cookieDomain = GetAttribute<string>(context, partName, "cookieDomain");
            part.Record.cookieDisable = GetAttribute<string>(context, partName, "cookieDisable");
            part.Record.cookieDiscreetLink = GetAttribute<bool>(context, partName, "cookieDiscreetLink");
            part.Record.cookieDiscreetReset = GetAttribute<bool>(context, partName, "cookieDiscreetReset");
            part.Record.cookiePolicyPage = GetAttribute<bool>(context, partName, "cookiePolicyPage");
            part.Record.cookieAnalytics = GetAttribute<bool>(context, partName, "cookieAnalytics");
            part.Record.cookieNotificationLocationBottom = GetAttribute<bool>(context, partName, "cookieNotificationLocationBottom");
            part.Record.showCookieDeclineButton = GetAttribute<bool>(context, partName, "showCookieDeclineButton");
            part.Record.showCookieAcceptButton = GetAttribute<bool>(context, partName, "showCookieAcceptButton");
            part.Record.showCookieResetButton = GetAttribute<bool>(context, partName, "showCookieResetButton");
            part.Record.cookieOverlayEnabled = GetAttribute<bool>(context, partName, "cookieOverlayEnabled");
            part.Record.cookieCutter = GetAttribute<bool>(context, partName, "cookieCutter");
        }

        private TV GetAttribute<TV>(ImportContentContext context, string partName, string elementName)
        {
            string value = context.Attribute(partName, elementName);
            if (value != null)
            {
                return (TV)Convert.ChangeType(value, typeof(TV));
            }
            return default(TV);
        }
    }
}