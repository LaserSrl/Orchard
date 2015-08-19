using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.CookieCuttr.Models
{
    public class CookiecuttrSettingsPartRecord : ContentPartRecord
    {
        public virtual string cookieDiscreetPosition { get; set; }
        public virtual string cookieDomain { get; set; }
        public virtual bool cookieDiscreetLink { get; set; }
        public virtual bool cookieDiscreetReset { get; set; }
        public virtual bool cookiePolicyPage { get; set; }
        public virtual string cookieDisable { get; set; }
        public virtual bool cookieAnalytics { get; set; }
        public virtual bool cookieNotificationLocationBottom { get; set; }
        public virtual bool showCookieDeclineButton { get; set; }
        public virtual bool showCookieAcceptButton { get; set; }
        public virtual bool showCookieResetButton { get; set; }
        public virtual bool cookieOverlayEnabled { get; set; }
        public virtual bool cookieCutter { get; set; }
    }
}