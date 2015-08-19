using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.CookieCuttr.Models
{
    public class CookiecuttrSettingsPart : ContentPart<CookiecuttrSettingsPartRecord>
    {
        public string cookieDiscreetPosition { get { return Record.cookieDiscreetPosition; } set { Record.cookieDiscreetPosition = value; } }
        public string cookieDomain { get { return Record.cookieDomain; } set { Record.cookieDomain = value; } }
        public bool cookieDiscreetLink { get { return Record.cookieDiscreetLink; } set { Record.cookieDiscreetLink = value; } }
        public bool cookieDiscreetReset { get { return Record.cookieDiscreetReset; } set { Record.cookieDiscreetReset = value; } }
        public bool cookiePolicyPage { get { return Record.cookiePolicyPage; } set { Record.cookiePolicyPage = value; } }
        public string cookieDisable { get { return Record.cookieDisable; } set { Record.cookieDisable = value; } }
        public bool cookieAnalytics { get { return Record.cookieAnalytics; } set { Record.cookieAnalytics = value; } }
        public bool cookieNotificationLocationBottom { get { return Record.cookieNotificationLocationBottom; } set { Record.cookieNotificationLocationBottom = value; } }
        public bool showCookieDeclineButton { get { return Record.showCookieDeclineButton; } set { Record.showCookieDeclineButton = value; } }
        public bool showCookieAcceptButton { get { return Record.showCookieAcceptButton; } set { Record.showCookieAcceptButton = value; } }
        public bool showCookieResetButton { get { return Record.showCookieResetButton; } set { Record.showCookieResetButton = value; } }
        public bool cookieOverlayEnabled { get { return Record.cookieOverlayEnabled; } set { Record.cookieOverlayEnabled = value; } }
        public bool cookieCutter { get { return Record.cookieCutter; } set { Record.cookieCutter = value; } }
    }
}