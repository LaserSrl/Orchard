
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.Messages;
using DotNetOpenAuth.OpenId.RelyingParty;
using Laser.Orchard.AppDirect.Models;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Users.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Net.Http.Headers;

namespace Laser.Orchard.AppDirect.Controllers {
    public class SubscriptionController : Controller {
        public string ConsumerKey= "app-retail-b2c-172476";
        public string ConsumerSecret= "wpIwWYppbVM07hIV";
        private readonly IMembershipService _membershipService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly ILogger Logger;
        private readonly IRepository<LogEventsRecord> _repositoryLog;
        private readonly IContentManager _contentManager;
  

        public Localizer T { get; set; }

        public SubscriptionController(IMembershipService membershipService, IAuthenticationService authenticationService, IUserEventHandler userEventHandler, IRepository<LogEventsRecord> repositoryLog, IContentManager contentManager) {

            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _userEventHandler = userEventHandler;
            _repositoryLog = repositoryLog;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            _contentManager = contentManager;
        }

        protected string NormalizeParameters(SortedDictionary<string, string> parameters) {
            StringBuilder stringBuilder = new StringBuilder();
            int num = 0;
            foreach (KeyValuePair<string, string> parameter in parameters) {
                if (num > 0)
                    stringBuilder.Append("&");
                stringBuilder.AppendFormat("{0}={1}", (object)parameter.Key, (object)parameter.Value);
                ++num;
            }
            return stringBuilder.ToString();
        }

        private string GenerateBase(string nonce, string timeStamp, Uri url) {
            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>() { { "oauth_consumer_key", ConsumerKey }, { "oauth_signature_method", "HMAC-SHA1" }, { "oauth_timestamp", timeStamp }, { "oauth_nonce", nonce }, { "oauth_version", "1.0" } };
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("GET");
            stringBuilder.Append("&" + Uri.EscapeDataString(url.AbsoluteUri));
            stringBuilder.Append("&" + Uri.EscapeDataString(NormalizeParameters(parameters)));
            return stringBuilder.ToString();
        }

        public string GenerateSignature(string nonce, string timeStamp, Uri url) {
            return Convert.ToBase64String(new HMACSHA1(Encoding.ASCII.GetBytes(string.Format("{0}&{1}",  ConsumerSecret, (object)""))).ComputeHash(new ASCIIEncoding().GetBytes(GenerateBase(nonce, timeStamp, url))));
        }

        private bool VerifyValidRequest() {
            if (string.IsNullOrEmpty((Request.Headers["Authorization"] ?? "").ToString()))
                return false;
           // return true;
            var oauth_consumer_key = "app-retail-b2c-172476"; 
var oauth_nonce = "-383666841795120434";
            var oauth_signature = "QdGA1k3BIalYMGterng1UnCgilY%3D";
            var oauth_signature_method = "HMAC-SHA1";
var oauth_timestamp = "1501590001"; 
var oauth_version = "1.0";
            OAuthBase oauthBase = new OAuthBase();
            string normalizedUrl = "";
            string normalizedRequestParameters = "";
            //Request.Url
            var aa=oauthBase.GenerateSignature(Request.Url, ConsumerKey, ConsumerSecret, "", "", "GET", oauth_timestamp, oauth_nonce, out normalizedUrl, out normalizedRequestParameters);
            //OAuthContext context = new OAuthContextBuilder().FromHttpRequest(request);
            //OAuthContextSigner signer = new OAuthContextSigner();
            //SigningContext signingContext = new SigningContext();

            //signingContext.Algorithm = ...; // if a certificate is associated with the consumer (for RSA-SHA1 etc.)
            //signingContext.ConsumerSecret = ...; // if there is a consumer secret

            //if (signer.ValidateSignature(context, signingContext)) {
            //    // signature was valid.
            //}



            return true;
        }

        private void WriteEvent(EventType type, string log) {
            _repositoryLog.Create(new LogEventsRecord(type, log, GetCurrentMethod()));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod() {
            return new StackTrace().GetFrame(2).GetMethod().Name;
        }

        private bool OpenId(string uri, out string outresponse, string token = "", string tokenSecret = "") {
            outresponse = "";
            try {
                OAuthBase oauthBase = new OAuthBase();
                if (string.IsNullOrEmpty(uri))
                    return false;
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

        private bool Login(string userName) {
            if (string.IsNullOrWhiteSpace(userName))
                return false;
            IUser user = _membershipService.GetUser(userName);
            if (user != null)
                _authenticationService.SignIn(user, true);
            IUser authenticatedUser = _authenticationService.GetAuthenticatedUser();
            if (authenticatedUser == null)
                return false;
            _userEventHandler.LoggedIn(authenticatedUser);
            return true;
        }

        private bool CreateUserOrchard(string username, string email) {
            try {
                string password = Membership.GeneratePassword(10, 5);
                if (_membershipService.CreateUser(new CreateUserParams(username, password, email, T.Invoke("Auto Registered User", new object[0]).Text, password, true)) != null)
                    return true;
                Logger.Error( string.Format("AppDirect => Error Creating user username={0} email={1}", (object)username, (object)email));
                return false;
            }
            catch (Exception ex) {
                Logger.Error(string.Format("AppDirect => Error Creating user username={0} email={1}", (object)username, (object)email) + " " + ex.Message);
                return false;
            }
        }

        private bool CreateOrLoginUser(JObject json) {
            if (json["creator"] == null)
                return false;
            string email = (json["creator"]["email"] ?? "").ToString();
            string lowerInvariant = ("AppDirect_" + (json["creator"]["firstName"] ?? "").ToString() + "." + (json["creator"]["lastName"] ?? "").ToString() + "." + (json["creator"]["uuid"] ?? "").ToString()).ToLowerInvariant();
            if (!Login(lowerInvariant)) {
                CreateUserOrchard(lowerInvariant, email);
                Login(lowerInvariant);
            }
            return _authenticationService.GetAuthenticatedUser() != null;
        }

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

        public ActionResult Create() {
            string str = Request.QueryString["url"];
            WriteEvent(EventType.Input, str);
            if (VerifyValidRequest()) {
                WriteEvent(EventType.Input, "OpenAuthValidation");
            }
            else {
                WriteEvent(EventType.Input, "OpenAuthValidation Failed");
                Response.StatusCode = 404;
            }
            string outresponse;
            if (OpenId(str, out outresponse, "", "") && !string.IsNullOrEmpty(outresponse)) {
                
                CreateContentItemRequest(outresponse);
                //if (!CreateOrLoginUser(json)) {

                //}
            }
            var data = new { success = "True" };
            return Json((object)data, (JsonRequestBehavior)0);
        }

        private void CreateContentItemRequest(string jsonstring) {
            JObject json = JObject.Parse(jsonstring);
            ContentItem contentItem = _contentManager.New("AppDirectRequest");
            _contentManager.Create(contentItem);
            if (json["creator"] != null) {
                contentItem.As<AppDirectUserPart>().Email = (json["creator"]["email"] ?? "").ToString();
                contentItem.As<AppDirectUserPart>().FirstName = (json["creator"]["firstName"] ?? "").ToString();
                contentItem.As<AppDirectUserPart>().Language = (json["creator"]["language"] ?? "").ToString();
                contentItem.As<AppDirectUserPart>().LastName = (json["creator"]["lastName"] ?? "").ToString();
                contentItem.As<AppDirectUserPart>().Locale = (json["creator"]["locale"] ??"").ToString();
                contentItem.As<AppDirectUserPart>().OpenIdCreator = (json["creator"]["openId"] ?? "").ToString();
                contentItem.As<AppDirectUserPart>().UuidCreator = (json["creator"]["uuid"] ?? "").ToString();
            }
            if (json["payload"] != null && json["payload"]["company"] != null) {
                contentItem.As<AppDirectUserPart>().CompanyCountry = (json["payload"]["company"]["country"] ?? "").ToString();
                contentItem.As<AppDirectUserPart>().CompanyName = (json["payload"]["company"]["name"] ?? "").ToString();
                contentItem.As<AppDirectUserPart>().CompanyUuidCreator = (json["payload"]["company"]["uuid"] ?? "").ToString();
                contentItem.As<AppDirectUserPart>().CompanyWebSite = (json["payload"]["company"]["website"] ?? "").ToString();
            }
            ((dynamic)contentItem).AppDirectRequestPart.Request.Value = jsonstring;
            ((dynamic)contentItem).AppDirectRequestPart.Action.Value = "Create instance.";
            _contentManager.Publish(contentItem);
        }

    public ActionResult Edit()
    {
      HttpContext.Request.QueryString.ToString();
      var data = new{ success = "True" };
      return (ActionResult) Json((object) data, (JsonRequestBehavior) 0);
    }

    public ActionResult Cancel()
    {
      HttpContext.Request.QueryString.ToString();
      var data = new{ success = "True" };
      return (ActionResult) Json((object) data, (JsonRequestBehavior) 0);
    }

    public ActionResult Status()
    {
      HttpContext.Request.QueryString.ToString();
      var data = new{ success = "True" };
      return (ActionResult) Json((object) data, (JsonRequestBehavior) 0);
    }
  }
}
