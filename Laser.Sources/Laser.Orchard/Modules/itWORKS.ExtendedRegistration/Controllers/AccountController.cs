using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Themes;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;


namespace itWORKS.ExtendedRegistration.Controllers
{
    [Themed]
    public class AccountController : Controller, IUpdateModel
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IUserService _userService;
        private readonly IOrchardServices _orchardServices;
        private readonly IEnumerable<IUserEventHandler> _userEventHandlers;
        //private readonly IShapeFactory shapeFactory;
        private readonly IContentManager _contentManager;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        dynamic Shape { get; set; }

        int MinPasswordLength
        {
            get
            {
                return _membershipService.GetSettings().MinRequiredPasswordLength;
            }
        }

        public AccountController(
          IAuthenticationService authenticationService,
          IMembershipService membershipService,
          IUserService userService,
          IOrchardServices orchardServices,
          IShapeFactory shapeFactory,
          IContentManager contentManager,
          IEnumerable<IUserEventHandler> userEventHandlers)
        {
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _userService = userService;
            _orchardServices = orchardServices;
            _userEventHandlers = userEventHandlers;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            _contentManager = contentManager;
        }

        public ActionResult Register()
        {
            // ensure users can register
            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            if (!registrationSettings.UsersCanRegister)
            {
                return HttpNotFound();
            }

            ViewData["PasswordLength"] = MinPasswordLength;

            var shape = _orchardServices.New.Register();

            var user = _orchardServices.ContentManager.New("User");
            if (user != null)
            {
                shape.UserProfile = _contentManager.BuildEditor(user);
            }

            return new ShapeResult(this, shape);
        }

        [HttpPost]
        public ActionResult Register(string userName, string email, string password, string confirmPassword)
        {
            // ensure users can register
            var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();
            if (!registrationSettings.UsersCanRegister)
            {
                return HttpNotFound();
            }
            ViewData["PasswordLength"] = MinPasswordLength;

            var shape = _orchardServices.New.Register();

            // validate user part, create a temp user part to validate input
            var userPart = _orchardServices.ContentManager.New("User");
            if (userPart != null)
            {
                shape.UserProfile = _orchardServices.ContentManager.UpdateEditor(userPart, this);
                if (!ModelState.IsValid)
                {
                    _orchardServices.TransactionManager.Cancel();
                    return new ShapeResult(this, shape);
                }
            }

            if (ValidateRegistration(userName, email, password, confirmPassword))
            {
                // Attempt to register the user
                // No need to report this to IUserEventHandler because _membershipService does that for us
                var user = _membershipService.CreateUser(new CreateUserParams(userName, password, email, null, null, false));

                if (user != null)
                {
                    // we know userpart data is ok, now we update the 'real' recently published userpart
                    _orchardServices.ContentManager.UpdateEditor(user.ContentItem, this);

                    var userPart2 = user.As<UserPart>();
                    if (user.As<UserPart>().EmailStatus == UserStatus.Pending)
                    {
                        _userService.SendChallengeEmail(user.As<UserPart>(), nonce => Url.AbsoluteAction(() => Url.Action("ChallengeEmail", "Account", new { Area = "Orchard.Users", nonce = nonce })));

                        foreach (var userEventHandler in _userEventHandlers)
                        {
                            userEventHandler.SentChallengeEmail(user);
                        }
                        return RedirectToAction("ChallengeEmailSent", "Account", new { area = "Orchard.Users" });
                    }

                    if (user.As<UserPart>().RegistrationStatus == UserStatus.Pending)
                    {
                        return RedirectToAction("RegistrationPending", "Account", new { area = "Orchard.Users" });
                    }

                    _authenticationService.SignIn(user, false /* createPersistentCookie */);
                    return Redirect("~/");
                }

                ModelState.AddModelError("_FORM", T(ErrorCodeToString(/*createStatus*/MembershipCreateStatus.ProviderError)));
            }

            // If we got this far, something failed, redisplay form
            //var shape = _orchardServices.New.Register();
            return new ShapeResult(this, shape);
        }

        private bool ValidateRegistration(string userName, string email, string password, string confirmPassword)
        {
            bool validate = true;

            if (String.IsNullOrEmpty(userName))
            {
                ModelState.AddModelError("username", T("You must specify a username."));
                validate = false;
            }

            if (String.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("email", T("You must specify an email address."));
                validate = false;
            }
            else if (!Regex.IsMatch(email, UserPart.EmailPattern, RegexOptions.IgnoreCase))
            {
                // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                ModelState.AddModelError("email", T("You must specify a valid email address."));
                validate = false;
            }

            if (!validate)
                return false;

            if (!_userService.VerifyUserUnicity(userName, email))
            {
                ModelState.AddModelError("userExists", T("User with that username and/or email already exists."));
            }
            if (password == null || password.Length < MinPasswordLength)
            {
                ModelState.AddModelError("password", T("You must specify a password of {0} or more characters.", MinPasswordLength));
            }
            if (!String.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError("_FORM", T("The new password and confirmation password do not match."));
            }
            return ModelState.IsValid;
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://msdn.microsoft.com/en-us/library/system.web.security.membershipcreatestatus.aspx for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "Username already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A username for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return
                        "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return
                        "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return
                        "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }

}