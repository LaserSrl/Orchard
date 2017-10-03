using KrakeDefaultTheme.Settings.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KrakeDefaultTheme.Settings.Handlers {
    public class ThemeSettingsHandler : ContentHandler {

        //public ThemeSettingsHandler() {
        //    T = NullLocalizer.Instance;
        //    Logger = NullLogger.Instance;

        //    Filters.Add(new ActivatingFilter<ThemeSettingsPart>("Site"));
        //    // TO be deleted
        //    // As the theme is not active in the backend - The Admin Theme is the current theme in fact - you can not use a driver to show your views
        //    // Filters.Add(new TemplateFilterForPart<ThemeSettingsPart>("ThemeSettings_Edit", "Parts/ThemeSettings"));

        //}

        //public Localizer T { get; set; }
        //// TO be deleted
        ////protected override void GetItemMetadata(GetContentItemMetadataContext context) {
        ////    if (context.ContentItem.ContentType != "Site")
        ////        return;
        ////    base.GetItemMetadata(context);
        ////    context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Krake")));
        ////}
    }
}