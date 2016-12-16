using Laser.Orchard.HID.Models;
using Orchard;
using Orchard.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.HID.Services {
    public interface IHIDAPIService : IDependency {
        AuthenticationErrors Authenticate();
        HIDUser SearchHIDUser(IUser user);

        string AuthorizationToken { get; }
    }
}
