using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard;

namespace Laser.Orchard.StartupConfig.Services {
    public interface ICommonsServices : IDependency {
        DevicesBrands GetDeviceBrandByUserAgent();
    }
}
