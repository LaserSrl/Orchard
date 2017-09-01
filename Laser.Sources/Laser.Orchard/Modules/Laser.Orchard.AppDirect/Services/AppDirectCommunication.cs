using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using Laser.Orchard.AppDirect.Models;
using Laser.Orchard.AppDirect.ViewModels;
using Orchard;
using Orchard.Data;
using Orchard.Logging;

namespace Laser.Orchard.AppDirect.Services {
    public interface IAppDirectCommunication : IDependency {
        bool MakeRequestToAppdirect(string uri, Method Method, string postdata, string key, out string outresponse, string token = "", string tokenSecret = "");
        void WriteEvent(EventType type, string log);
        AppDirectSettingVM Get_oAuthCredential(string key);
    }
    public enum Method { GET, POST };
    public class AppDirectCommunication : IAppDirectCommunication {
        private readonly IOrchardServices _orchardServices;
        private readonly ILogger Logger;
        private readonly IRepository<LogEventsRecord> _repositoryLog;
        private readonly IRepository<AppDirectSettingsRecord> _repoSetting;

        public AppDirectCommunication(
            IOrchardServices orchardServices,
            IRepository<LogEventsRecord> repositoryLog,
            IRepository<AppDirectSettingsRecord> repoSetting) {
            _orchardServices = orchardServices;
            _repositoryLog = repositoryLog;
            _repoSetting = repoSetting;
            Logger = NullLogger.Instance;
        }
        private static string UpperCaseUrlEncode(string s) {
            var temp = HttpUtility.UrlEncode(s).ToCharArray();
            for (int i = 0; i < temp.Length - 2; i++) {
                if (temp[i] == '%') {
                    temp[i + 1] = char.ToUpper(temp[i + 1]);
                    temp[i + 2] = char.ToUpper(temp[i + 2]);
                }
            }
            return new string(temp);
        }

        public AppDirectSettingVM Get_oAuthCredential(string key) {
            var oAuthSetting = new AppDirectSettingVM();
            var setting_oAuth = new AppDirectSettingsRecord();
            if (string.IsNullOrEmpty(key))
                setting_oAuth = _repoSetting.Fetch(x => x.Id > 0).FirstOrDefault();
            else
                setting_oAuth = _repoSetting.Fetch(x => x.TheKey.Equals(key)).FirstOrDefault();
            if (setting_oAuth != null) {
                oAuthSetting.ConsumerKey = setting_oAuth.ConsumerKey;
                oAuthSetting.ConsumerSecret = setting_oAuth.ConsumerSecret;
            }
            return oAuthSetting;
        }
        public bool MakeRequestToAppdirect(string uri, Method Method, string postData, string key, out string outresponse, string token = "", string tokenSecret = "") {
            outresponse = "";
            try {
                var oauthBase = new OAuthBase();
                if (string.IsNullOrEmpty(uri))
                    return false;
                var setting_oAuth = Get_oAuthCredential(key);
                if (setting_oAuth == null) {
                    WriteEvent(EventType.Output, string.Format("oAuth key not found -> {0}", key));
                    return false;
                }
                var consumerKey = setting_oAuth.ConsumerKey;
                var consumerSecret = setting_oAuth.ConsumerSecret;
                var timeStamp = oauthBase.GenerateTimeStamp();
                var nonce = oauthBase.GenerateNonce();
                string normalizedUrl;
                string normalizedRequestParameters;
                var st = oauthBase.GenerateSignature(new Uri(uri), consumerKey, consumerSecret, token, tokenSecret, Method.ToString(), timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters);
                var str1 = UpperCaseUrlEncode(st);
                var str2 = string.Format("{0}?{1}&oauth_signature={2}", (object)normalizedUrl, (object)normalizedRequestParameters, (object)str1);
                WriteEvent(EventType.Output, str2);
                HttpWebRequest httpWebRequest;
                httpWebRequest = (HttpWebRequest)WebRequest.Create(str2);

                if (Method == Method.POST) {
                    httpWebRequest.ContentType = "application/json";
                    var myheader = String.Format("OAuth oauth_consumer_key=\"{0}\",oauth_nonce=\"{1}\",oauth_signature=\"{2}\",oauth_signature_method=\"HMAC-SHA1\",oauth_timestamp=\"{3}\",oauth_version=\"1.0\"", consumerKey, nonce, str1, timeStamp);
                    httpWebRequest.Headers[HttpRequestHeader.Authorization] = myheader;
                    httpWebRequest.KeepAlive = true;
                }
                else {
                    httpWebRequest.Accept = "application/json";
                }
                httpWebRequest.Method = Method.ToString();

                if ((Method == Method.POST) && !(string.IsNullOrEmpty(postData))) {
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                        streamWriter.Write(postData);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }
                using (var streamReader = new StreamReader(httpWebRequest.GetResponse().GetResponseStream())) {
                    var end = streamReader.ReadToEnd();
                    WriteEvent(EventType.Input, end);
                    outresponse = end;
                }
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
        public static string GetCurrentMethod() => new StackTrace().GetFrame(2).GetMethod().Name;
    }
}