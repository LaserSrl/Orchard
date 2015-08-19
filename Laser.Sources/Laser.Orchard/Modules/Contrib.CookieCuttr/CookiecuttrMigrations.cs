using Orchard.Data.Migration;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using System;

namespace Contrib.CookieCuttr
{
    public class CookiecuttrMigrations : DataMigrationImpl
    {
        public const string cookiemsg = "We use cookies on this website, you can <a href=\"{{cookiePolicyLink}}\" title=\"read about our cookies\">read about them here</a>. To use the website as intended please...";
        public const string cookieanalyticsmsg = "We use cookies, just to track visits to our website, we store no personal details. To use the website as intended please...";
        public const string acceptmsg = "ACCEPT COOKIES";
        public const string declinemsg = "DECLINE COOKIES";
        public const string resetmsg = "RESET COOKIES FOR THIS WEBSITE";
        public const string whataremsg = "What are Cookies?";
        public const string discreetmsg = "Cookies?";
        public const string errormsg = "We're sorry, you declined the use of cookies on this website, this feature places cookies in your browser and has therefore been disabled.<br>To continue this functionality please";
        public const string whatarecookieslink = "http://www.allaboutcookies.org/";
        public int Create()
        {
            SchemaBuilder.CreateTable("CookieCuttrSettingsPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<bool>("cookieNotificationLocationBottom", c => c.WithDefault(false))       
                    .Column<bool>("cookieAnalytics", c => c.WithDefault(true))                                       
                    .Column<bool>("showCookieDeclineButton", c => c.WithDefault(false))                
                    .Column<bool>("showCookieAcceptButton", c => c.WithDefault(true))                  
                    .Column<bool>("showCookieResetButton", c => c.WithDefault(false))                  
                    .Column<bool>("cookieOverlayEnabled", c => c.WithDefault(false))                    
                    .Column<bool>("cookieCutter", c => c.WithDefault(false))                                       
                    .Column<string>("cookieDisable", c => c.WithDefault(string.Empty))                      
                    .Column<bool>("cookiePolicyPage", c => c.WithDefault(false))                        
                    .Column<bool>("cookieDiscreetLink", c => c.WithDefault(false))                  
                    .Column<bool>("cookieDiscreetReset", c => c.WithDefault(false))                       
                    .Column<string>("cookieDiscreetPosition", c => c.WithDefault("topleft"))        
                    .Column<string>("cookieDomain", c => c.WithDefault(string.Empty))               
                );

            SchemaBuilder.CreateTable("CookieCuttrPartRecord",
                table => table
                    .ContentPartRecord()
                    .Column<string>("cookieAnalyticsMessage", c => c.WithDefault(cookieanalyticsmsg))
                    .Column<string>("cookiePolicyLink", c => c.WithDefault(string.Empty))
                    .Column<string>("cookieMessage", c => c.WithDefault(cookiemsg))
                    .Column<string>("cookieWhatAreTheyLink", c => c.WithDefault(whatarecookieslink))
                    .Column<string>("cookieAcceptButtonText", c => c.WithDefault(acceptmsg))
                    .Column<string>("cookieDeclineButtonText", c => c.WithDefault(declinemsg))
                    .Column<string>("cookieResetButtonText", c => c.WithDefault(resetmsg))
                    .Column<string>("cookieWhatAreLinkText", c => c.WithDefault(whataremsg))
                    .Column<string>("cookieErrorMessage", c => c.WithDefault(errormsg))
                    .Column<string>("cookiePolicyPageMessage", c => c.WithDefault(string.Empty))
                    .Column<string>("cookieDiscreetLinkText", c => c.WithDefault(discreetmsg))
                );

            ContentDefinitionManager.AlterPartDefinition("CookiecuttrPart", part => part
                .WithDescription("Renders the CookieCuttr plugin."));

            ContentDefinitionManager.AlterTypeDefinition("CookiecuttrWidget",
                cfg => cfg
                    .WithPart("WidgetPart")
                    .WithPart("CommonPart")
                    .WithPart("IdentityPart")
                    .WithPart("CookiecuttrPart")
                    .WithSetting("Stereotype", "Widget")
                );

            return 1;
        }
    }
}