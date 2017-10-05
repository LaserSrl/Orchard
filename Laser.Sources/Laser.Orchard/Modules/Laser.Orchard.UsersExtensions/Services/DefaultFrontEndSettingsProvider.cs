using Contrib.Profile.Services;

namespace Laser.Orchard.UsersExtensions.Services {
    public class DefaultFrontEndSettingsProvider : DefaultFrontEndSettingsProviderBase {
        public DefaultFrontEndSettingsProvider()
                : base("UserRegistrationPolicyPart", false, true) {}
    }
}