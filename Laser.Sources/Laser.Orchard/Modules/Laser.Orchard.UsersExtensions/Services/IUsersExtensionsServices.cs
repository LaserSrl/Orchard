﻿using Laser.Orchard.Mobile.Handlers;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.Policy.Models;
using Laser.Orchard.Policy.Services;
using Laser.Orchard.Policy.ViewModels;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.UsersExtensions.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Localization.Records;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.Utility.Extensions;
using Orchard.Mvc.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.UsersExtensions.Services {
    public interface IUsersExtensionsServices : IDependency {
        void Register(UserRegistration userRegistrationParams);
        void SignIn(UserLogin userLoginParams);
        void SignOut();
        IEnumerable<PolicyTextInfoPart> GetUserLinkedPolicies(string culture = null);
        bool ValidateRegistration(string userName, string email, string password, string confirmPassword, out List<string> errors);
        IList<UserPolicyAnswerWithContent> BuildEditorForRegistrationPolicies();
        string SendLostPasswordSms(string internationalPrefix, string phoneNumber, Func<string, string> createUrl);
        UserPart GetUserByMail(string mail);
    }


    public class UsersExtensionsServices : IUsersExtensionsServices {
        private readonly IOrchardServices _orchardServices;
        private readonly IPolicyServices _policySerivces;
        private readonly IMembershipService _membershipService;
        private readonly IUtilsServices _utilsServices;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserService _userService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IShapeFactory _shapeFactory;
        private ISmsServices _smsServices;
        private readonly ICultureManager _cultureManager;

        private static readonly TimeSpan DelayToResetPassword = new TimeSpan(1, 0, 0, 0); // 24 hours to reset password
        private readonly IRepository<CultureRecord> _repositoryCultures;


        public UsersExtensionsServices(IOrchardServices orchardServices, IPolicyServices policySerivces, IMembershipService membershipService, IUtilsServices utilsServices, IAuthenticationService authenticationService, IUserService userService, IUserEventHandler userEventHandler, IShapeFactory shapeFactory, ICultureManager cultureManager, IRepository<CultureRecord> repositoryCultures) {
            T = NullLocalizer.Instance;
            Log = NullLogger.Instance;
            _policySerivces = policySerivces;
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _utilsServices = utilsServices;
            _userService = userService;
            _userEventHandler = userEventHandler;
            _shapeFactory = shapeFactory;
            _cultureManager = cultureManager;
            _repositoryCultures = repositoryCultures;
        }

        public Localizer T { get; set; }

        private ILogger Log { get; set; }

        int MinPasswordLength {
            get {
                return _membershipService.GetSettings().GetMinimumPasswordLength();
            }
        }

        public void Register(UserRegistration userRegistrationParams) {
            if (RegistrationSettings.UsersCanRegister) {
                var policyAnswers = new List<PolicyForUserViewModel>();
                if (_utilsServices.FeatureIsEnabled("Laser.Orchard.Policy") && UserRegistrationExtensionsSettings.IncludePendingPolicy == Policy.IncludePendingPolicyOptions.Yes) {
                    IEnumerable<PolicyTextInfoPart> policies = GetUserLinkedPolicies(userRegistrationParams.Culture);
                    // controllo che tutte le policy abbiano una risposta e che le policy obbligatorie siano accettate 
                    var allRight = true;
                    foreach (var policy in policies) {
                        var policyId = policy.Id;
                        var policyRequired = policy.UserHaveToAccept;
                        var answer = userRegistrationParams.PolicyAnswers.Where(w => w.PolicyId == policyId).SingleOrDefault();
                        if (answer != null) {
                            if (!answer.PolicyAnswer && policyRequired) {
                                allRight = false;
                            }
                        }
                        else if (answer == null && policyRequired) {
                            allRight = false;
                        }
                        if (answer != null) {
                            policyAnswers.Add(new PolicyForUserViewModel {
                                OldAccepted = false,
                                PolicyTextId = policyId,
                                Accepted = answer.PolicyAnswer,
                                AnswerDate = DateTime.Now
                            });
                        }
                    }
                    if (!allRight) {
                        throw new SecurityException(T("User has to accept policies!").Text);
                    }
                }
                var registrationErrors = new List<string>();
                if (ValidateRegistration(userRegistrationParams.Username, userRegistrationParams.Email, userRegistrationParams.Password, userRegistrationParams.ConfirmPassword, out registrationErrors)) {
                    var createdUser = _membershipService.CreateUser(new CreateUserParams(
                        userRegistrationParams.Username,
                        userRegistrationParams.Password,
                        userRegistrationParams.Email,
                        userRegistrationParams.PasswordQuestion,
                        userRegistrationParams.PasswordAnswer,
                        (RegistrationSettings.UsersAreModerated == false) && (RegistrationSettings.UsersMustValidateEmail == false)
                        ));
                    var favCulture = createdUser.As<FavoriteCulturePart>();
                    if (favCulture != null) {
                        var culture = _repositoryCultures.Fetch(x => x.Culture.Equals(userRegistrationParams.Culture)).SingleOrDefault();
                        if (culture != null) {
                            favCulture.Culture_Id = culture.Id;
                        }
                        else {
                            // usa la culture di default del sito
                            favCulture.Culture_Id = _cultureManager.GetCultureByName(_cultureManager.GetSiteCulture()).Id;
                        }
                    }
                    if ((RegistrationSettings.UsersAreModerated == false) && (RegistrationSettings.UsersMustValidateEmail == false)) {
                        _authenticationService.SignIn(createdUser, true);

                        // solleva l'evento LoggedIn sull'utente
                        _userEventHandler.LoggedIn(createdUser);
                    }
                    
                    // [HS] BEGIN: Whe have to save the PoliciesAnswers cookie and persist answers on the DB after Login/SignIn events because during Login/Signin events database is not updated yet and those events override cookie in an unconsistent way.
                    if (_utilsServices.FeatureIsEnabled("Laser.Orchard.Policy") && UserRegistrationExtensionsSettings.IncludePendingPolicy == Policy.IncludePendingPolicyOptions.Yes) {
                        _policySerivces.PolicyForUserMassiveUpdate(policyAnswers, createdUser);
                    }
                    // [HS] END

                    if (RegistrationSettings.UsersMustValidateEmail) {
                        // send challenge e-mail
                        var siteUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
                        if (string.IsNullOrWhiteSpace(siteUrl)) {
                            siteUrl = HttpContext.Current.Request.ToRootUrlString();
                        }
                        UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                        _userService.SendChallengeEmail(createdUser, nonce => urlHelper.MakeAbsolute(urlHelper.Action("ChallengeEmail", "Account", new { Area = "Orchard.Users", nonce = nonce }), siteUrl));
                    }
                } else {
                    throw new SecurityException(String.Join(", ", registrationErrors));
                }
            } else {
                throw new SecurityException(T("User cannot register due to Site settings").Text);
            }
        }

        public void SignIn(UserLogin userLoginParams) {
            var user = _membershipService.ValidateUser(userLoginParams.Username, userLoginParams.Password);
            if (user != null) {
                _authenticationService.SignIn(user, true);
                _userEventHandler.LoggedIn(user);
            } else {
                throw new SecurityException(T("The username or e-mail or password provided is incorrect.").Text);
            }
        }

        public void SignOut() {
            _authenticationService.SignOut();
        }

        public string SendLostPasswordSms(string internationalPrefix, string phoneNumber, Func<string, string> createUrl) {
            _orchardServices.WorkContext.TryResolve<ISmsServices>(out _smsServices);
            if (_smsServices == null) return "FALSE";

            var user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>()
                .Join<UserPwdRecoveryPartRecord>()
                .Where(u => u.InternationalPrefix == internationalPrefix.ToString() && u.PhoneNumber == phoneNumber.ToString())
                .List().FirstOrDefault();

            if (user != null) {
                string nonce = _userService.CreateNonce(user, DelayToResetPassword);
                string url = createUrl(nonce);

                //var template = _shapeFactory.Create("Template_User_LostPassword", Arguments.From(new {
                //    User = user,
                //    LostPasswordUrl = url
                //}));
                //template.Metadata.Wrappers.Add("Template_User_Wrapper");

                //var parameters = new Dictionary<string, object> {
                //            {"Subject", T("Lost password").Text},
                //            {"Body", _shapeDisplay.Display(template)},
                //            {"Recipients", user.Email }
                //        };
                var smsSettings = _orchardServices.WorkContext.CurrentSite.As<SmsSettingsPart>();

                long phoneNumberComplete = 0;
                if (long.TryParse(String.Concat(internationalPrefix.Trim(), phoneNumber.Trim()), out phoneNumberComplete)) {
                    //return _smsServices.SendSms(new long[] { phoneNumberComplete }, user.UserName + "\r\n" + url);

                    Hashtable hs = new Hashtable();
                    hs.Add("SmsContactNumber", phoneNumberComplete);

                    List<Hashtable> listaDestinatari = new List<Hashtable>();
                    listaDestinatari.Add(hs);

                    return _smsServices.SendSms(listaDestinatari, user.UserName + "\r\n" + url);
                }
            }

            return "FALSE";
        }

        public IEnumerable<PolicyTextInfoPart> GetUserLinkedPolicies(string culture = null) {
            IEnumerable<PolicyTextInfoPart> policies;
            if (UserRegistrationExtensionsSettings.IncludePendingPolicy == Policy.IncludePendingPolicyOptions.No) return new List<PolicyTextInfoPart>(); // se selezionato No allora nessuna policy è obbligatoria e ritorno una collection vuota
            if (UserRegistrationExtensionsSettings.PolicyTextReferences.FirstOrDefault() == null || UserRegistrationExtensionsSettings.PolicyTextReferences.FirstOrDefault() == "{All}") {
                policies = _policySerivces.GetPolicies(culture);
            } else {
                var ids = UserRegistrationExtensionsSettings.PolicyTextReferences.Select(x => Convert.ToInt32(x.Replace("{", "").Replace("}", ""))).ToArray();
                policies = _policySerivces.GetPolicies(culture, ids);
            }
            return policies;
        }

        public bool ValidateRegistration(string userName, string email, string password, string confirmPassword, out List<string> errors) {
            bool validate = true;
            errors = new List<string>();
            if (String.IsNullOrEmpty(userName)) {
                errors.Add(T("You must specify a username.").Text);
                validate = false;
            } else {
                if (userName.Length >= 255) {
                    errors.Add(T("The username you provided is too long.").Text);
                    validate = false;
                }
            }

            if (String.IsNullOrEmpty(email)) {
                errors.Add(T("You must specify an email address.").Text);
                validate = false;
            } else if (email.Length >= 255) {
                errors.Add(T("The email address you provided is too long.").Text);
                validate = false;
            } else if (!Regex.IsMatch(email, UserPart.EmailPattern, RegexOptions.IgnoreCase)) {
                // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                errors.Add(T("You must specify a valid email address.").Text);
                validate = false;
            }

            if (!validate)
                return false;

            if (!_userService.VerifyUserUnicity(userName, email)) {
                errors.Add(T("User with that username and/or email already exists.").Text);
            }
            if (password == null || password.Length < MinPasswordLength) {
                errors.Add(T("You must specify a password of {0} or more characters.", MinPasswordLength).Text);
            }
            if (!String.Equals(password, confirmPassword, StringComparison.Ordinal)) {
                errors.Add(T("The new password and confirmation password do not match.").Text);
            }
            return errors.Count == 0;
        }

        public IList<UserPolicyAnswerWithContent> BuildEditorForRegistrationPolicies() {
            var policies = GetUserLinkedPolicies().Select(x => new UserPolicyAnswerWithContent {
                PolicyAnswer = false,
                PolicyId = x.Id,
                UserHaveToAccept = x.UserHaveToAccept,
                PolicyText = x.ContentItem
            }).ToList();
            return policies;
        }
        public UserPart GetUserByMail(string mail) {
            var qry = _orchardServices.ContentManager.Query("User").Where<UserPartRecord>(x => x.Email == mail);
            var usr = qry.Slice(0, 1).FirstOrDefault();
            if(usr != null) {
                return usr.As<UserPart>();
            }
            return null;
        }

        private RegistrationSettingsPart RegistrationSettings {
            get {
                var orchardUsersSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
                return orchardUsersSettings;
            }
        }

        private UserRegistrationSettingsPart UserRegistrationExtensionsSettings {
            get {

                var orchardUsersSettings = _orchardServices.WorkContext.CurrentSite.As<UserRegistrationSettingsPart>();
                return orchardUsersSettings;
            }
        }

    }
}