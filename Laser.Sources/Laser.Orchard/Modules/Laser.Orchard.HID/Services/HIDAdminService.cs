using Laser.Orchard.HID.Extensions;
using Laser.Orchard.HID.Models;
using Laser.Orchard.HID.ViewModels;
using Orchard;
using Orchard.Caching.Services;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Configuration;
using System;
using System.Linq;

namespace Laser.Orchard.HID.Services {
    public class HIDAdminService : IHIDAdminService {

        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<HIDPartNumberSet> _repository;
        private readonly ShellSettings _shellSetting;
        private readonly ICacheStorageProvider _cacheStorageProvider;

        public HIDAdminService(
            IOrchardServices orchardServices,
            IRepository<HIDPartNumberSet> repository,
            ICacheStorageProvider cacheStorageProvider,
            ShellSettings shellSetting) {

            _orchardServices = orchardServices;
            _repository = repository;
            _cacheStorageProvider = cacheStorageProvider;
            _shellSetting = shellSetting;
        }

        public HIDSiteSettingsViewModel GetSiteSettings() {
            var settings = _orchardServices.WorkContext.CurrentSite.As<HIDSiteSettingsPart>();
            var vm = new HIDSiteSettingsViewModel(settings) {
                PartNumberSets = _repository.Table.Select(pns => new HIDPartNumberSetViewModel(pns)).ToList()
            };
            if (!vm.PartNumberSets.Any()) {
                vm.PartNumberSets.Add(new HIDPartNumberSetViewModel(HIDPartNumberSet.DefaultEmptySet()));
            }
            return vm;
        }

        public AuthenticationErrors Authenticate() {
            var token = HIDAuthToken.Authenticate(this);
            switch (token.Error) {
                case AuthenticationErrors.NoError:
                    AuthTokenToCache(token); // store token
                    break;
                case AuthenticationErrors.NotAuthenticated:
                    break;
                case AuthenticationErrors.ClientInfoInvalid:
                    break;
                case AuthenticationErrors.CommunicationError:
                    break;
                default:
                    break;
            }
            return token.Error;
        }

        public bool VerifyAuthentication() {
            if (string.IsNullOrWhiteSpace(AuthorizationToken)) {
                if (Authenticate() != AuthenticationErrors.NoError) {
                    return false;
                }
            }
            return true; // authentication ok
        }

        private string BaseURI {
            get {
                return String.Format(HIDAPIEndpoints.BaseURIFormat,
              GetSiteSettings().UseTestEnvironment ? HIDAPIEndpoints.BaseURITest : HIDAPIEndpoints.BaseURIProd);
            }
        }

        public string BaseEndpoint {
            get { return String.Format(HIDAPIEndpoints.CustomerURIFormat, BaseURI, GetSiteSettings().CustomerID.ToString()); }
        }

        #region Token is in the cache
        private string CacheTokenTypeKey {
            get { return string.Format(Constants.CacheTokenTypeKeyFormat, _shellSetting.Name); }
        }
        private string CacheAccessTokenKey {
            get { return string.Format(Constants.CacheAccessTokenKeyFormat, _shellSetting.Name); }
        }
        private void AuthTokenToCache(HIDAuthToken token) {
            TimeSpan validity = TimeSpan.FromSeconds(token.ExpiresIn * 0.8); //80% of what is given by the authentication, to have margin
            _cacheStorageProvider.Put(CacheTokenTypeKey, token.TokenType, validity);
            _cacheStorageProvider.Put(CacheAccessTokenKey, token.AccessToken, validity);
        }
        #endregion

        /// <summary>
        /// Get the full authorization token from cache.
        /// </summary>
        public string AuthorizationToken {
            get {
                string tokenType = (string)_cacheStorageProvider.Get<string>(CacheTokenTypeKey);
                string accessToken = (string)_cacheStorageProvider.Get<string>(CacheAccessTokenKey);
                if (!string.IsNullOrWhiteSpace(tokenType) && !string.IsNullOrWhiteSpace(accessToken)) {
                    return tokenType + " " + accessToken;
                }
                return ""; //TODO: regenerate token here
            }
        }

        public string UsersEndpoint {
            get { return String.Format(HIDAPIEndpoints.UsersEndpointFormat, BaseEndpoint); }
        }

        public string ExternalIdFormat {
            get {
                return "Laser."
                    + _shellSetting.Name
                    + ".{0}";
            }
        }
    }
}