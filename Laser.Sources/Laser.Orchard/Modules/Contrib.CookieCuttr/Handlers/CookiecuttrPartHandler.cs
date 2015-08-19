using Contrib.CookieCuttr.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.CookieCuttr.Handlers {
    public class CookiecuttrPartHandler : ContentHandler {
        public CookiecuttrPartHandler(IRepository<CookiecuttrPartRecord> repository) {
            T = NullLocalizer.Instance;
            Filters.Add(StorageFilter.For(repository));

            OnInitializing<CookiecuttrPart>((context, part) => {
                part.cookieAnalyticsMessage = CookiecuttrMigrations.cookieanalyticsmsg;
                part.cookiePolicyLink = string.Empty;
                part.cookieMessage = CookiecuttrMigrations.cookiemsg;
                part.cookieWhatAreTheyLink = CookiecuttrMigrations.whatarecookieslink;
                part.cookieErrorMessage = CookiecuttrMigrations.errormsg;
                part.cookieAcceptButtonText = CookiecuttrMigrations.acceptmsg;
                part.cookieDeclineButtonText = CookiecuttrMigrations.declinemsg;
                part.cookieResetButtonText = CookiecuttrMigrations.resetmsg;
                part.cookieWhatAreLinkText = CookiecuttrMigrations.whataremsg;
                part.cookiePolicyPageMessage = string.Empty;
                part.cookieDiscreetLinkText = string.Empty;
            });
        }

        public Localizer T { get; set; }

    }
}