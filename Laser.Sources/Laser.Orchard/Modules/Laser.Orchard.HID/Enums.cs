using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID {
    public enum AuthenticationErrors { NoError = 0, NotAuthenticated = 1, ClientInfoInvalid = 2, CommunicationError = 3 }
    public enum UserErrors { NoError = 0, UnknownError = 1, DoesNotExist = 2, AuthorizationFailed = 3, InternalServerError = 4,
        InvalidParameters = 5, EmailNotUnique = 6, PreconditionFailed = 7 }
    public enum SearchErrors { NoError = 0, InvalidParameters = 1, AuthorizationFailed = 2, InternalServerError = 3, 
        UnknownError = 4, NoResults = 5, TooManyResults = 6 }
}