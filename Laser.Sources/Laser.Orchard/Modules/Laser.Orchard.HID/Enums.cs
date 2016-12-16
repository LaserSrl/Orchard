using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID {
    public enum AuthenticationErrors { NoError = 0, NotAuthenticated = 1, ClientInfoInvalid = 2, CommunicationError = 3 }
    public enum UserErrors { NoError = 0, UnknownError = 1, DoesNotExist = 2, NotAuthorized = 3, InternalServerError = 4 }
}