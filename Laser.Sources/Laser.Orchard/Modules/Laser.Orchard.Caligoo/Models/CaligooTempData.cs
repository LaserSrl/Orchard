using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IdentityModel.Tokens;

namespace Laser.Orchard.Caligoo.Models {
    public class CaligooTempData : ISingletonDependency {
        public JwtSecurityToken CurrentJwtToken { get; set; }
        public string Test { get; set; }
    }
}