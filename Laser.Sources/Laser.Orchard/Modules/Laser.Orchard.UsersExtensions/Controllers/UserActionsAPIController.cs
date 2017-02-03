using System.Web.Http;
using Laser.Orchard.Mobile.ViewModels;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.UsersExtensions.Models;
using Laser.Orchard.UsersExtensions.Services;

namespace Laser.Orchard.UsersExtensions.Controllers {


    public class UserActionsAPIController : ApiController {
  
            //private readonly ICsrfTokenHelper _csrfTokenHelper;
            //private readonly IUsersExtensionsServices _usersExtensionsServices;
            //private readonly IControllerContextAccessor _controllerContextAccessor;
            //private readonly IOrchardServices _orchardServices;
            //private readonly IUserService _userService;
            //private readonly IUtilsServices _utilsServices;
            private readonly IUserActionMethods _userActionMethods;
          //  public ILogger Log { get; set; }

            public UserActionsAPIController(
                //IOrchardServices orchardServices, ICsrfTokenHelper csrfTokenHelper, IUsersExtensionsServices usersExtensionsServices, IUserService userService,IControllerContextAccessor controllerContextAccessor, IUtilsServices utilsServices,
                IUserActionMethods userActionMethods) {
                //_csrfTokenHelper = csrfTokenHelper;
                //_usersExtensionsServices = usersExtensionsServices;
                //_controllerContextAccessor = controllerContextAccessor;
                //_orchardServices = orchardServices;
                //_userService = userService;
                //T = NullLocalizer.Instance;
                //Log = NullLogger.Instance;
                //_utilsServices = utilsServices;
                _userActionMethods = userActionMethods;
            }

         //   public Localizer T { get; set; }

            #region [https calls]

            /// <summary>
            /// </summary>
            /// <param name="userRegistrationParams">
            /// </param>
            /// <returns></returns>
            [System.Web.Mvc.HttpPost]
            public Response RegisterSsl(UserRegistration userRegistrationParams) {
                return _userActionMethods.RegisterLogic(userRegistrationParams);
            }

            [System.Web.Mvc.HttpPost]
            public Response SignInSsl(UserLogin login) {
                return _userActionMethods.SignInLogic(login);
            }

            [System.Web.Mvc.HttpPost]
            public Response SignOutSsl() {
                return _userActionMethods.SignOutLogic();
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="phoneNumber">
            ///
            /// {
            ///     "phoneNumber":{
            ///         "internationalPrefix":"39",
            ///         "phoneNumber":"3477543903"
            ///     }
            /// }
            /// </param>
            /// <returns></returns>
            [System.Web.Mvc.HttpPost]
            public Response RequestLostPasswordSmsSsl(PhoneNumberViewModel phoneNumber) {
                return _userActionMethods.RequestLostPasswordLogic(phoneNumber.PhoneNumber, LostPasswordUserOptions.Phone, phoneNumber.InternationalPrefix);
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="username">
            ///  {
            ///      "username":"h.sbicego"
            ///  }
            /// </param>
            /// <returns></returns>
            [System.Web.Mvc.HttpPost]
            public Response RequestLostPasswordAccountOrEmailSsl(string username) {
                return _userActionMethods.RequestLostPasswordLogic(username, LostPasswordUserOptions.Account);
            }

  
            public string GetCleanRegistrationPoliciesSsl(string lang = null) {
                return _userActionMethods.GetCleanRegistrationPoliciesLogic(lang);
            }


            public string GetRegistrationPoliciesSsl(string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string lang = null) {
                return _userActionMethods.GetRegistrationPoliciesLogic(mfilter, page, pageSize, tinyResponse, minified, realformat, deeplevel, lang);
            }

   

            public UserRegistration GetUserRegistrationModelSsl() {
                return _userActionMethods.GetUserRegistrationModelLogic();
            }

            #endregion [https calls]

            #region [http calls]

            [System.Web.Mvc.HttpPost]
            public Response Register(UserRegistration userRegistrationParams) {
                return _userActionMethods.RegisterLogic(userRegistrationParams);
            }

            [System.Web.Mvc.HttpPost]
            public Response SignIn(UserLogin login) {
                return _userActionMethods.SignInLogic(login);
            }

            [System.Web.Mvc.HttpPost]
            public Response SignOut() {
                return _userActionMethods.SignOutLogic();
            }

            [System.Web.Mvc.HttpPost]
            public Response RequestLostPasswordSms(PhoneNumberViewModel phoneNumber) {
                return _userActionMethods.RequestLostPasswordLogic(phoneNumber.PhoneNumber, LostPasswordUserOptions.Phone, phoneNumber.InternationalPrefix);
            }

            [System.Web.Mvc.HttpPost]
            public Response RequestLostPasswordAccountOrEmail(string username) {
                return _userActionMethods.RequestLostPasswordLogic(username, LostPasswordUserOptions.Account);
            }


            //public string GetCleanRegistrationPolicies(string lang = null) {
            //    return Json.Parser(_userActionMethods.GetCleanRegistrationPoliciesLogic(lang));
            //}


            //public string GetRegistrationPolicies(string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string lang = null) {
            //    return _userActionMethods.GetRegistrationPoliciesLogic(mfilter, page, pageSize, tinyResponse, minified, realformat, deeplevel, lang);
            //}


            public UserRegistration GetUserRegistrationModel() {
                return _userActionMethods.GetUserRegistrationModelLogic();
            }

            #endregion [http calls]

            //private Response RegisterLogic(UserRegistration userRegistrationParams) {
            //    Response result;
            //    // ensure users can request lost password
            //    var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            //    if (!registrationSettings.UsersCanRegister) {
            //        result = _utilsServices.GetResponse(ResponseType.None, T("Users cannot register due to site settings.").Text);
            //        return result;
            //    }
            //    try {
            //        _usersExtensionsServices.Register(userRegistrationParams);
            //        var registeredServicesData = new {
            //            RegisteredServices = _controllerContextAccessor.Context.Controller.TempData
            //        };
            //        result = _utilsServices.GetResponse(ResponseType.Success, data: registeredServicesData);
            //    }
            //    catch (Exception ex) {
            //        result = _utilsServices.GetResponse(ResponseType.None, ex.Message);
            //    }

            //    return result;
            //}

            //private Response SignInLogic(UserLogin login) {
            //    Response result;
            //    try {
            //        _usersExtensionsServices.SignIn(login);
            //        var registeredServicesData = new {
            //            RegisteredServices = _controllerContextAccessor.Context.Controller.TempData
            //        };
            //        result = _utilsServices.GetResponse(ResponseType.Success, "", registeredServicesData);
            //    }
            //    catch (Exception ex) {
            //        result = _utilsServices.GetResponse(ResponseType.InvalidUser, ex.Message);
            //    }
            //    return result;
            //}

            //private Response SignOutLogic() {
            //    Response result;
            //    try {
            //        _usersExtensionsServices.SignOut();
            //        result = _utilsServices.GetResponse(ResponseType.Success);
            //    }
            //    catch (Exception ex) {
            //        result = _utilsServices.GetResponse(ResponseType.InvalidUser, ex.Message);
            //    }
            //    return result;
            //}

            //private string GetCleanRegistrationPoliciesLogic(string lang = null) {
            //    var sb = new StringBuilder();
            //    var policies = _usersExtensionsServices.GetUserLinkedPolicies(lang);

            //    //policy.PendingPolicies
            //    sb.Insert(0, "{");
            //    sb.Append("\"Policies\":[");

            //    int i = 0;

            //    foreach (var item in policies) {
            //        if (i > 0) {
            //            sb.Append(",");
            //        }
            //        sb.Append("{");
            //        sb.Append("\"PolicyId\":" + item.Id.ToString() + ",");
            //        sb.Append("\"Title\":\"" + item.ContentItem.As<TitlePart>().Title.Replace("\"", "\\\"") + "\",");
            //        sb.Append("\"Body\":\"" + item.ContentItem.As<BodyPart>().Text.Replace("\"", "\\\"").Replace("\r\n", "\\r\\n") + "\",");
            //        sb.Append("\"PolicyType\":\"" + item.PolicyType.ToString() + "\",");
            //        sb.Append("\"UserHaveToAccept\":" + item.UserHaveToAccept.ToString().ToLowerInvariant() + "");
            //        sb.Append("}");
            //        i++;
            //    }
            //    sb.Append("]");
            //    sb.Append("}");
            //    return sb.ToString();
            //    //return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
            //}

            //private ContentResult GetRegistrationPoliciesLogic(string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string lang = null, string complexBehaviour = "") {
            //    var sb = new StringBuilder();
            //    var _filterContentFieldsParts = mfilter.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //    XElement dump;
            //    XElement projectionDump = null;
            //    // il dump dell'oggetto principale non filtra per field
            //    ObjectDumper dumper;
            //    var policies = _usersExtensionsServices.GetUserLinkedPolicies(lang);

            //    //policy.PendingPolicies
            //    sb.Insert(0, "{");
            //    sb.AppendFormat("\"n\": \"{0}\"", "Model");
            //    sb.AppendFormat(", \"v\": \"{0}\"", "VirtualContent");
            //    sb.Append(", \"m\": [{");
            //    sb.AppendFormat("\"n\": \"{0}\"", "VirtualId"); // Unused property for mobile mapper needs
            //    sb.AppendFormat(", \"v\": \"{0}\"", "0");
            //    sb.Append("}]");

            //    sb.Append(", \"l\":[");

            //    int i = 0;
            //    sb.Append("{");
            //    sb.AppendFormat("\"n\": \"{0}\"", "RegistrationPolicies");
            //    sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
            //    sb.Append(", \"m\": [");

            //    foreach (var item in policies) {
            //        if (i > 0) {
            //            sb.Append(",");
            //        }
            //        sb.Append("{");
            //        dumper = new ObjectDumper(deeplevel, _filterContentFieldsParts, false, tinyResponse, complexBehaviour.Split(','));
            //        projectionDump = dumper.Dump(item.ContentItem, String.Format("[{0}]", i));
            //        JsonConverter.ConvertToJSon(projectionDump, sb, minified, realformat);
            //        sb.Append("}");
            //        i++;
            //    }
            //    sb.Append("]");
            //    sb.Append("}");

            //    sb.Append("]"); // l : [
            //    sb.Append("}");
            //    return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
            //}

            //private UserRegistration GetUserRegistrationModelLogic() {
            //    var userRegistration = new UserRegistration {
            //        Username = "MyUserName",
            //        Password = "MyPassword",
            //        ConfirmPassword = "MyPassword",
            //        PasswordQuestion = "MyPasswordQuestion",
            //        PasswordAnswer = "MyPasswordAnswer",
            //        Email = "myname@mydomain.it",
            //        Culture = "it-IT",
            //        PolicyAnswers = _usersExtensionsServices.GetUserLinkedPolicies("it-IT").Select(x => new UserPolicyAnswer {
            //            PolicyId = x.Id,
            //            UserHaveToAccept = x.UserHaveToAccept,
            //            PolicyAnswer = false
            //        }).ToList()
            //    };
            //    return userRegistration;
            //}

            //private Response RequestLostPasswordLogic(string username, LostPasswordUserOptions userOptions, string internationalPrefix = null) {
            //    // ensure users can request lost password
            //    Response result = _utilsServices.GetResponse(ResponseType.None, T("Send email failed.").Text);
            //    var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            //    if (!registrationSettings.EnableLostPassword) {
            //        result = _utilsServices.GetResponse(ResponseType.None, T("Users cannot recover lost password due to site settings.").Text);

            //        return (result);
            //    }

            //    if (String.IsNullOrWhiteSpace(username)) {
            //        result = _utilsServices.GetResponse(ResponseType.None, T("Invalid user.").Text);
            //        return (result);
            //    }

            //    var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
            //    if (String.IsNullOrWhiteSpace(siteUrl)) {
            //       // siteUrl = HttpContext.Request.ToRootUrlString();
            //        siteUrl = string.Format("{0}://{1}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Headers["Host"]);
            //    }

            //    // test if user is user/email or phone number
            //    UrlHelper urlHelper = new UrlHelper();
            //    if (userOptions == LostPasswordUserOptions.Account) {
                   

            //        if (_userService.SendLostPasswordEmail(username, nonce => urlHelper.MakeAbsolute(urlHelper.Action("LostPassword", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl))) {
            //            result = _utilsServices.GetResponse(ResponseType.Success);
            //        }
            //        else {
            //            result = _utilsServices.GetResponse(ResponseType.None, T("Send email failed.").Text);
            //        }
            //    }
            //    else {
            //        var sendSmsResult = _usersExtensionsServices.SendLostPasswordSms(internationalPrefix, username, nonce => urlHelper.MakeAbsolute(urlHelper.Action("LostPassword", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl));

            //        if (sendSmsResult == "TRUE") {
            //            result = _utilsServices.GetResponse(ResponseType.Success);
            //        }
            //        else {
            //            Dictionary<string, string> errors = new Dictionary<string, string>();
            //            errors.Add("BODYEXCEEDED", T("Message rejected: too many characters. (160 max)").ToString()); //"messaggio rigettato per superamento lunghezza max di testo (160 caratteri)");
            //            errors.Add("MISSINGPARAMETER_1", T("Missing recipient").ToString()); //"Destinatario mancante");
            //            errors.Add("MISSINGPARAMETER_2", T("Sender identifier missing").ToString()); //"Identificativo di invio mancante");
            //            errors.Add("MISSINGPARAMETER_3", T("Sender missing or wrong").ToString()); //"Mittente mancante o errato");
            //            errors.Add("MISSINGPARAMETER_4", T("Missing text").ToString()); //"Testo mancante");
            //            errors.Add("MISSINGPARAMETER_5", T("Priority missing or wrong").ToString()); //"Priorità mancante o errata");
            //            errors.Add("FALSE", T("Generic error").ToString()); //"Errore generico");
            //            result = _utilsServices.GetResponse(ResponseType.None, T("Send SMS failed.").Text + errors[sendSmsResult].ToString());
            //        }
            //    }
            //    return (result);
            //}
        }


}