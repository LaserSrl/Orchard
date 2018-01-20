using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.OpenAuthentication.ViewModels {
    public class OpenAuthTemporaryUser {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Provider { get; set; }
        public string ProviderUserId { get; set; }
    }
}