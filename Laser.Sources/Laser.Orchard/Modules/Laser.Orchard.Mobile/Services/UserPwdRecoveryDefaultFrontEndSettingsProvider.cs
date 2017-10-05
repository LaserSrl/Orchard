using Contrib.Profile.Services;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Mobile.Services {
    [OrchardFeature("Laser.Orchard.Sms")]
    public class UserPwdRecoveryDefaultFrontEndSettingsProvider : DefaultFrontEndSettingsProviderBase {
        public UserPwdRecoveryDefaultFrontEndSettingsProvider()
                : base("UserPwdRecoveryPart", false, true) { }
    }
}