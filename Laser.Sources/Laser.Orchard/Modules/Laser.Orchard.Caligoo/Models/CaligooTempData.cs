using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IdentityModel.Tokens;
using System.Net.Http;

namespace Laser.Orchard.Caligoo.Models {
    public class CaligooTempData : ISingletonDependency {
        public CaligooTempData() {
            WebApiClient = new HttpClient();
        }
        /// <summary>
        /// Implements destructor to free resource of HttpClient.
        /// </summary>
        ~CaligooTempData() {
            WebApiClient.Dispose();
        }
        public JwtSecurityToken CurrentJwtToken { get; set; }
        public string Test { get; set; }
        public HttpClient WebApiClient { get; set; }
    }
}