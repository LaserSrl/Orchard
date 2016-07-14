using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.StartupConfig.Services {
    public interface ICommonsServices : IDependency {
        DevicesBrands GetDeviceBrandByUserAgent();
        IContent GetContentByAlias(string displayAlias);
        string CreateNonce(string parametri, TimeSpan delay);
        bool DecryptNonce(string nonce, out string parametri);
    }
}
