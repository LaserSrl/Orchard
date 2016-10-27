using System.Collections.Generic;
using Laser.Orchard.OpenAuthentication.Models;

namespace Laser.Orchard.OpenAuthentication.ViewModels {
    public class IndexViewModel {
        public bool AutoRegistrationEnabled { get; set; }

        public IEnumerable<ProviderConfigurationRecord> CurrentProviders { get; set; }
    }
}