using Laser.Orchard.HID.ViewModels;
using Orchard;

namespace Laser.Orchard.HID.Services {
    public interface IHIDAdminService : IDependency {
        
        /// <summary>
        /// Gets an object describing the HID configuration on the tenant
        /// </summary>
        /// <returns>An object describing the HID configuration on the tenant</returns>
        HIDSiteSettingsViewModel GetSiteSettings();
        
        /// <summary>
        /// Attempts authentication to the HID services, using the login information from the settings.
        /// </summary>
        /// <returns>A value identifying possible errors, or NoError in case of success.</returns>
        AuthenticationErrors Authenticate();
        
        /// <summary>
        /// This method verifies whether we are authenticated with HID's systems. If not, it attempts to
        /// authenticate once.
        /// </summary>
        /// <returns>True if authentication is successful, false otherwise.</returns>
        bool VerifyAuthentication();
        
        string AuthorizationToken { get; }

        string BaseEndpoint { get; }

        string UsersEndpoint { get; }

        /// <summary>
        /// Used to compute an HIDUser's externalId
        /// </summary>
        string ExternalIdFormat { get; }
    }
}
