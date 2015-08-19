using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.CookieCuttr.Models
{

    public class CookiecuttrPartRecord : ContentPartRecord
    {
        public virtual string cookieDiscreetLinkText { get; set; }
        public virtual string cookiePolicyPageMessage { get; set; }
        public virtual string cookieErrorMessage { get; set; }
        public virtual string cookieAcceptButtonText { get; set; }
        public virtual string cookieDeclineButtonText { get; set; }
        public virtual string cookieResetButtonText { get; set; }
        public virtual string cookieWhatAreLinkText { get; set; }
        public virtual string cookieAnalyticsMessage { get; set; }
        public virtual string cookiePolicyLink { get; set; }
        public virtual string cookieMessage { get; set; }
        public virtual string cookieWhatAreTheyLink { get; set; }
    }

    public class CookiecuttrPart : ContentPart<CookiecuttrPartRecord>
    {
        public string cookieDiscreetLinkText { get { return Record.cookieDiscreetLinkText; } set { Record.cookieDiscreetLinkText = value; } }
        public string cookiePolicyPageMessage { get { return Record.cookiePolicyPageMessage; } set { Record.cookiePolicyPageMessage = value; } }
        public string cookieErrorMessage { get { return Record.cookieErrorMessage; } set { Record.cookieErrorMessage = value; } }
        public string cookieAcceptButtonText { get { return Record.cookieAcceptButtonText; } set { Record.cookieAcceptButtonText = value; } }
        public string cookieDeclineButtonText { get { return Record.cookieDeclineButtonText; } set { Record.cookieDeclineButtonText = value; } }
        public string cookieResetButtonText { get { return Record.cookieResetButtonText; } set { Record.cookieResetButtonText = value; } }
        public string cookieWhatAreLinkText { get { return Record.cookieWhatAreLinkText; } set { Record.cookieWhatAreLinkText = value; } }
        public string cookieAnalyticsMessage { get { return Record.cookieAnalyticsMessage; } set { Record.cookieAnalyticsMessage = value; } }
        public string cookiePolicyLink { get { return Record.cookiePolicyLink; } set { Record.cookiePolicyLink = value; } }
        public string cookieMessage { get { return Record.cookieMessage; } set { Record.cookieMessage = value; } }
        public string cookieWhatAreTheyLink { get { return Record.cookieWhatAreTheyLink; } set { Record.cookieWhatAreTheyLink = value; } }
    }
}