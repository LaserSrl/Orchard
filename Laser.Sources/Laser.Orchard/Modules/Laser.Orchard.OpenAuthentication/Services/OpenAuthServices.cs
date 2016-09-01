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
using Laser.Orchard.StartupConfig.Handlers;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IOpenAuthMembershipServices : IDependency {
        bool CanRegister();
        IUser CreateUser(OpenAuthCreateUserParams createUserParams);
    }

    public class OpenAuthMembershipServices : IOpenAuthMembershipServices {
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        private readonly IUsernameService _usernameService;
        private readonly IPasswordGeneratorService _passwordGeneratorService;
        private readonly IEnumerable<IOpenAuthUserEventHandler> _openAuthUserEventHandlers;
        private readonly IContactRelatedEventHandler _contactEventHandler;

        public OpenAuthMembershipServices(IOrchardServices orchardServices,
            IMembershipService membershipService,
            IUsernameService usernameService,
            IPasswordGeneratorService passwordGeneratorService,
            IEnumerable<IOpenAuthUserEventHandler> openAuthUserEventHandlers,
            IContactRelatedEventHandler contactEventHandler) {
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _usernameService = usernameService;
            _passwordGeneratorService = passwordGeneratorService;
            _openAuthUserEventHandlers = openAuthUserEventHandlers;
            _contactEventHandler = contactEventHandler;
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

        public IUser CreateUser(OpenAuthCreateUserParams createUserParams) {
            string emailAddress = string.Empty;
            if (createUserParams.UserName.IsEmailAddress()) {
                emailAddress = createUserParams.UserName;
            }
            else {
                foreach (var key in createUserParams.ExtraData.Keys) {
                    switch (key.ToLower()) {
                        case "mail":
                        case "email":
                        case "e-mail":
                        case "email-address":
                            emailAddress = createUserParams.ExtraData[key];
                            break;
                    }
                }
            }

            createUserParams.UserName = _usernameService.Normalize(createUserParams.UserName);
            var creatingContext = new CreatingOpenAuthUserContext(createUserParams.UserName, emailAddress, createUserParams.ProviderName, createUserParams.ProviderUserId, createUserParams.ExtraData);

            _openAuthUserEventHandlers.Invoke(o => o.Creating(creatingContext), Logger);

            var createdUser = _membershipService.CreateUser(new CreateUserParams(
                _usernameService.Calculate(createUserParams.UserName),
                _passwordGeneratorService.Generate(),
                creatingContext.EmailAddress,
                @T("Auto Registered User").Text,
                _passwordGeneratorService.Generate() /* Noone can guess this */,
                true
                ));

            var createdContext = new CreatedOpenAuthUserContext(createdUser, createUserParams.ProviderName, createUserParams.ProviderUserId, createUserParams.ExtraData);
            _openAuthUserEventHandlers.Invoke(o => o.Created(createdContext), Logger);

            //solleva l'evento di sincronizzazione dell'utente orchard
            _contactEventHandler.Synchronize(createdUser);

            return createdUser;
        }
    }
}