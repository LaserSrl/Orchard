﻿using System;
using Laser.Orchard.OpenAuthentication.Services;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.ContentManagement;
using Orchard.UI.Notify;
using Orchard.Localization;

namespace Laser.Orchard.OpenAuthentication.Events {
    public class OpenAuthUserEventHandler : IUserEventHandler {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrchardOpenAuthWebSecurity _orchardOpenAuthWebSecurity;
        private readonly INotifier _notifier;
        private readonly IAuthenticationService _authenticationService;

        public OpenAuthUserEventHandler(IHttpContextAccessor httpContextAccessor,
            IOrchardOpenAuthWebSecurity orchardOpenAuthWebSecurity,
            IAuthenticationService authenticationService,
            INotifier notifier) {
            _httpContextAccessor = httpContextAccessor;
            _orchardOpenAuthWebSecurity = orchardOpenAuthWebSecurity;
            _authenticationService = authenticationService;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Creating(UserContext context) {
        }

        public void Created(UserContext context) {
            CreateOrUpdateOpenAuthUser(context.User);
        }

        public void LoggedIn(IUser user) {
            CreateOrUpdateOpenAuthUser(user);
        }

        private void CreateOrUpdateOpenAuthUser(IUser user) {
            var current = _httpContextAccessor.Current();
            if (current == null)
                return;

            var request = current.Request;

            if (request == null)
                return;

            var userName = request.QueryString["UserName"];
            var externalLoginData = request.QueryString["ExternalLoginData"];

            if (string.IsNullOrWhiteSpace(userName) ||
                string.IsNullOrWhiteSpace(externalLoginData))
                return;

            string providerName;
            string providerUserId;

            if (
                !_orchardOpenAuthWebSecurity.TryDeserializeProviderUserId(externalLoginData, out providerName,
                                                                          out providerUserId))
                return;

            _orchardOpenAuthWebSecurity.CreateOrUpdateAccount(providerName, providerUserId, user);
        }

        public void LoggedOut(IUser user) {
        }

        public void AccessDenied(IUser user) {
        }

        public void ChangedPassword(IUser user) {
        }

        public void SentChallengeEmail(IUser user) {
        }

        public void ConfirmedEmail(IUser user) {
        }

        public void Approved(IUser user) {
            var userPart = user.ContentItem.As<UserPart>();
            if (userPart.LastLoginUtc!= null) return;

            var closestUser = _orchardOpenAuthWebSecurity.GetClosestMergeableKnownUser(userPart);
            if (closestUser.UserName != user.UserName) {
                closestUser.ContentItem.As<UserPart>().Password = userPart.Password;
                closestUser.ContentItem.As<UserPart>().PasswordFormat = userPart.PasswordFormat;
                closestUser.ContentItem.As<UserPart>().PasswordSalt = userPart.PasswordSalt;
                closestUser.ContentItem.As<UserPart>().UserName = userPart.UserName;
                closestUser.ContentItem.As<UserPart>().NormalizedUserName = userPart.NormalizedUserName;
                closestUser.ContentItem.As<UserPart>().HashAlgorithm= userPart.HashAlgorithm;
                userPart.RegistrationStatus = UserStatus.Pending;
                _notifier.Information(T("Approved account has been merged with previous account."));
                if (_authenticationService.GetAuthenticatedUser()==null) { // TODO: to specialize behaviour based on caller (registration, approved back-end, approved front-end
                    _authenticationService.SignIn(closestUser, false);
                    LoggedIn(closestUser);
                }
            }
        }

        public void LoggingIn(string userNameOrEmail, string password) {
        }

        public void LogInFailed(string userNameOrEmail, string password) {
        }
    }
}