using Orchard.Security.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Security;
using Orchard.Settings;
using Orchard.ContentManagement;
using Laser.Orchard.Mobile.Models;
using Orchard;
using Orchard.Users.Models;

namespace Laser.Orchard.Mobile.Services {
    public class LastDeviceUserDataProvider : BaseUserDataProvider {

        private readonly ISiteService _siteService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public LastDeviceUserDataProvider(
            ISiteService siteService,
            IWorkContextAccessor workContextAccessor) 
            : base(true) {

            // base(true) sets base.DefaultValid = true, but we are going to override the
            // IsValid behaviour anyway, as well as the DefaultValid value

            _siteService = siteService;
            _workContextAccessor = workContextAccessor;
        }

        protected override bool DefaultValid {
            get {
                return !AuthenticateOnlyLatestUUID;
            }
        }

        private bool AuthenticateOnlyLatestUUID {
            get {
                return _siteService
                  .GetSiteSettings()
                  .As<LastDeviceUserDataProviderSettingsPart>()
                  .AuthenticateOnlyLatestUUID;
            }
        }

        protected override string Value(IUser user) {
            // This will be used to provide the value to put in the UserData dictionary
            // upon user's SignIn

            var callUUID = GetCallUUID();

            if (!string.IsNullOrWhiteSpace(callUUID)) {
                // TODO: register the latest UUID
                return callUUID;
            }
            // TODO: decide what whould be done here
            return null;
            // null values are not inserted in the UserDataDictionary
        }

        public override bool IsValid(IUser user, IDictionary<string, string> userData) {
            var callUUID = GetCallUUID();
            var latestUUID = GetLatestUUID(user);
            var userDataUUID = GetUserDataUUID(userData);

            if (AuthenticateOnlyLatestUUID) {

            } else {

            }
            // Logic here should be:
            // If the setting tells that we should not consider UUID => return true;
            // If userData does not contain the key for the element

            return DefaultValid || base.IsValid(user, userData);
        }

        /// <summary>
        /// Fetch the UUID the user used in its latest SignIn
        /// </summary>
        /// <param name="user">The user w are considering</param>
        /// <returns>The UUID, or null if it does not exist.</returns>
        private string GetLatestUUID(IUser user) {
            var userPart = user.As<UserPart>();
            if (userPart == null) {
                return null;
            }
            var record = userPart.Record;

            // TODO
            return string.Empty;
        }

        /// <summary>
        /// Fetch the UUID recorded in the UserData dictionary of the authentication cookie.
        /// </summary>
        /// <param name="userData">The UserData dictionary from the authentication cookie.</param>
        /// <returns>The UUID, or null if it does not exist.</returns>
        private string GetUserDataUUID(IDictionary<string, string> userData) {
            if (!userData.ContainsKey(Key)) {
                return null;
            }
            return userData[Key];
        }

        /// <summary>
        /// Fetch the UUID passed with the current request
        /// </summary>
        /// <returns>The UUID, or null if it does not exist.</returns>
        private string GetCallUUID() {
            var request = _workContextAccessor.GetContext()
                .HttpContext
                .Request;

            if (request.Headers["x-uuid"] != null) {
                return request.Headers["x-uuid"].ToString();
            }
            
            // if the UUID is not in the header, see if it is in query string 

            return null;
        }

        #region Response templates for IsValid()
        private bool CookieTamperedWith() {
            return false;
        }
        private bool OldSignIn() {
            return true;
        }
        private bool WrongDevice() {
            return false;
        }
        private bool NeverRegisteredLogin() {
            return false;
        }
        private bool Valid() {
            return true;
        }
        #endregion
    }
}