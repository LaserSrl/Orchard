using System.Collections.Generic;
using Laser.Orchard.OpenAuthentication.Events;
using Laser.Orchard.OpenAuthentication.Extensions;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Users.Models;
using System.Collections;
using System.Text.RegularExpressions;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IOpenAuthMembershipServices : IDependency {
        bool CanRegister();
        UserAccountLogin CreateUser(OpenAuthCreateUserParams createUserParams);
    }

    public class OpenAuthMembershipServices : IOpenAuthMembershipServices {
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        private readonly IUsernameService _usernameService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;
        private readonly IEnumerable<IOpenAuthUserEventHandler> _openAuthUserEventHandlers;

        public OpenAuthMembershipServices(IOrchardServices orchardServices,
            IMembershipService membershipService,
            IUsernameService usernameService,
            IPasswordGeneratorService passwordGeneratorService,
            IEnumerable<IOpenAuthUserEventHandler> openAuthUserEventHandlers) {
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _usernameService = usernameService;
            _passwordGeneratorService = passwordGeneratorService;
            _openAuthUserEventHandlers = openAuthUserEventHandlers;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public bool CanRegister() {
            var openAuthenticationSettings = _orchardServices.WorkContext.CurrentSite.As<OpenAuthenticationSettingsPart>();
            var orchardUsersSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();

            return orchardUsersSettings.UsersCanRegister && openAuthenticationSettings.AutoRegistrationEnabled;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="createUserParams"></param>
        /// <returns></returns>
        public UserAccountLogin CreateUser(OpenAuthCreateUserParams createUserParams) {
                   
            string emailAddress = string.Empty;
            string name = string.Empty;
            string firstname = string.Empty;
            string username = string.Empty;
            string sesso = string.Empty;

          
             foreach (KeyValuePair<string, string> values in createUserParams.ExtraData) {

                 if (createUserParams.ProviderName.ToLower() == "facebook") 
                 {
                    //if (values.Key == "username") 
                    //{
                    //    username = values.Value.IsEmailAddress() ? values.Value.Substring(0, values.Value.IndexOf('@')) : values.Value;
                    //    createUserParams.UserName = username;
                    //}

                    if (values.Key == "email" || values.Key =="mail") {
                        emailAddress = values.Value.IsEmailAddress() ? values.Value : "";
                        username = values.Value.IsEmailAddress() ? values.Value.Substring(0, values.Value.IndexOf('@')) : values.Value;
                        createUserParams.UserName = username;
                    }

                    if (values.Key == "gender") {
                        sesso = values.Value;
                    }

                    if (values.Key == "name") {
                        name = values.Value;
                    }

                    if (values.Key == "firstname") {
                        firstname = values.Value;
                    } 
                 }

                 if (createUserParams.ProviderName.ToLower() == "google") {
                     if (values.Key == "email") {
                         emailAddress = values.Value.IsEmailAddress() ? values.Value : "";
                         username = values.Value.IsEmailAddress() ? values.Value.Substring(0, values.Value.IndexOf('@')) : values.Value;
                         createUserParams.UserName = username;
                     }
                 }

                 if (createUserParams.ProviderName.ToLower() == "linkedin") {
                     if (values.Key == "email-address") {
                         emailAddress = values.Value.IsEmailAddress() ? values.Value : "";
                     }
                 }
            }

            if (createUserParams.ProviderName.ToLower() == "twitter")
                username = createUserParams.UserName;

            var creatingContext = new CreatingOpenAuthUserContext(createUserParams.UserName, emailAddress, createUserParams.ProviderName, createUserParams.ProviderUserId, createUserParams.ExtraData);

            _openAuthUserEventHandlers.Invoke(o => o.Creating(creatingContext), Logger);

            var createdUser = _membershipService.CreateUser(new CreateUserParams(
                _usernameService.Calculate(createUserParams.UserName, createUserParams.ProviderName.ToLower().ToString()),
               // createUserParams.UserName,
                _passwordGeneratorService.Generate(),
                creatingContext.EmailAddress,
                @T("Auto Registered User").Text,
                _passwordGeneratorService.Generate() /* Noone can guess this */,
                true
                ));

            var createdContext = new CreatedOpenAuthUserContext(createdUser, createUserParams.ProviderName, createUserParams.ProviderUserId, createUserParams.ExtraData);
            _openAuthUserEventHandlers.Invoke(o => o.Created(createdContext), Logger);

            UserAccountLogin retVal = new UserAccountLogin();
            retVal.Id = createdUser.Id;
            retVal.IUserParz = createdUser;
            retVal.Email = createdUser.Email;
            retVal.FirstName = firstname;
            retVal.Name = name;
            retVal.UserName = username;
            retVal.Sesso = sesso;
            retVal.UserNameOrchard = createdUser.UserName;
            return retVal;
        }
    }
}