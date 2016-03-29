using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IApiKeyService : IDependency {
        string ValidateRequestByApiKey(string additionalCacheKey, bool protectAlways = false);
        string GetValidApiKey(string sIV);
    }
}