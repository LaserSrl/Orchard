using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using Laser.Orchard.AppDirect.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;

namespace Laser.Orchard.AppDirect.Services {
    public interface IAppDirectCommunication : IDependency {
        bool MakeRequestToAppdirect(string uri, out string outresponse, string token = "", string tokenSecret = "");
        void WriteEvent(EventType type, string log);
    }
    public class AppDirectCommunication: IAppDirectCommunication {
        private readonly IOrchardServices _orchardServices;
        private readonly ILogger Logger;
        private readonly IRepository<LogEventsRecord> _repositoryLog;
        public AppDirectCommunication(IOrchardServices orchardServices,
            IRepository<LogEventsRecord> repositoryLog) {
            _orchardServices = orchardServices;
            _repositoryLog = repositoryLog;
            Logger = NullLogger.Instance;
        }

        public bool MakeRequestToAppdirect(string uri, out string outresponse, string token = "", string tokenSecret = "") {
            outresponse = "";
            try {
                OAuthBase oauthBase = new OAuthBase();
                if (string.IsNullOrEmpty(uri))
                    return false;
                var setting = _orchardServices.WorkContext.CurrentSite.As<AppDirectSettingsPart>();
                string ConsumerKey = setting.ConsumerKey;
                string ConsumerSecret = setting.ConsumerSecret;
                string consumerKey = ConsumerKey;
                string consumerSecret = ConsumerSecret;
                string timeStamp = oauthBase.GenerateTimeStamp();
                string nonce = oauthBase.GenerateNonce();
                string normalizedUrl;
                string normalizedRequestParameters;
                string str1 = HttpUtility.UrlEncode(oauthBase.GenerateSignature(new Uri(uri), consumerKey, consumerSecret, token, tokenSecret, "GET", timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters));
                string str2 = string.Format("{0}?{1}&oauth_signature={2}", (object)normalizedUrl, (object)normalizedRequestParameters, (object)str1);
                WriteEvent(EventType.Output, str2);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(str2);
                httpWebRequest.Accept = "application/json";
                string end = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()).ReadToEnd();
                WriteEvent(EventType.Input, end);
                outresponse = end;
                return true;
            }
            catch (Exception ex) {
                Logger.Error(ex.Message);
                return false;
            }
        }
        public void WriteEvent(EventType type, string log) {
            _repositoryLog.Create(new LogEventsRecord(type, log, GetCurrentMethod()));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod() {
            return new StackTrace().GetFrame(2).GetMethod().Name;
        }
    }
}