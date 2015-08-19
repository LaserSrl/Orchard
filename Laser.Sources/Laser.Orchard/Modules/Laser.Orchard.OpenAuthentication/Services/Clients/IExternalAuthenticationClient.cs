using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Models;
using Orchard;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public interface IExternalAuthenticationClient : IDependency {
        string ProviderName { get; }
        IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord);
        AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, string userAccessToken, string userAccessSecretKey="");
    }
}