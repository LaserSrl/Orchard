using Laser.Orchard.Commons.Services;
using Laser.Orchard.Mobile.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.UsersExtensions.Models;
using Laser.Orchard.UsersExtensions.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Laser.Orchard.UsersExtensions.Controllers {

    [OutputCache(NoStore = true, Duration = 0)]
    [RoutePrefix("UserActions")]
    public class UserActionsController : Controller {
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IUsersExtensionsServices _usersExtensionsServices;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IOrchardServices _orchardServices;
        private readonly IUserService _userService;
        private readonly IUtilsServices _utilsServices;

        public ILogger Log { get; set; }

        public UserActionsController(IOrchardServices orchardServices, ICsrfTokenHelper csrfTokenHelper, IUsersExtensionsServices usersExtensionsServices, IUserService userService,
            IControllerContextAccessor controllerContextAccessor, IUtilsServices utilsServices) {
            _csrfTokenHelper = csrfTokenHelper;
            _usersExtensionsServices = usersExtensionsServices;
            _controllerContextAccessor = controllerContextAccessor;
            _orchardServices = orchardServices;
            _userService = userService;
            T = NullLocalizer.Instance;
            Log = NullLogger.Instance;
            _utilsServices = utilsServices;
        }

        public Localizer T { get; set; }

        #region [https calls]

        /// <summary>
        /// </summary>
        /// <param name="userRegistrationParams">
        /// </param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RegisterSsl(UserRegistration userRegistrationParams) {
            return RegisterLogic(userRegistrationParams);
        }

        [HttpPost]
        public JsonResult SignInSsl(UserLogin login) {
            return SignInLogic(login);
        }

        [HttpPost]
        public JsonResult SignOutSsl() {
            return SignOutLogic();
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
        [HttpPost]
        public JsonResult RequestLostPasswordSmsSsl(PhoneNumberViewModel phoneNumber) {
            return RequestLostPasswordLogic(phoneNumber.PhoneNumber, LostPasswordUserOptions.Phone, phoneNumber.InternationalPrefix);
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
        [HttpPost]
        public JsonResult RequestLostPasswordAccountOrEmailSsl(string username) {
            return RequestLostPasswordLogic(username, LostPasswordUserOptions.Account);
        }

        [HttpGet]
        public ContentResult GetCleanRegistrationPoliciesSsl(string lang = null) {
            return GetCleanRegistrationPoliciesLogic(lang);
        }

        [HttpGet]
        public ContentResult GetRegistrationPoliciesSsl(string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string lang = null) {
            return GetRegistrationPoliciesLogic(mfilter, page, pageSize, tinyResponse, minified, realformat, deeplevel, lang);
        }

        [HttpGet]
        public JsonResult GetUserRegistrationModelSsl() {
            return GetUserRegistrationModelLogic();
        }

        #endregion [https calls]

        #region [http calls]

        [HttpPost]
        public JsonResult Register(UserRegistration userRegistrationParams) {
            return RegisterLogic(userRegistrationParams);
        }

        [HttpPost]
        public JsonResult SignIn(UserLogin login) {
            return SignInLogic(login);
        }

        [HttpPost]
        public JsonResult SignOut() {
            return SignOutLogic();
        }

        [HttpPost]
        public JsonResult RequestLostPasswordSms(PhoneNumberViewModel phoneNumber) {
            return RequestLostPasswordLogic(phoneNumber.PhoneNumber, LostPasswordUserOptions.Phone, phoneNumber.InternationalPrefix);
        }

        [HttpPost]
        public JsonResult RequestLostPasswordAccountOrEmail(string username) {
            return RequestLostPasswordLogic(username, LostPasswordUserOptions.Account);
        }

        [HttpGet]
        public ContentResult GetCleanRegistrationPolicies(string lang = null) {
            return GetCleanRegistrationPoliciesLogic(lang);
        }

        [HttpGet]
        public ContentResult GetRegistrationPolicies(string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string lang = null) {
            return GetRegistrationPoliciesLogic(mfilter, page, pageSize, tinyResponse, minified, realformat, deeplevel, lang);
        }

        [HttpGet]
        public JsonResult GetUserRegistrationModel() {
            return GetUserRegistrationModelLogic();
        }

        #endregion [http calls]

        private JsonResult RegisterLogic(UserRegistration userRegistrationParams) {
            Response result;
            // ensure users can request lost password
            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            if (!registrationSettings.UsersCanRegister) {
                result = _utilsServices.GetResponse(ResponseType.None, T("Users cannot register due to site settings.").Text);
                return Json(result);
            }
            try {
                _usersExtensionsServices.Register(userRegistrationParams);
                var registeredServicesData = new {
                    RegisteredServices = _controllerContextAccessor.Context.Controller.TempData
                };
                result = _utilsServices.GetResponse(ResponseType.Success, data: registeredServicesData);
            } catch (Exception ex) {
                result = _utilsServices.GetResponse(ResponseType.None, ex.Message);
            }

            return Json(result);
        }

        private JsonResult SignInLogic(UserLogin login) {
            Response result;
            try {
                _usersExtensionsServices.SignIn(login);
                var registeredServicesData = new {
                    RegisteredServices = _controllerContextAccessor.Context.Controller.TempData
                };
                result = _utilsServices.GetResponse(ResponseType.Success, "", registeredServicesData);
            } catch (Exception ex) {
                result = _utilsServices.GetResponse(ResponseType.InvalidUser, ex.Message);
            }
            return Json(result);
        }

        private JsonResult SignOutLogic() {
            Response result;
            try {
                _usersExtensionsServices.SignOut();
                result = _utilsServices.GetResponse(ResponseType.Success);
            } catch (Exception ex) {
                result = _utilsServices.GetResponse(ResponseType.InvalidUser, ex.Message);
            }
            return Json(result);
        }

        private ContentResult GetCleanRegistrationPoliciesLogic(string lang = null) {
            var sb = new StringBuilder();
            var policies = _usersExtensionsServices.GetUserLinkedPolicies(lang);

            //policy.PendingPolicies
            sb.Insert(0, "{");
            sb.Append("\"Policies\":[");

            int i = 0;

            foreach (var item in policies) {
                if (i > 0) {
                    sb.Append(",");
                }
                sb.Append("{");
                sb.Append("\"PolicyId\":" + item.Id.ToString() + ",");
                sb.Append("\"Title\":\"" + item.ContentItem.As<TitlePart>().Title.Replace("\"", "\\\"") + "\",");
                sb.Append("\"Body\":\"" + item.ContentItem.As<BodyPart>().Text.Replace("\"", "\\\"").Replace("\r\n", "\\r\\n") + "\",");
                sb.Append("\"PolicyType\":\"" + item.PolicyType.ToString() + "\",");
                sb.Append("\"UserHaveToAccept\":" + item.UserHaveToAccept.ToString().ToLowerInvariant() + "");
                sb.Append("}");
                i++;
            }
            sb.Append("]");
            sb.Append("}");
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        private ContentResult GetRegistrationPoliciesLogic(string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true, bool minified = false, bool realformat = false, int deeplevel = 10, string lang = null, string complexBehaviour = "") {
            var sb = new StringBuilder();
            var _filterContentFieldsParts = mfilter.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            
            XElement projectionDump = null;
            // il dump dell'oggetto principale non filtra per field
            ObjectDumper dumper;
            var policies = _usersExtensionsServices.GetUserLinkedPolicies(lang);

            //policy.PendingPolicies
            sb.Insert(0, "{");
            sb.AppendFormat("\"n\": \"{0}\"", "Model");
            sb.AppendFormat(", \"v\": \"{0}\"", "VirtualContent");
            sb.Append(", \"m\": [{");
            sb.AppendFormat("\"n\": \"{0}\"", "VirtualId"); // Unused property for mobile mapper needs
            sb.AppendFormat(", \"v\": \"{0}\"", "0");
            sb.Append("}]");

            sb.Append(", \"l\":[");

            int i = 0;
            sb.Append("{");
            sb.AppendFormat("\"n\": \"{0}\"", "RegistrationPolicies");
            sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
            sb.Append(", \"m\": [");

            foreach (var item in policies) {
                if (i > 0) {
                    sb.Append(",");
                }
                sb.Append("{");
                dumper = new ObjectDumper(deeplevel, _filterContentFieldsParts, false, tinyResponse, complexBehaviour.Split(','));
                projectionDump = dumper.Dump(item.ContentItem, String.Format("[{0}]", i));
                JsonConverter.ConvertToJSon(projectionDump, sb, minified, realformat);
                sb.Append("}");
                i++;
            }
            sb.Append("]");
            sb.Append("}");

            sb.Append("]"); // l : [
            sb.Append("}");
            return new ContentResult { Content = sb.ToString(), ContentType = "application/json" };
        }

        private JsonResult GetUserRegistrationModelLogic() {
            var userRegistration = new UserRegistration {
                Username = "MyUserName",
                Password = "MyPassword",
                ConfirmPassword = "MyPassword",
                PasswordQuestion = "MyPasswordQuestion",
                PasswordAnswer = "MyPasswordAnswer",
                Email = "myname@mydomain.it",
                Culture = "it-IT",
                PolicyAnswers = _usersExtensionsServices.GetUserLinkedPolicies("it-IT").Select(x => new UserPolicyAnswer {
                    PolicyId = x.Id,
                    UserHaveToAccept = x.UserHaveToAccept,
                    PolicyAnswer = false
                }).ToList()
            };
            return Json(userRegistration, JsonRequestBehavior.AllowGet);
        }

        private JsonResult RequestLostPasswordLogic(string username, LostPasswordUserOptions userOptions, string internationalPrefix = null) {
            // ensure users can request lost password
            Response result = _utilsServices.GetResponse(ResponseType.None, T("Send email failed.").Text);
            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            if (!registrationSettings.EnableLostPassword) {
                result = _utilsServices.GetResponse(ResponseType.None, T("Users cannot recover lost password due to site settings.").Text);

                return Json(result);
            }

            if (String.IsNullOrWhiteSpace(username)) {
                result = _utilsServices.GetResponse(ResponseType.None, T("Invalid user.").Text);
                return Json(result);
            }

            var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
            if (String.IsNullOrWhiteSpace(siteUrl)) {
                siteUrl = HttpContext.Request.ToRootUrlString();
            }

            // test if user is user/email or phone number
            if (userOptions == LostPasswordUserOptions.Account) {
                if (_userService.SendLostPasswordEmail(username, nonce => Url.MakeAbsolute(Url.Action("LostPassword", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl))) {
                    result = _utilsServices.GetResponse(ResponseType.Success);
                } else {
                    result = _utilsServices.GetResponse(ResponseType.None, T("Send email failed.").Text);
                }
            } else {
                var sendSmsResult = _usersExtensionsServices.SendLostPasswordSms(internationalPrefix, username, nonce => Url.MakeAbsolute(Url.Action("LostPassword", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl));

                if (sendSmsResult == "TRUE") {
                    result = _utilsServices.GetResponse(ResponseType.Success);
                } else {
                    Dictionary<string, string> errors = new Dictionary<string, string>();
                    errors.Add("BODYEXCEEDED", T("Message rejected: too many characters. (160 max)").ToString()); //"messaggio rigettato per superamento lunghezza max di testo (160 caratteri)");
                    errors.Add("MISSINGPARAMETER_1", T("Missing recipient").ToString()); //"Destinatario mancante");
                    errors.Add("MISSINGPARAMETER_2", T("Sender identifier missing").ToString()); //"Identificativo di invio mancante");
                    errors.Add("MISSINGPARAMETER_3", T("Sender missing or wrong").ToString()); //"Mittente mancante o errato");
                    errors.Add("MISSINGPARAMETER_4", T("Missing text").ToString()); //"Testo mancante");
                    errors.Add("MISSINGPARAMETER_5", T("Priority missing or wrong").ToString()); //"Priorità mancante o errata");
                    errors.Add("FALSE", T("Generic error").ToString()); //"Errore generico");
                    result = _utilsServices.GetResponse(ResponseType.None, T("Send SMS failed.").Text + errors[sendSmsResult].ToString());
                }
            }
            return Json(result);
        }
    }
}