using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.AppDirect.Models;
using Laser.Orchard.AppDirect.Services;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Workflows.Services;

namespace Laser.Orchard.AppDirect.Controllers {
    public class SubscriptionController : Controller {
        private readonly IMembershipService _membershipService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly ILogger Logger;
        private readonly IContentManager _contentManager;
        private readonly IWorkflowManager _workflowManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IAppDirectCommunication _appDirectCommunication;
        

        public Localizer T { get; set; }

        public SubscriptionController(
            IMembershipService membershipService, 
            IAuthenticationService authenticationService, 
            IUserEventHandler userEventHandler, 
            IContentManager contentManager,
            IWorkflowManager workflowManager,
            IOrchardServices orchardServices,
            IAppDirectCommunication appDirectCommunication) {
            _appDirectCommunication = appDirectCommunication;
            _orchardServices = orchardServices;
            _workflowManager = workflowManager;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _userEventHandler = userEventHandler;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            _contentManager = contentManager;
        }

        //protected string NormalizeParameters(SortedDictionary<string, string> parameters) {
        //    StringBuilder stringBuilder = new StringBuilder();
        //    int num = 0;
        //    foreach (KeyValuePair<string, string> parameter in parameters) {
        //        if (num > 0)
        //            stringBuilder.Append("&");
        //        stringBuilder.AppendFormat("{0}={1}", (object)parameter.Key, (object)parameter.Value);
        //        ++num;
        //    }
        //    return stringBuilder.ToString();
        //}

        //private string GenerateBase(string nonce, string timeStamp, Uri url) {
        //    SortedDictionary<string, string> parameters = new SortedDictionary<string, string>() { { "oauth_consumer_key", ConsumerKey }, { "oauth_signature_method", "HMAC-SHA1" }, { "oauth_timestamp", timeStamp }, { "oauth_nonce", nonce }, { "oauth_version", "1.0" } };
        //    StringBuilder stringBuilder = new StringBuilder();
        //    stringBuilder.Append("GET");
        //    stringBuilder.Append("&" + Uri.EscapeDataString(url.AbsoluteUri));
        //    stringBuilder.Append("&" + Uri.EscapeDataString(NormalizeParameters(parameters)));
        //    return stringBuilder.ToString();
        //}

        //public string GenerateSignature(string nonce, string timeStamp, Uri url) {
        //    return Convert.ToBase64String(new HMACSHA1(Encoding.ASCII.GetBytes(string.Format("{0}&{1}",  ConsumerSecret, (object)""))).ComputeHash(new ASCIIEncoding().GetBytes(GenerateBase(nonce, timeStamp, url))));
        //}


        private string GetAuthorizationHeaderValue(string Authorization,string key) {
            var value = "";
            var pieces=   Authorization.Split(new[] { " oauth_" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string piece in pieces) {
                if (piece.StartsWith(key + "=")) {
                    value= piece.Substring(key.Length + 2, piece.LastIndexOf("\"") - (key.Length + 2));
                    break;
                }
            }
         return value;
        }

        private bool VerifyValidRequest() {       
            string authorization = (Request.Headers["Authorization"]?? "").ToString();
            if (string.IsNullOrEmpty(authorization)) {
                Logger.Error(T("Authorization Header is empty").ToString());
                return false;
            }
            var setting=_orchardServices.WorkContext.CurrentSite.As<AppDirectSettingsPart>();
            string ConsumerKey = setting.ConsumerKey;
            string ConsumerSecret = setting.ConsumerSecret;
            var oauth_consumer_key = GetAuthorizationHeaderValue(authorization, "consumer_key");
            var oauth_nonce = GetAuthorizationHeaderValue(authorization, "nonce");
            var oauth_signature = GetAuthorizationHeaderValue(authorization, "signature");
            var oauth_timestamp = GetAuthorizationHeaderValue(authorization, "timestamp");
            if (oauth_consumer_key != ConsumerKey) {
                Logger.Error(T("Authorization Header {0} have incorrect ConsumerKey", authorization).ToString());
                return false;
            }
            var oauthBase = new OAuthBase();
            var normalizedUrl = "";
            var normalizedRequestParameters = "";
            var uri = Request.Url;
            // if i use ip then i'm debugging on my pc and i correct uri with the port redirect
            if (uri.ToString().Contains("185.11.22.191")) {
                var uriBuilder = new UriBuilder(Request.Url);
                uriBuilder.Port = 1235;
                uri = uriBuilder.Uri;
            }
            var oauth_signature_Calculated= oauthBase.GenerateSignature(uri, ConsumerKey, ConsumerSecret, "", "", "GET", oauth_timestamp, oauth_nonce, out normalizedUrl, out normalizedRequestParameters);
            // faccio HttpUtility.decode e non encodo quello calcolato perchè HttpUtility.Encode uso uno standart lowercase mentre java usa lo standard uppercase e quindi le stringhe sarebbero diverse
            var oauth_signature_decoded = HttpUtility.UrlDecode(oauth_signature);
            if (oauth_signature_decoded.Equals(oauth_signature_Calculated))
                return true;
            else
                Logger.Error(T("Authorization Header {0} have incorrect oauth_signature", authorization).ToString());
            return false;
        }



        //private bool MakeRequestToAppdirect(string uri, out string outresponse, string token = "", string tokenSecret = "") {
        //    outresponse = "";
        //    try {
        //        OAuthBase oauthBase = new OAuthBase();
        //        if (string.IsNullOrEmpty(uri))
        //            return false;
        //        var setting = _orchardServices.WorkContext.CurrentSite.As<AppDirectSettingsPart>();
        //        string ConsumerKey = setting.ConsumerKey;
        //        string ConsumerSecret = setting.ConsumerSecret;
        //        string consumerKey = ConsumerKey;
        //        string consumerSecret = ConsumerSecret;
        //        string timeStamp = oauthBase.GenerateTimeStamp();
        //        string nonce = oauthBase.GenerateNonce();
        //        string normalizedUrl;
        //        string normalizedRequestParameters;
        //        string str1 = HttpUtility.UrlEncode(oauthBase.GenerateSignature(new Uri(uri), consumerKey, consumerSecret, token, tokenSecret, "GET", timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters));
        //        string str2 = string.Format("{0}?{1}&oauth_signature={2}", (object)normalizedUrl, (object)normalizedRequestParameters, (object)str1);
        //        WriteEvent(EventType.Output, str2);
        //        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(str2);
        //        httpWebRequest.Accept = "application/json";
        //        string end = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()).ReadToEnd();
        //        WriteEvent(EventType.Input, end);
        //        outresponse = end;
        //        return true;
        //    }
        //    catch (Exception ex) {
        //        Logger.Error(ex.Message);
        //        return false;
        //    }
        //}

        //private bool Login(string userName) {
        //    if (string.IsNullOrWhiteSpace(userName))
        //        return false;
        //    IUser user = _membershipService.GetUser(userName);
        //    if (user != null)
        //        _authenticationService.SignIn(user, true);
        //    IUser authenticatedUser = _authenticationService.GetAuthenticatedUser();
        //    if (authenticatedUser == null)
        //        return false;
        //    _userEventHandler.LoggedIn(authenticatedUser);
        //    return true;
        //}

        //private bool CreateUserOrchard(string username, string email) {
        //    try {
        //        string password = Membership.GeneratePassword(10, 5);
        //        if (_membershipService.CreateUser(new CreateUserParams(username, password, email, T.Invoke("Auto Registered User", new object[0]).Text, password, true)) != null)
        //            return true;
        //        Logger.Error( string.Format("AppDirect => Error Creating user username={0} email={1}", (object)username, (object)email));
        //        return false;
        //    }
        //    catch (Exception ex) {
        //        Logger.Error(string.Format("AppDirect => Error Creating user username={0} email={1}", (object)username, (object)email) + " " + ex.Message);
        //        return false;
        //    }
        //}

        //private bool CreateOrLoginUser(JObject json) {
        //    if (json["creator"] == null)
        //        return false;
        //    string email = (json["creator"]["email"] ?? "").ToString();
        //    string lowerInvariant = ("AppDirect_" + (json["creator"]["firstName"] ?? "").ToString() + "." + (json["creator"]["lastName"] ?? "").ToString() + "." + (json["creator"]["uuid"] ?? "").ToString()).ToLowerInvariant();
        //    if (!Login(lowerInvariant)) {
        //        CreateUserOrchard(lowerInvariant, email);
        //        Login(lowerInvariant);
        //    }
        //    return _authenticationService.GetAuthenticatedUser() != null;
        //}

        //    public ActionResult LogOn(string loginIdentifier) {
        //        string stropenid = Request.QueryString["openid"];
        //        OpenIdRelyingParty rpopenid = new OpenIdRelyingParty();

        //        var response = rpopenid.GetResponse();
        //        if (response != null) {
        //            switch (response.Status) {
        //                case AuthenticationStatus.Authenticated:
        //                    var extradata = response.GetExtension<ClaimsResponse>();
        //                   var email= extradata.Email;
        //                    // NotLoggedIn.Visible = false;
        //                    Session["GoogleIdentifier"] = response.ClaimedIdentifier.ToString();
        //                    //AttributeValues att = new AttributeValues();
        //                    //Response.Write(Session["GoogleIdentifier"]);

        //                    //Response.Redirect("Main.aspx"); //redirect to main page of your website
        //                    break;
        //                case AuthenticationStatus.Canceled:
        //                    //lblAlertMsg.Text = "Cancelled.";
        //                    break;
        //                case AuthenticationStatus.Failed:
        //                    //lblAlertMsg.Text = "Login Failed.";
        //                    break;
        //            }
        //        }
        //        // string outresponse;
        //        // OpenId(openid, out outresponse, "", "");
        //        //// string str2 = Request.QueryString["accountId"];
        //        // WebClient webClient = new WebClient();
        //        // webClient.Credentials = (ICredentials)new NetworkCredential(ConsumerKey, ConsumerSecret);
        //        // webClient.Headers.Add("Content-Type", "application/json; charset=utf-8");
        //        // webClient.DownloadString(new Uri(openid));
        //        using (OpenIdRelyingParty openIdRelyingParty = new OpenIdRelyingParty()) {
        //            IAuthenticationRequest request = openIdRelyingParty.CreateRequest(stropenid);
        //            request.AddExtension(new ClaimsRequest {
        //                Email=DemandLevel.Request,
        //                Nickname=DemandLevel.Request
        //            });
        //            request.RedirectToProvider();
        ////            openIdRelyingParty.CreateRequest(stropenid).RedirectToProvider();
        //        }




        //           return null;
        //    }

        //[AcceptVerbs]
        //public ActionResult LogOn(string loginIdentifier) {
        //    if (!Identifier.IsValid(loginIdentifier)) {
        //    //    get_ModelState().AddModelError("loginIdentifier", "The specified login identifier is invalid");
        //        return (ActionResult)View();
        //    }
        //    IAuthenticationRequest request = new OpenIdRelyingParty().CreateRequest(Identifier.Parse(loginIdentifier));
        //    IAuthenticationRequest iauthenticationRequest = request;
        //    ClaimsRequest claimsRequest = new ClaimsRequest();
        //    //int num1 = 0;
        //    //claimsRequest.set_BirthDate((DemandLevel)num1);
        //    //int num2 = 2;
        //    //claimsRequest.set_Email((DemandLevel)num2);
        //    //int num3 = 2;
        //    //claimsRequest.set_FullName((DemandLevel)num3);
        //    iauthenticationRequest.AddExtension((IOpenIdMessageExtension)claimsRequest);
        //    return MessagingUtilities.AsActionResult(request.RedirectingResponse);
        //}
        private ContentItem CreateContentItemRequest(string jsonstring, RequestState Action) {
            JObject json = JObject.Parse(jsonstring);
            ContentItem contentItem = _contentManager.New("AppDirectRequest");
            _contentManager.Create(contentItem);
            if ((json["creator"]).Type != JTokenType.Null) {
                var appDirectUserPart = contentItem.As<AppDirectUserPart>();
                appDirectUserPart.Email = (json["creator"]["email"] ?? "").ToString();
                appDirectUserPart.FirstName = (json["creator"]["firstName"] ?? "").ToString();
                appDirectUserPart.Language = (json["creator"]["language"] ?? "").ToString();
                appDirectUserPart.LastName = (json["creator"]["lastName"] ?? "").ToString();
                appDirectUserPart.Locale = (json["creator"]["locale"] ?? "").ToString();
                appDirectUserPart.OpenIdCreator = (json["creator"]["openId"] ?? "").ToString();
                appDirectUserPart.UuidCreator = (json["creator"]["uuid"] ?? "").ToString();
            }
            if ((json["payload"]).Type != JTokenType.Null && (json["payload"]["company"]).Type != JTokenType.Null ) {
                var appDirectUserPart = contentItem.As<AppDirectUserPart>();
                appDirectUserPart.CompanyCountry = (json["payload"]["company"]["country"] ?? "").ToString();
                appDirectUserPart.CompanyName = (json["payload"]["company"]["name"] ?? "").ToString();
                appDirectUserPart.CompanyUuidCreator = (json["payload"]["company"]["uuid"] ?? "").ToString();
                appDirectUserPart.CompanyWebSite = (json["payload"]["company"]["website"] ?? "").ToString();
            }
            var appDirectRequestPart = ((dynamic)contentItem).AppDirectRequestPart;
            appDirectRequestPart.Request.Value = jsonstring;
            appDirectRequestPart.Action.Value = EnumHelper< RequestState>.GetDisplayValue(Action);
            appDirectRequestPart.State.Value = Action.ToString();
            if ((json["payload"]).Type != JTokenType.Null && (json["payload"]["order"]).Type != JTokenType.Null && (json["payload"]["order"]["editionCode"]).Type != JTokenType.Null) {
                appDirectRequestPart.Edition.Value = (json["payload"]["order"]["editionCode"] ?? "").ToString();
            }
            appDirectRequestPart.Uri.Value = Request.QueryString["url"];
            var user = _membershipService.GetUser("Market_AppDirect");
            contentItem.As<CommonPart>().Owner = user;
            return contentItem;
        }
        public ActionResult Create() {
            string str = Request.QueryString["url"];
            _appDirectCommunication.WriteEvent(EventType.Input, str);
            if (VerifyValidRequest()) {
                _appDirectCommunication.WriteEvent(EventType.Input, "OpenAuthValidation");
            }
            else {
                _appDirectCommunication.WriteEvent(EventType.Input, "OpenAuthValidation Failed");
                Response.StatusCode = 404;
            }
            string outresponse;
            if (_appDirectCommunication.MakeRequestToAppdirect(str,Method.GET,"", out outresponse, "", "") && !string.IsNullOrEmpty(outresponse)) {
                var contentitem=CreateContentItemRequest(outresponse,RequestState.ToCreate);
                _workflowManager.TriggerEvent("SubscriptionEvent", contentitem, () => new Dictionary<string, object> { { "Content", contentitem }, { "Action", "Create" } });
                Response.StatusCode = 202; //async
                var dataResponse = new { success = "true" };
                return Json((object)dataResponse, JsonRequestBehavior.AllowGet);
            }
            else {
                Logger.Error(T("Can't retrive order {0}", str).ToString());
                _appDirectCommunication.WriteEvent(EventType.Input, "Error Can't retrive order "+str);
                var dataResponse = new { success = "false", errorCode= "INVALID_RESPONSE", message="Can't access order" };
                return Json((object)dataResponse, JsonRequestBehavior.AllowGet);
            }
        }

 

    public ActionResult Edit()
    {
      HttpContext.Request.QueryString.ToString();
      var data = new{ success = "True" };
      return (ActionResult) Json((object) data, (JsonRequestBehavior) 0);
    }

    public ActionResult Cancel()
    {
           string str = Request.QueryString["url"];
            _appDirectCommunication.WriteEvent(EventType.Input, str);
            if (VerifyValidRequest()) {
                _appDirectCommunication.WriteEvent(EventType.Input, "OpenAuthValidation");
            }
            else {
                _appDirectCommunication.WriteEvent(EventType.Input, "OpenAuthValidation Failed");
                Response.StatusCode = 404;
            }
            string outresponse;
            if (_appDirectCommunication.MakeRequestToAppdirect(str, Method.GET, "", out outresponse, "", "") && !string.IsNullOrEmpty(outresponse)) {
                var contentitem = CreateContentItemRequest(outresponse,RequestState.ToCancel);
                _workflowManager.TriggerEvent("SubscriptionEvent", contentitem, () => new Dictionary<string, object> { { "Content", contentitem }, { "Action", "Cancel" } });
                Response.StatusCode = 202; //async
                var dataResponse = new { success = "true" };
                return Json((object)dataResponse, JsonRequestBehavior.AllowGet);
            }
            else {
                Logger.Error(T("Can't retrive order {0}", str).ToString());
                _appDirectCommunication.WriteEvent(EventType.Input, "Error Can't retrive order " + str);
                var dataResponse = new { success = "false", errorCode = "INVALID_RESPONSE", message = "Can't access order" };
                return Json((object)dataResponse, JsonRequestBehavior.AllowGet);
            }
    }

    public ActionResult Status()
    {
      HttpContext.Request.QueryString.ToString();
      var data = new{ success = "True" };
      return (ActionResult) Json((object) data, (JsonRequestBehavior) 0);
    }
  }
}
