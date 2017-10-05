using Contrib.Profile.Services;

namespace Laser.Orchard.StartupConfig.Services {
    public class FavoriteCultureDefaultFrontEndSettingsProvider : DefaultFrontEndSettingsProviderBase {
        public FavoriteCultureDefaultFrontEndSettingsProvider()
                : base("FavoriteCulturePart", true, true) { }
    }
}