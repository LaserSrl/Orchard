using DotNetOpenAuth.AspNet;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Security;
using Orchard;

namespace Laser.Orchard.OpenAuthentication.Services.Clients {
    public interface IExternalAuthenticationClient : IDependency {
        string ProviderName { get; }
        
        IAuthenticationClient Build(ProviderConfigurationRecord providerConfigurationRecord);

        AuthenticationResult GetUserData(ProviderConfigurationRecord clientConfiguration, AuthenticationResult previousAuthResult, string userAccessToken, string userAccessSecretKey = "");
        
        OpenAuthCreateUserParams NormalizeData(OpenAuthCreateUserParams clientData);

        bool RewriteRequest();
    }
}