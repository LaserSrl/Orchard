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
        HIDUserSearchResult SearchHIDUser(IUser user);
        HIDUser CreateHIDUser(IUser user, string familyName, string givenName, string email = null);

        string AuthorizationToken { get; }
        string BaseEndpoint { get; }
        string UsersEndpoint { get; }
    }
}
