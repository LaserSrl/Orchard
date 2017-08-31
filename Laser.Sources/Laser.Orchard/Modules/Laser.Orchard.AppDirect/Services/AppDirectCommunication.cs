using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using Laser.Orchard.AppDirect.Models;
using Laser.Orchard.AppDirect.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;

namespace Laser.Orchard.AppDirect.Services {
    public interface IAppDirectCommunication : IDependency {
        bool MakeRequestToAppdirect(string uri, Method Method, string postdata, string key, out string outresponse, string token = "", string tokenSecret = "");
        void WriteEvent(EventType type, string log);
        //  bool MakePostToAppdirect(string uri, Method Method, string postData, out string outresponse, string token = "", string tokenSecret = "");
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
        private string UpperCaseUrlEncode(string s) {
            char[] temp = HttpUtility.UrlEncode(s).ToCharArray();
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
                OAuthBase oauthBase = new OAuthBase();
                if (string.IsNullOrEmpty(uri))
                    return false;
                // var setting = _orchardServices.WorkContext.CurrentSite.As<AppDirectSettingsPart>();
                var setting_oAuth = Get_oAuthCredential(key);
                if (setting_oAuth == null) {
                    WriteEvent(EventType.Output, string.Format("oAuth key not found -> {0}", key));
                    return false;
                }
                string consumerKey = setting_oAuth.ConsumerKey;
                string consumerSecret = setting_oAuth.ConsumerSecret;
                //  string consumerKey = ConsumerKey;
                // string consumerSecret = ConsumerSecret;
                string timeStamp = oauthBase.GenerateTimeStamp();
                string nonce = oauthBase.GenerateNonce();
                string normalizedUrl;
                string normalizedRequestParameters;
                var st = oauthBase.GenerateSignature(new Uri(uri), consumerKey, consumerSecret, token, tokenSecret, Method.ToString(), timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters);
                string str1 = UpperCaseUrlEncode(st);
                // string str1 = HttpUtility.UrlEncode(oauthBase.GenerateSignature(new Uri(uri), consumerKey, consumerSecret, token, tokenSecret, Method.ToString(), timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters));
                //  str1 = UpperCaseUrlEncode(str1);
                string str2 = string.Format("{0}?{1}&oauth_signature={2}", (object)normalizedUrl, (object)normalizedRequestParameters, (object)str1);
                WriteEvent(EventType.Output, str2);
                HttpWebRequest httpWebRequest;
                httpWebRequest = (HttpWebRequest)WebRequest.Create(str2);

                if (Method == Method.POST) {
                    httpWebRequest.ContentType = "application/json";
                    //   httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                    string myheader = String.Format("OAuth oauth_consumer_key=\"{0}\",oauth_nonce=\"{1}\",oauth_signature=\"{2}\",oauth_signature_method=\"HMAC-SHA1\",oauth_timestamp=\"{3}\",oauth_version=\"1.0\"", consumerKey, nonce, str1, timeStamp);
                    httpWebRequest.Headers[HttpRequestHeader.Authorization] = myheader;
                    httpWebRequest.KeepAlive = true;
                    // httpWebRequest.Accept = "application/json";
                }
                else {

                    httpWebRequest.Accept = "application/json";
                }
                httpWebRequest.Method = Method.ToString();

                if ((Method == Method.POST) && !(string.IsNullOrEmpty(postData))) {
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {
                        // string json = "{\"success\":\"true\"," +
                        //               "\"password\":\"bla\"}";
                        // string json = "{\"success\":\"true\"}";
                        // streamWriter.Write(json);
                        streamWriter.Write(postData);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    //var data = new ASCIIEncoding().GetBytes(postData);
                    //// var data = new Encoding.ASCII.GetBytes(postData);
                    //httpWebRequest.ContentLength = postData.Length;


                    //using (var stream = httpWebRequest.GetRequestStream()) {
                    //    stream.Write(data, 0, data.Length);
                    //}
                }

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

        //     public  bool MakePostToAppdirect(string uri, Method Method, string postData, out string outresponse, string token = "", string tokenSecret = "") {
        //    outresponse = "";
        //    try {
        //        var setting = _orchardServices.WorkContext.CurrentSite.As<AppDirectSettingsPart>();
        //        string ConsumerKey = setting.ConsumerKey;
        //        string ConsumerSecret = setting.ConsumerSecret;
        //        OAuthBase oauthBase = new OAuthBase();
        //        string timeStamp = oauthBase.GenerateTimeStamp();
        //        string nonce = oauthBase.GenerateNonce();
        //       // oauth.GenerateSignature(rq, callback, consumerKey, consumerSecret, null, null, "POST", timestamp, null, nonce, out url, out url2);
        //        if (string.IsNullOrEmpty(uri))
        //            return false;
        //        using (var httpClient = new System.Net.Http.HttpClient()) {

        //            string authorizationHeaderParams = String.Empty;
        //            //  authorizationHeaderParams += "OAuth ";
        //            authorizationHeaderParams += "oauth_nonce=" + "\"" + Uri.EscapeDataString(nonce) + "\",";
        //            authorizationHeaderParams += "oauth_signature_method=" + "\"" + Uri.EscapeDataString("HMAC-SHA1") + "\",";
        //            authorizationHeaderParams += "oauth_timestamp=" + "\"" + Uri.EscapeDataString(timeStamp) + "\",";
        //            authorizationHeaderParams += "oauth_consumer_key=" + "\"" + Uri.EscapeDataString(ConsumerKey) + "\",";
        //           // authorizationHeaderParams += "oauth_token=" + "\"" + Uri.EscapeDataString(accessToken.Text) + "\",";
        //            authorizationHeaderParams += "oauth_signature=" + "\"" + Uri.EscapeDataString(signature) + "\",";
        //            authorizationHeaderParams += "oauth_version=" + "\"" + Uri.EscapeDataString("1.0") + "\"";

        //            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("OAuth", authorizationHeaderParams);
        //            System.Net.Http.HttpContent content = new System.Net.Http.StringContent(@"{ ""success"": """ + "true" + @"""}");
        //            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        //            var resp = httpClient.PostAsync(new Uri(uri, UriKind.Absolute), content);
        //        }
        //    }catch(Exception ex) {
        //        string a = "A";
        //    }

        //    return true;


        //    //    outresponse = "";
        //    //    try {
        //    //        //  OAuthBase oauthBase = new OAuthBase();
        //    //        if (string.IsNullOrEmpty(uri))
        //    //            return false;
        //    //        //var setting = _orchardServices.WorkContext.CurrentSite.As<AppDirectSettingsPart>();
        //    //        //string ConsumerKey = setting.ConsumerKey;
        //    //        //string ConsumerSecret = setting.ConsumerSecret;
        //    //        //string consumerKey = ConsumerKey;
        //    //        //string consumerSecret = ConsumerSecret;
        //    //        //string timeStamp = oauthBase.GenerateTimeStamp();
        //    //        //string nonce = oauthBase.GenerateNonce();
        //    //        //string normalizedUrl;
        //    //        //string normalizedRequestParameters;
        //    //        //string str1 = HttpUtility.UrlEncode(oauthBase.GenerateSignature(new Uri(uri), consumerKey, consumerSecret, token, tokenSecret, Method.ToString(), timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters));
        //    //        //string str2 = string.Format("{0}?{1}&oauth_signature={2}", (object)normalizedUrl, (object)normalizedRequestParameters, (object)str1);
        //    //        //WriteEvent(EventType.Output, str2);
        //    //        string str2 = uri;
        //    //        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(str2);
        //    //        //if (Method == Method.POST)
        //    //        httpWebRequest.ContentType = "application/x-www-form-urlencoded";
        //    //        // else
        //    //        httpWebRequest.Accept = "application/json";
        //    //        httpWebRequest.Method = Method.ToString();
        //    //        if (!(string.IsNullOrEmpty(postData))) {
        //    //            var data = Encoding.ASCII.GetBytes(postData);
        //    //            httpWebRequest.ContentLength = postData.Length;
        //    //            //var postData = "thing1=hello";
        //    //            //postData += "&thing2=world";

        //    //            using (var stream = httpWebRequest.GetRequestStream()) {
        //    //                stream.Write(data, 0, data.Length);
        //    //            }
        //    //        }
        //    //        string end = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()).ReadToEnd();
        //    //        WriteEvent(EventType.Input, end);
        //    //        outresponse = end;
        //    //        return true;
        //    //    }
        //    //    catch (Exception ex) {
        //    //        Logger.Error(ex.Message);
        //    //        return false;
        //    //    }
        //    //}
        //}

        public void WriteEvent(EventType type, string log) {
            _repositoryLog.Create(new LogEventsRecord(type, log, GetCurrentMethod()));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod() {
            return new StackTrace().GetFrame(2).GetMethod().Name;
        }
    }
}