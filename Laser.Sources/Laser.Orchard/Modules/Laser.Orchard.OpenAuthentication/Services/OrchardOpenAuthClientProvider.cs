using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.AspNet.Clients;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Services.Clients;
using Orchard;
using Orchard.Validation;
using Laser.Orchard.OpenAuthentication.Security;

namespace Laser.Orchard.OpenAuthentication.Services {
    public interface IOrchardOpenAuthClientProvider : IDependency {
        IAuthenticationClient GetClient(string providerName);
        OrchardAuthenticationClientData GetClientData(string providerName);
        AuthenticationResult GetUserData(string providerName, AuthenticationResult previosAuthResult, string userAccessToken, string userAccessSecret = "");
        OpenAuthCreateUserParams NormalizeData(string providerName, OpenAuthCreateUserParams userData);
        //void RewriteRequest();
    }

    public class OrchardOpenAuthClientProvider : IOrchardOpenAuthClientProvider {
        private readonly IProviderConfigurationService _providerConfigurationService;
        private readonly IEnumerable<IExternalAuthenticationClient> _openAuthAuthenticationClients;

        public OrchardOpenAuthClientProvider(IProviderConfigurationService providerConfigurationService,
            IEnumerable<IExternalAuthenticationClient> openAuthAuthenticationClients) {
            _providerConfigurationService = providerConfigurationService;
            _openAuthAuthenticationClients = openAuthAuthenticationClients;
        }

        public IAuthenticationClient GetClient(string providerName) {
            Argument.ThrowIfNullOrEmpty(providerName, "providerName");

            // Do we have a configuration?
            var clientConfiguration = _providerConfigurationService.Get(providerName);

            if (clientConfiguration == null)
                return null;

            // Is this a known internal client
            var clientBuilder = _openAuthAuthenticationClients
                .SingleOrDefault(o => o.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

            if (clientBuilder != null)
                return clientBuilder.Build(clientConfiguration);

            return CreateOpenIdClient(clientConfiguration);
        }



        public OrchardAuthenticationClientData GetClientData(string providerName) {
            Argument.ThrowIfNullOrEmpty(providerName, "providerName");

            // Do we have a configuration?
            var clientConfiguration = _providerConfigurationService.Get(providerName);

            if (clientConfiguration == null || clientConfiguration.IsEnabled!=1)
                return null;

            // Is this a known internal client
            var clientBuilder = _openAuthAuthenticationClients
                .SingleOrDefault(o => o.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

            IAuthenticationClient client = clientBuilder != null ?
                clientBuilder.Build(clientConfiguration) : CreateOpenIdClient(clientConfiguration);

            if (client != null)
                return new OrchardAuthenticationClientData(client, clientConfiguration.DisplayName, new Dictionary<string, object>());
            else
                return null;
        }

        public AuthenticationResult GetUserData(string providerName, AuthenticationResult previosAuthResult, string userAccessToken, string userAccessSecret = "") {
            Argument.ThrowIfNullOrEmpty(providerName, "providerName");
            // Do we have a configuration?
            var clientConfiguration = _providerConfigurationService.Get(providerName);

            if (clientConfiguration == null)
                return null;

            // Is this a known internal client
            var client = _openAuthAuthenticationClients
                .SingleOrDefault(o => o.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

            return client.GetUserData(clientConfiguration, previosAuthResult, userAccessToken, userAccessSecret);
        }

        public OpenAuthCreateUserParams NormalizeData(string providerName, OpenAuthCreateUserParams userData) {
            Argument.ThrowIfNullOrEmpty(providerName, "providerName");
            // Do we have a configuration?
            var clientConfiguration = _providerConfigurationService.Get(providerName);

            if (clientConfiguration == null)
                return null;

            // Is this a known internal client
            var client = _openAuthAuthenticationClients
                .SingleOrDefault(o => o.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

            return client.NormalizeData(userData);
        }

        private static IAuthenticationClient CreateOpenIdClient(ProviderConfigurationRecord clientConfiguration) {
            return new CustomOpenIdAuthenticationClient(clientConfiguration.ProviderName).Build(clientConfiguration);
        }
    }
}