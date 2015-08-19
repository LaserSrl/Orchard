using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Laser.Orchard.IXMSD.Models {
    public class IXMSDSettingsPart:ContentPart {
        public int DaysBeforeTodayToParseIntoLog {
            get { return this.Retrieve(x => x.DaysBeforeTodayToParseIntoLog,1); }
            set { this.Store(x => x.DaysBeforeTodayToParseIntoLog, value); }
        }
        public string LogDownloadFileFolder {
            get { return this.Retrieve(x => x.LogDownloadFileFolder,HostingEnvironment.MapPath("~/") + @"App_Data\Logs\"); }
            set { this.Store(x => x.LogDownloadFileFolder, value); }
        }
        public string LogFileName {
            get { return this.Retrieve(x => x.LogFileName,"publishing.log"); }
            set { this.Store(x => x.LogFileName, value); }
        }
        public string URL {
            get { return this.Retrieve(x => x.URL,"http://freemium.calcio.vodafone.it/VDFInforma/auto/"); }
            set { this.Store(x => x.URL, value); }
        }
        public string ftpRemoteHost {
            get { return this.Retrieve(x => x.ftpRemoteHost,"web4.laser-group.com"); }
            set { this.Store(x => x.ftpRemoteHost, value); }
        }
        public string ftpUser {
            get { return this.Retrieve(x => x.ftpUser,"ftp.costa"); }
            set { this.Store(x => x.ftpUser, value); }
        }
        public string ftpPwd {
            get { return this.Retrieve(x => x.ftpPwd,"c0st4"); }
            set { this.Store(x => x.ftpPwd, value); }
        }
    }
}