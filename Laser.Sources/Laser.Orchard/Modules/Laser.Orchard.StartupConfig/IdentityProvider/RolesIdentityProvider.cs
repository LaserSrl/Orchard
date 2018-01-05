using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using Orchard.Roles.Models;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.IdentityProvider {
    public class RolesIdentityProvider : IIdentityProvider {
        public KeyValuePair<string, object> GetRelatedId(Dictionary<string, object> context) {
            var result = new KeyValuePair<string, object>("", 0);
            if (context.ContainsKey("Content")) {
                var ci = context["Content"];
                if (ci is ContentItem) {
                    var rolesPart = (ci as ContentItem).As<UserRolesPart>();
                    if (rolesPart != null) {
                        result = new KeyValuePair<string, object>("Roles", new JArray(rolesPart.Roles));
                    }
                }
            }
            return result;
        }
    }
}