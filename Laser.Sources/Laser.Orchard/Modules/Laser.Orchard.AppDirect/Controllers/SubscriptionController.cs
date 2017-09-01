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
        private readonly IRepository<AppDirectSettingsRecord> _repoSetting;

        public Localizer T { get; set; }

        public SubscriptionController(
            IMembershipService membershipService,
            IAuthenticationService authenticationService,
            IUserEventHandler userEventHandler,
            IContentManager contentManager,
            IWorkflowManager workflowManager,
            IOrchardServices orchardServices,
            IAppDirectCommunication appDirectCommunication,
            IRepository<AppDirectSettingsRecord> repoSetting) {
            _appDirectCommunication = appDirectCommunication;
            _orchardServices = orchardServices;
            _workflowManager = workflowManager;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _userEventHandler = userEventHandler;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            _contentManager = contentManager;
            _repoSetting = repoSetting;
        }

        private static string GetAuthorizationHeaderValue(string Authorization, string key) {
            var value = "";
            var pieces = Authorization.Split(new[] { " oauth_" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string piece in pieces) {
                if (piece.StartsWith(key + "=")) {
                    value = piece.Substring(key.Length + 2, piece.LastIndexOf("\"") - (key.Length + 2));
                    break;
                }
            }
            return value;
        }

        private bool VerifyValidRequest(string key) {
            var authorization = (Request.Headers["Authorization"] ?? "").ToString();
            if (string.IsNullOrEmpty(authorization)) {
                Logger.Error(T("Authorization Header is empty").ToString());
                return false;
            }

            var setting_oAuth = _appDirectCommunication.Get_oAuthCredential(key);
            if (setting_oAuth == null) {
                _appDirectCommunication.WriteEvent(EventType.Output, string.Format("oAuth key not found -> {0}", key));
                return false;
            }
            var consumerKey = setting_oAuth.ConsumerKey;
            var consumerSecret = setting_oAuth.ConsumerSecret;
            var oauth_consumer_key = GetAuthorizationHeaderValue(authorization, "consumer_key");
            var oauth_nonce = GetAuthorizationHeaderValue(authorization, "nonce");
            var oauth_signature = GetAuthorizationHeaderValue(authorization, "signature");
            var oauth_timestamp = GetAuthorizationHeaderValue(authorization, "timestamp");
            if (!oauth_consumer_key.Equals(consumerKey)) {
                Logger.Error(T("Authorization Header {0} have incorrect ConsumerKey", authorization).ToString());
                return false;
            }
            var oauthBase = new OAuthBase();
            var normalizedUrl = "";
            var normalizedRequestParameters = "";
            var uri = Request.Url;
            // if i use ip then i'm debugging on my pc and i correct uri with the port redirect
            if (uri.ToString().Contains("185.11.22.191")) {
                var uriBuilder = new UriBuilder(Request.Url) {
                    Port = 1235
                };
                uri = uriBuilder.Uri;
            }
            var oauth_signature_Calculated = oauthBase.GenerateSignature(uri, consumerKey, consumerSecret, "", "", "GET", oauth_timestamp, oauth_nonce, out normalizedUrl, out normalizedRequestParameters);
            // faccio HttpUtility.decode e non encodo quello calcolato perchè HttpUtility.Encode uso uno standart lowercase mentre java usa lo standard uppercase e quindi le stringhe sarebbero diverse
            var oauth_signature_decoded = HttpUtility.UrlDecode(oauth_signature);
            if (oauth_signature_decoded.Equals(oauth_signature_Calculated))
                return true;
            else
                Logger.Error(T("Authorization Header {0} have incorrect oauth_signature", authorization).ToString());
            return false;
        }

        private ContentItem CreateContentItemRequest(string jsonstring, RequestState Action, string key) {
            var json = JObject.Parse(jsonstring);
            var contentItem = _contentManager.New("AppDirectRequest");
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
                if ((json["payload"]).Type != JTokenType.Null && (json["payload"]["account"]).Type != JTokenType.Null && (json["payload"]["account"]["accountIdentifier"]).Type != JTokenType.Null)
                    appDirectUserPart.AccountIdentifier = (json["payload"]["account"]["accountIdentifier"] ?? "").ToString();
            }
            if ((json["payload"]).Type != JTokenType.Null && (json["payload"]["company"]).Type != JTokenType.Null) {
                var appDirectUserPart = contentItem.As<AppDirectUserPart>();
                appDirectUserPart.CompanyCountry = (json["payload"]["company"]["country"] ?? "").ToString();
                appDirectUserPart.CompanyName = (json["payload"]["company"]["name"] ?? "").ToString();
                appDirectUserPart.CompanyUuidCreator = (json["payload"]["company"]["uuid"] ?? "").ToString();
                appDirectUserPart.CompanyWebSite = (json["payload"]["company"]["website"] ?? "").ToString();
            }
            var appDirectRequestPart = ((dynamic)contentItem).AppDirectRequestPart;
            appDirectRequestPart.Request.Value = jsonstring;
            appDirectRequestPart.Action.Value = EnumHelper<RequestState>.GetDisplayValue(Action);
            appDirectRequestPart.State.Value = Action.ToString();
            if ((json["payload"]).Type != JTokenType.Null && (json["payload"]["order"]).Type != JTokenType.Null && (json["payload"]["order"]["editionCode"]).Type != JTokenType.Null) {
                appDirectRequestPart.Edition.Value = (json["payload"]["order"]["editionCode"] ?? "").ToString();
            }
            appDirectRequestPart.Uri.Value = Request.QueryString["url"];
            appDirectRequestPart.ProductKey.Value = key;
            var user = _membershipService.GetUser("Market_AppDirect");
            contentItem.As<CommonPart>().Owner = user;
            return contentItem;
        }
        public ActionResult Create() {
            var str = Request.QueryString["url"];
            var key = Request.QueryString["productKey"];
            _appDirectCommunication.WriteEvent(EventType.Input, str);
            if (VerifyValidRequest(key)) {
                _appDirectCommunication.WriteEvent(EventType.Input, "OpenAuthValidation");
            }
            else {
                _appDirectCommunication.WriteEvent(EventType.Input, "OpenAuthValidation Failed");
                Response.StatusCode = 404;
            }
            string outresponse;
            if (_appDirectCommunication.MakeRequestToAppdirect(str, Method.GET, "", key, out outresponse, "", "") && !string.IsNullOrEmpty(outresponse)) {
                var contentitem = CreateContentItemRequest(outresponse, RequestState.ToCreate, key);
                _workflowManager.TriggerEvent("SubscriptionEvent", contentitem, () => new Dictionary<string, object> { { "Content", contentitem }, { "Action", "Create" } });
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



        public ActionResult Edit() {
            var str = Request.QueryString["url"];
            var key = Request.QueryString["productKey"];
            _appDirectCommunication.WriteEvent(EventType.Input, str);
            if (VerifyValidRequest(key)) {
                _appDirectCommunication.WriteEvent(EventType.Input, "OpenAuthValidation");
            }
            else {
                _appDirectCommunication.WriteEvent(EventType.Input, "OpenAuthValidation Failed");
                Response.StatusCode = 404;
            }
            string outresponse;
            if (_appDirectCommunication.MakeRequestToAppdirect(str, Method.GET, "", key, out outresponse, "", "") && !string.IsNullOrEmpty(outresponse)) {
                var contentitem = CreateContentItemRequest(outresponse, RequestState.ToModify, key);
                _workflowManager.TriggerEvent("SubscriptionEvent", contentitem, () => new Dictionary<string, object> { { "Content", contentitem }, { "Action", "Edit" } });
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

        public ActionResult Cancel() {
            var str = Request.QueryString["url"];
            var key = Request.QueryString["productKey"];
            _appDirectCommunication.WriteEvent(EventType.Input, str);
            if (VerifyValidRequest(key)) {
                _appDirectCommunication.WriteEvent(EventType.Input, "OpenAuthValidation");
            }
            else {
                _appDirectCommunication.WriteEvent(EventType.Input, "OpenAuthValidation Failed");
                Response.StatusCode = 404;
            }
            string outresponse;
            if (_appDirectCommunication.MakeRequestToAppdirect(str, Method.GET, "", key, out outresponse, "", "") && !string.IsNullOrEmpty(outresponse)) {
                var contentitem = CreateContentItemRequest(outresponse, RequestState.ToCancel, key);
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

        public ActionResult Status() {
            var str = Request.QueryString["url"];
            var key = Request.QueryString["productKey"];
            _appDirectCommunication.WriteEvent(EventType.Input, str);
            if (VerifyValidRequest(key)) {
                _appDirectCommunication.WriteEvent(EventType.Input, "OpenAuthValidation");
            }
            else {
                _appDirectCommunication.WriteEvent(EventType.Input, "OpenAuthValidation Failed");
                Response.StatusCode = 404;
            }
            string outresponse;
            if (_appDirectCommunication.MakeRequestToAppdirect(str, Method.GET, "", key, out outresponse, "", "") && !string.IsNullOrEmpty(outresponse)) {
                var contentitem = CreateContentItemRequest(outresponse, RequestState.Status, key);
                _workflowManager.TriggerEvent("SubscriptionEvent", contentitem, () => new Dictionary<string, object> { { "Content", contentitem }, { "Action", "Status" } });
                Response.StatusCode = 200; //NOT async
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
    }
}
