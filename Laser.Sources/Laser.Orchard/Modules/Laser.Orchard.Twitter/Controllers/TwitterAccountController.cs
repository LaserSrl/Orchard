//using Laser.Orchard.Twitter.Helpers;
//using Laser.Orchard.Twitter.Models;
//using Laser.Orchard.Twitter.ViewModels;
//using Laser.Orchard.OpenAuthentication.Models;
//using Laser.Orchard.OpenAuthentication.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using Twitterizer;
using Laser.Orchard.Twitter.ViewModels;
using Laser.Orchard.Twitter.Models;
using Laser.Orchard.Twitter.Helpers;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Services;

namespace Laser.Orchard.Twitter.Controllers {

    public class TwitterAccountController : Controller, IUpdateModel {
        private readonly IProviderConfigurationService _providerConfigurationService;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly string contentType = "SocialTwitterAccount";
        private readonly dynamic TestPermission = Permissions.ManageTwitterAccount;
        private readonly INotifier _notifier;
        private Localizer T { get; set; }

        public TwitterAccountController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager
                , IProviderConfigurationService providerConfigurationService
            ) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _providerConfigurationService = providerConfigurationService;
        }

        [Admin]
        public ActionResult Edit(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            object model;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                //  model = _orchardServices.ContentManager.BuildEditor(newContent);
                //   _contentManager.Create(newContent);
                model = _contentManager.BuildEditor(newContent);
            }
            else
                model = _contentManager.BuildEditor(_orchardServices.ContentManager.Get(id));
            return View((object)model);
        }

        [HttpPost, ActionName("Edit"), Admin]
        public ActionResult EditPOST(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            ContentItem content;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                _orchardServices.ContentManager.Create(newContent);
                content = newContent;
            }
            else
                content = _orchardServices.ContentManager.Get(id);
            var model = _orchardServices.ContentManager.UpdateEditor(content, this);

            if (!ModelState.IsValid) {
                foreach (string key in ModelState.Keys) {
                    if (ModelState[key].Errors.Count > 0)
                        foreach (var error in ModelState[key].Errors)
                            _notifier.Add(NotifyType.Error, T(error.ErrorMessage));
                }
                _orchardServices.TransactionManager.Cancel();
                return View(model);
            }
            _notifier.Add(NotifyType.Information, T("Twitter Account Added"));
            return RedirectToAction("Index", "TwitterAccount");
        }

        [HttpPost]
        [Admin]
        public ActionResult Remove(Int32 id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            ContentItem content = _orchardServices.ContentManager.Get(id);
            _orchardServices.ContentManager.Remove(content);

            return RedirectToAction("Index", "TwitterAccount");
        }

        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, SearchVM search, bool ShowVideo = false) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, search, ShowVideo);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, SearchVM search, bool ShowVideo = false) {
            dynamic Options = new System.Dynamic.ExpandoObject();
            Options.ShowVideo = false;
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            var expression = search.Expression;
            IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query().ForType(contentType).OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc);
            Int32 currentiduser = _orchardServices.WorkContext.CurrentUser.Id;
            IEnumerable<ContentItem> ListContent;
            bool hasAdminPermission = _orchardServices.Authorizer.Authorize(Permissions.AdminTwitterAccount);
            if (hasAdminPermission)
                ListContent = contentQuery.List();
            else
                ListContent = contentQuery.List().Where(x => x.As<TwitterAccountPart>().IdUser == currentiduser);

            if (!string.IsNullOrEmpty(search.Expression))
                ListContent = from content in ListContent
                              where
                              ((content.As<TitlePart>().Title ?? "").Contains(expression, StringComparison.InvariantCultureIgnoreCase))
                              select content;
            IEnumerable<ContentIndexVM> listVM = ListContent.Select(p => new ContentIndexVM {
                Id = p.Id,
                Title = p.As<TwitterAccountPart>().AccountType + " - "+p.As<TwitterAccountPart>().DisplayAs,// string.IsNullOrEmpty(p.As<TwitterAccountPart>().PageName) ? "User Account" : " Page -> " + p.As<TwitterAccountPart>().PageName,
                ModifiedUtc = p.As<CommonPart>().ModifiedUtc,
                UserName = p.As<CommonPart>().Owner.UserName,
                Option = new { Valid = p.As<TwitterAccountPart>().Valid, Shared = p.As<TwitterAccountPart>().Shared }
            });
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            dynamic pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(listVM.Count());
            var list = listVM.Skip(pager.GetStartIndex())
                                .Take(pager.PageSize);
            var model = new SearchIndexVM(list, search, pagerShape, Options);
            return View((object)model);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
        public class TwitAuthenticateResponse {
            public string token_type { get; set; }
            public string access_token { get; set; }
        }


        //public void PostTwitter(string message) {
        //    message = "Hello";
        //    if (!string.IsNullOrEmpty(message)) {
        //        try {
        //            var credentials = Twitter_GetCredentials();
        //            var creds = new TwitterCredentials(credentials["CONSUMER_KEY"], credentials["CONSUMER_SECRET"], credentials["ACCESS_TOKEN"], credentials["ACCESS_TOKEN_SECRET"]);
        //            var tweet = Auth.ExecuteOperationWithCredentials(creds, () => {
        //                return Tweet.PublishTweet(message);
        //            });

        //        }
        //        catch (Exception ex) {

        //        }
        //    }

        //    // return rsp;
        //}


        #region [Read from Settings]


        private Dictionary<string, string> Twitter_GetCredentials() {
            //var getpart = _orchardServices.WorkContext.CurrentSite.As<TwitterPostSettingPart>();
            Dictionary<string, string> credential = new Dictionary<string, string>();
            credential.Add("CONSUMER_KEY", "89jyVswT6yFteqkIFvsQDIkZc");
            credential.Add("CONSUMER_SECRET", "vgzceO2VMyccTBkFgsDhp8ZM98nKxz131VuGXbPWbVkOP0B2Gi");
            credential.Add("ACCESS_TOKEN", "788780550-WQHMgxTMmWBPpjAbM1kfEdCrqNqRO1uvQkxydTuC");
            credential.Add("ACCESS_TOKEN_SECRET", "4wyxQoFwICbOH9jDk3BTzj9jpZr34do6kycGVFqfa2mzU");
            return credential;

        }
        #endregion



        //private void PostMessageToTwitter(string message) {
        //    //The Twitter json url to update the status
        //    string TwitterURL = "http://api.twitter.com/1.1/statuses/update.json";

        //    //set the access tokens (REQUIRED)
        //    string oauth_consumer_key = "Enter customer key here";
        //    string oauth_consumer_secret = "Enter customer secret here";
        //    string oauth_token = "Enter access token";
        //    string oauth_token_secret = "Enter access token secret";

        //    // set the oauth version and signature method
        //    string oauth_version = "1.0";
        //    string oauth_signature_method = "HMAC-SHA1";

        //    // create unique request details
        //    string oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        //    System.TimeSpan timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        //    string oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

        //    // create oauth signature
        //    string baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" + "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&status={6}";

        //    string baseString = string.Format(
        //        baseFormat,
        //        oauth_consumer_key,
        //        oauth_nonce,
        //        oauth_signature_method,
        //        oauth_timestamp, oauth_token,
        //        oauth_version,
        //        Uri.EscapeDataString(message)
        //    );

        //    string oauth_signature = null;
        //    using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(Uri.EscapeDataString(oauth_consumer_secret) + "&" + Uri.EscapeDataString(oauth_token_secret)))) {
        //        oauth_signature = Convert.ToBase64String(hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes("POST&" + Uri.EscapeDataString(TwitterURL) + "&" + Uri.EscapeDataString(baseString))));
        //    }

        //    // create the request header
        //    string authorizationFormat = "OAuth oauth_consumer_key=\"{0}\", oauth_nonce=\"{1}\", " + "oauth_signature=\"{2}\", oauth_signature_method=\"{3}\", " + "oauth_timestamp=\"{4}\", oauth_token=\"{5}\", " + "oauth_version=\"{6}\"";

        //    string authorizationHeader = string.Format(
        //        authorizationFormat,
        //        Uri.EscapeDataString(oauth_consumer_key),
        //        Uri.EscapeDataString(oauth_nonce),
        //        Uri.EscapeDataString(oauth_signature),
        //        Uri.EscapeDataString(oauth_signature_method),
        //        Uri.EscapeDataString(oauth_timestamp),
        //        Uri.EscapeDataString(oauth_token),
        //        Uri.EscapeDataString(oauth_version)
        //    );

        //    HttpWebRequest objHttpWebRequest = (HttpWebRequest)WebRequest.Create(TwitterURL);
        //    objHttpWebRequest.Headers.Add("Authorization", authorizationHeader);
        //    objHttpWebRequest.Method = "POST";
        //    objHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
        //    using (Stream objStream = objHttpWebRequest.GetRequestStream()) {
        //        byte[] content = ASCIIEncoding.ASCII.GetBytes("status=" + Uri.EscapeDataString(message));
        //        objStream.Write(content, 0, content.Length);
        //    }

        //    var responseResult = "";
        //    try {
        //        //success posting
        //        WebResponse objWebResponse = objHttpWebRequest.GetResponse();
        //        StreamReader objStreamReader = new StreamReader(objWebResponse.GetResponseStream());
        //        responseResult = objStreamReader.ReadToEnd().ToString();
        //    }
        //    catch (Exception ex) {
        //        //throw exception error
        //        responseResult = "Twitter Post Error: " + ex.Message.ToString() + ", authHeader: " + authorizationHeader;
        //    }
        //}



        //public void PostMessageToTwitter(string message) {
        //    message = "hello";
        //    //The Twitter json url to update the status
        //    string TwitterURL = "http://api.twitter.com/1.1/statuses/update.json";

        //    //set the access tokens (REQUIRED)
        //    string oauth_consumer_key = "89jyVswT6yFteqkIFvsQDIkZc";
        //    string oauth_consumer_secret = "vgzceO2VMyccTBkFgsDhp8ZM98nKxz131VuGXbPWbVkOP0B2Gi";
        //    string oauth_token = "788780550-WQHMgxTMmWBPpjAbM1kfEdCrqNqRO1uvQkxydTuC";
        //    string oauth_token_secret = "4wyxQoFwICbOH9jDk3BTzj9jpZr34do6kycGVFqfa2mzU";

        //    // set the oauth version and signature method
        //    string oauth_version = "1.0";
        //    string oauth_signature_method = "HMAC-SHA1";

        //    // create unique request details
        //    string oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
        //    System.TimeSpan timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        //    string oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

        //    // create oauth signature
        //    string baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" + "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&status={6}";

        //    string baseString = string.Format(
        //        baseFormat,
        //        oauth_consumer_key,
        //        oauth_nonce,
        //        oauth_signature_method,
        //        oauth_timestamp, oauth_token,
        //        oauth_version,
        //        Uri.EscapeDataString(message)
        //    );

        //    string oauth_signature = null;
        //    using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(Uri.EscapeDataString(oauth_consumer_secret) + "&" + Uri.EscapeDataString(oauth_token_secret)))) {
        //        oauth_signature = Convert.ToBase64String(hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes("POST&" + Uri.EscapeDataString(TwitterURL) + "&" + Uri.EscapeDataString(baseString))));
        //    }

        //    // create the request header
        //    string authorizationFormat = "OAuth oauth_consumer_key=\"{0}\", oauth_nonce=\"{1}\", " + "oauth_signature=\"{2}\", oauth_signature_method=\"{3}\", " + "oauth_timestamp=\"{4}\", oauth_token=\"{5}\", " + "oauth_version=\"{6}\"";

        //    string authorizationHeader = string.Format(
        //        authorizationFormat,
        //        Uri.EscapeDataString(oauth_consumer_key),
        //        Uri.EscapeDataString(oauth_nonce),
        //        Uri.EscapeDataString(oauth_signature),
        //        Uri.EscapeDataString(oauth_signature_method),
        //        Uri.EscapeDataString(oauth_timestamp),
        //        Uri.EscapeDataString(oauth_token),
        //        Uri.EscapeDataString(oauth_version)
        //    );

        //    HttpWebRequest objHttpWebRequest = (HttpWebRequest)WebRequest.Create(TwitterURL);
        //    objHttpWebRequest.Headers.Add("Authorization", authorizationHeader);
        //    objHttpWebRequest.Method = "POST";
        //    objHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
        //    using (
        //        System.IO.Stream objStream = objHttpWebRequest.GetRequestStream()) {
        //        byte[] content = ASCIIEncoding.ASCII.GetBytes("status=" + Uri.EscapeDataString(message));
        //        objStream.Write(content, 0, content.Length);
        //    }

        //    var responseResult = "";
        //    try {
        //        //success posting
        //        WebResponse objWebResponse = objHttpWebRequest.GetResponse();
        //        StreamReader objStreamReader = new StreamReader(objWebResponse.GetResponseStream());
        //        responseResult = objStreamReader.ReadToEnd().ToString();
        //    }
        //    catch (Exception ex) {
        //        //throw exception error
        //        responseResult = "Twitter Post Error: " + ex.Message.ToString() + ", authHeader: " + authorizationHeader;
        //    }
        //}


        //       [Admin]
        //public ActionResult GetPostTokenTwitter() {
        //    //Microsoft.Web.WebPages.OAuth..Manager();
        //    //{
        //    //    var oauth_consumer_key = "gjxG99ZA5jmJoB3FeXWJZA";
        //    //    var oauth_consumer_secret = "rsAAtEhVRrXUTNcwEecXqPyDHaOR4KjOuMkpb8g";

        //    //    if (Request["oauth_token"] == null) {
        //    //        OAuthTokenResponse reqToken = OAuthUtility.GetRequestToken(
        //    //            oauth_consumer_key,
        //    //            oauth_consumer_secret,
        //    //            Request.Url.AbsoluteUri);

        //    //        Response.Redirect(string.Format("http://twitter.com/oauth/authorize?oauth_token={0}",
        //    //            reqToken.Token));
        //    //    }
        //    //    else {
        //    //        string requestToken = Request["oauth_token"].ToString();
        //    //        string pin = Request["oauth_verifier"].ToString();

        //    //        var tokens = OAuthUtility.GetAccessToken(
        //    //            oauth_consumer_key,
        //    //            oauth_consumer_secret,
        //    //            requestToken,
        //    //            pin);

        //    //        OAuthTokens accesstoken = new OAuthTokens() {
        //    //            AccessToken = tokens.Token,
        //    //            AccessTokenSecret = tokens.TokenSecret,
        //    //            ConsumerKey = oauth_consumer_key,
        //    //            ConsumerSecret = oauth_consumer_secret
        //    //        };

        //    //        TwitterResponse<TwitterStatus> response = TwitterStatus.Update(
        //    //            accesstoken,
        //    //            "Testing!! It works (hopefully).");

        //    //        if (response.Result == RequestResult.Success) {
        //    //            Response.Write("we did it!");
        //    //        }
        //    //        else {
        //    //            Response.Write("it's all bad.");
        //    //        }
        //    //    }
        //    //}

        //    var oAuthConsumerKey = "O0FOsc7favW3BDY01ZCjVgIBp";
        //    var oAuthConsumerSecret = "SAQudp5XOBHOuBS3BzZ9j01mJ5UO3q6BLFMEBvOBk2VZWhyWXh";
        //    var oAuthUrl = "https://api.twitter.com/oauth2/token";
        //    var screenname = "lasersrlao";

        //    // Do the Authenticate
        //    var authHeaderFormat = "Basic {0}";

        //    var authHeader = string.Format(authHeaderFormat,
        //        Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(oAuthConsumerKey) + ":" +
        //        Uri.EscapeDataString((oAuthConsumerSecret)))
        //    ));

        //    var postBody = "grant_type=client_credentials";

        //    HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(oAuthUrl);
        //    authRequest.Headers.Add("Authorization", authHeader);
        //    authRequest.Method = "POST";
        //    authRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
        //    authRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        //    using (System.IO.Stream stream = authRequest.GetRequestStream()) {
        //        byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
        //        stream.Write(content, 0, content.Length);
        //    }

        //    authRequest.Headers.Add("Accept-Encoding", "gzip");

        //    WebResponse authResponse = authRequest.GetResponse();
        //    // deserialize into an object
        //    TwitAuthenticateResponse twitAuthResponse;
        //    using (authResponse) {
        //        using (var reader = new StreamReader(authResponse.GetResponseStream())) {
        //            JavaScriptSerializer js = new JavaScriptSerializer();
        //            var objectText = reader.ReadToEnd();
        //            twitAuthResponse = JsonConvert.DeserializeObject<TwitAuthenticateResponse>(objectText);
        //        }
        //    }

        //    // Do the timeline
        //    var timelineFormat = "https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name={0}&include_rts=1&exclude_replies=1&count=5";
        //    var timelineUrl = string.Format(timelineFormat, screenname);
        //    HttpWebRequest timeLineRequest = (HttpWebRequest)WebRequest.Create(timelineUrl);
        //    var timelineHeaderFormat = "{0} {1}";
        //    timeLineRequest.Headers.Add("Authorization", string.Format(timelineHeaderFormat, twitAuthResponse.token_type, twitAuthResponse.access_token));
        //    timeLineRequest.Method = "Get";
        //    WebResponse timeLineResponse = timeLineRequest.GetResponse();
        //    var timeLineJson = string.Empty;
        //    using (timeLineResponse) {
        //        using (var reader = new StreamReader(timeLineResponse.GetResponseStream())) {
        //            timeLineJson = reader.ReadToEnd();
        //        }
        //        return null;
        //    }








        //    //HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.twitter.com/oauth/request_token");
        //    //string response = "";
        //    //HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
        //    //using (StreamReader reader = new StreamReader(resp.GetResponseStream())) {
        //    //    response = reader.ReadToEnd();
        //    //}
        //    //Process proc = new Process();
        //    //proc.StartInfo.UseShellExecute = true;
        //    //proc.StartInfo.FileName = "https://api.twitter.com/oauth/authenticate?" + response;
        //    //proc.Start();



        //    //ProviderConfigurationRecord pcr = _providerConfigurationService.Get("Twitter");
        //    //string app_id = pcr.ProviderIdKey;
        //    //string app_secret = pcr.ProviderSecret;
        //    //string scope = "publish_actions,manage_pages,publish_pages";//user_status status_updated nelle extended permission

        //    //if (Request["oauth_token"] == null) {
        //    //    string url = string.Format(
        //    //        "https://graph.Twitter.com/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}",
        //    //        app_id, Request.Url.AbsoluteUri, scope);
        //    //    Response.Redirect(url, false);
        //    //}
        //    //else {
        //    //    Dictionary<string, string> tokens = new Dictionary<string, string>();

        //    //    string url = string.Format("https://graph.Twitter.com/oauth/access_token?client_id={0}&redirect_uri={1}&scope={2}&code={3}&client_secret={4}",
        //    //        app_id, Request.Url.AbsoluteUri, scope, Request["code"].ToString(), app_secret);

        //    //    HttpWebRequest request = System.Net.WebRequest.Create(url) as HttpWebRequest;

        //    //    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) {
        //    //        StreamReader reader = new StreamReader(response.GetResponseStream());

        //    //        string vals = reader.ReadToEnd();

        //    //        foreach (string token in vals.Split('&')) {
        //    //            tokens.Add(token.Substring(0, token.IndexOf("=")),
        //    //                token.Substring(token.IndexOf("=") + 1, token.Length - token.IndexOf("=") - 1));
        //    //        }
        //    //    }

        //    //    string access_token = tokens["access_token"];

        //    //    //TwitterAccountVM fvm = new TwitterAccountVM();
        //    //    //fvm.UserToken = access_token;
        //    //    //OrchardRegister(fvm);

        //    //    //var client = new TwitterClient(access_token);

        //    //    ////  TwitterPostSettingPart getpart = _orchardServices.WorkContext.CurrentSite.As<TwitterPostSettingPart>();
        //    //    ////  getpart.TwitterAccessToken = access_token;
        //    //    //JsonObject jsonResponse = client.Get("me/accounts") as JsonObject;
        //    //    //Dictionary<string, string> ElencoPagine = new Dictionary<string, string>();
        //    //    //foreach (var account in (JsonArray)jsonResponse["data"]) {
        //    //    //    string accountName = (string)(((JsonObject)account)["name"]);
        //    //    //    fvm = new TwitterAccountVM();
        //    //    //    fvm.UserToken = access_token;
        //    //    //    fvm.PageName = accountName;
        //    //    //    fvm.UserTokenSecret = (string)(((JsonObject)account)["access_token"]);
        //    //    //    fvm.IdPage = (string)(((JsonObject)account)["id"]);
        //    //    //    OrchardRegister(fvm);
        //    //    //}
        //    //    return RedirectToAction("Index", "TwitterAccount", new { area = "Laser.Orchard.Twitter", id = -10 });
        //    //}
        //    //return null;
        //    //    }

        //    //private void OrchardRegister(TwitterAccountVM fvm) {
        //    //    IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query().ForType(contentType);
        //    //    Int32 currentiduser = _orchardServices.WorkContext.CurrentUser.Id;
        //    //    fvm.IdPage = fvm.IdPage ?? "";
        //    //    Int32 elementi = contentQuery.List().Where(x => x.As<TwitterAccountPart>().IdUser == currentiduser && (x.As<TwitterAccountPart>().IdPage == fvm.IdPage)).Count();
        //    //    if (elementi > 0) {
        //    //        if (string.IsNullOrEmpty(fvm.IdPage)) {
        //    //            _notifier.Add(NotifyType.Warning, T("User Twitter Account can't be added, is duplicated"));
        //    //        }
        //    //        else {
        //    //            _notifier.Add(NotifyType.Warning, T("Twitter Page {0} can't be added, is duplicated", fvm.PageName));
        //    //        }
        //    //    }
        //    //    else {
        //    //        string displayas = "";
        //    //        if (string.IsNullOrEmpty(fvm.PageName)) {
        //    //            string json = new WebClient().DownloadString("https://graph.Twitter.com/me?access_token=" + fvm.UserToken);
        //    //            displayas = "User - "+ (JObject.Parse(json))["name"].ToString();
        //    //        }
        //    //        else
        //    //            displayas ="Page - "+ fvm.PageName;


        //    //        var newContent = _orchardServices.ContentManager.New(contentType);
        //    //        _orchardServices.ContentManager.Create(newContent);
        //    //        newContent.As<TwitterAccountPart>().IdUser = currentiduser;
        //    //        newContent.As<TwitterAccountPart>().DisplayAs = displayas;
        //    //        newContent.As<TwitterAccountPart>().SocialName = "Twitter";
        //    //        newContent.As<TwitterAccountPart>().UserToken = fvm.UserToken;
        //    //        newContent.As<TwitterAccountPart>().Valid = false;
        //    //        newContent.As<TwitterAccountPart>().PageName = fvm.PageName;
        //    //        newContent.As<TwitterAccountPart>().UserTokenSecret = fvm.UserTokenSecret;
        //    //        newContent.As<TwitterAccountPart>().IdPage = fvm.IdPage ?? "";
        //    //        if (string.IsNullOrEmpty(fvm.IdPage)) {
        //    //            _notifier.Add(NotifyType.Warning, T("User Twitter Account added"));
        //    //        }
        //    //        else {
        //    //            _notifier.Add(NotifyType.Warning, T("Twitter Page {0} added", fvm.PageName));
        //    //        }
        //    //    }
        //    //}
        //}
        #region twitterizer
        public ActionResult GetPostTokenTwitter() {
            ProviderConfigurationRecord pcr = _providerConfigurationService.Get("Twitter");
            if (pcr==null) {
                _notifier.Add(NotifyType.Error, T("No twitter account setting added, add one in Settings -> Open Authentication"));
                return RedirectToAction("Index", "TwitterAccount", new { area = "Laser.Orchard.Twitter", id = -10 });
        
            }

            string consumerKey = pcr.ProviderIdKey;
            string consumerSecret = pcr.ProviderSecret;
            // il meccanismo utilizzato è il 3-Legged oAuth
            if (Request["oauth_token"] == null) {
                string tmpreq = Request.Url.AbsoluteUri;
                //  tmpreq = "http://185.11.22.191:1235/Laser.Orchard/adv/Laser.Orchard.Twitter/twitteraccount/GetPostTokenTwitter";
                OAuthTokenResponse reqToken = OAuthUtility.GetRequestToken(consumerKey, consumerSecret, tmpreq);
                Response.Redirect(string.Format("http://twitter.com/oauth/authorize?oauth_token={0}", reqToken.Token));
            }
            else {
                string requestToken = Request["oauth_token"].ToString();
                string verifier = Request["oauth_verifier"].ToString();
                var tokens = OAuthUtility.GetAccessToken(consumerKey, consumerSecret, requestToken, verifier);
               TwitterAccountVM vm=new TwitterAccountVM();
               vm.DisplayAs = tokens.ScreenName;
               vm.UserToken = tokens.Token;
               vm.UserTokenSecret = tokens.TokenSecret; // conterrà l'account_token_secret
               OrchardRegister(vm);
                
               
            }
            return RedirectToAction("Index", "TwitterAccount", new { area = "Laser.Orchard.Twitter", id = -10 });
      
        }

        private void OrchardRegister(TwitterAccountVM fvm) {
            IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query().ForType(contentType);
            Int32 currentiduser = _orchardServices.WorkContext.CurrentUser.Id;
           
            Int32 elementi = contentQuery.List().Where(x => x.As<TwitterAccountPart>().IdUser == currentiduser ).Count();
            if (elementi > 0) {
                    _notifier.Add(NotifyType.Warning, T("User Twitter Account can't be added, is duplicated"));
            }
            else {
                var newContent = _orchardServices.ContentManager.New(contentType);
                _orchardServices.ContentManager.Create(newContent);
                newContent.As<TwitterAccountPart>().AccountType = "User";
                newContent.As<TwitterAccountPart>().IdUser = currentiduser;
                newContent.As<TwitterAccountPart>().DisplayAs = fvm.DisplayAs;
                newContent.As<TwitterAccountPart>().SocialName = "Twitter";
                newContent.As<TwitterAccountPart>().UserToken = fvm.UserToken;
                newContent.As<TwitterAccountPart>().Valid = true;
                newContent.As<TwitterAccountPart>().Shared = false;
                newContent.As<TwitterAccountPart>().UserTokenSecret = fvm.UserTokenSecret;
                _notifier.Add(NotifyType.Warning, T("User Twitter Account added"));
            }
        }
        #endregion
    }
}