using Laser.Orchard.UsersExtensions.Util;
using Orchard.Security;
using Orchard.Security.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Laser.Orchard.UsersExtensions.Controllers {
    public class UserUtilityController : Controller {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        public UserUtilityController(IAuthenticationService authenticationService, IMembershipService membershipService) {
            _authenticationService = authenticationService;
            _membershipService = membershipService;
        }

        public ContentResult VodafoneLogon() {
            string msIsdn = "";
            string aux = "";
            var vodafoneEncryption = new VodafoneEncryption();
            StringBuilder sb = new StringBuilder();
            foreach(var header in Request.Headers.AllKeys) {
                sb.AppendFormat("{0} = {1}\r\n", header, Request.Headers[header]);
                aux = vodafoneEncryption.Decrypt(Request.Headers[header]);
                if(string.IsNullOrWhiteSpace(aux) == false) {
                    msIsdn = aux;
                }
            }

            if(string.IsNullOrWhiteSpace(msIsdn) == false) {
                var usr = _membershipService.GetUser(msIsdn);
                if (usr == null) {
                    usr = _membershipService.CreateUser(new CreateUserParams(msIsdn, Membership.GeneratePassword(10, 5), string.Format("{0}@vodafone.it", msIsdn), null, null, true));
                }
                // crea il cookie per il login
                if(_authenticationService is FormsAuthenticationService) {
                    // setta la scadenza del cookie
                    (_authenticationService as FormsAuthenticationService).ExpirationTimeSpan = TimeSpan.FromDays(3);
                }
                _authenticationService.SignIn(usr, true);
            }

            var result = new ContentResult();
            result.ContentType = "text/plain";
            result.ContentEncoding = Encoding.UTF8;
            result.Content = sb.ToString();

            return result;
        }
    }
}