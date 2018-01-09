using Laser.Orchard.Fidelity.Models;
using Laser.Orchard.StartupConfig.IdentityProvider;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.Fidelity.Services {
    public class LoyalzooIdentityProvider : IIdentityProvider {
        public KeyValuePair<string, object> GetRelatedId(Dictionary<string, object> context) {
            var result = new KeyValuePair<string, object>("", 0);
            var tempData = false;
            if (context.ContainsKey("Content")) {
                var ci = context["Content"];
                if (ci is ContentItem) {
                    var loyalzoo = (ci as ContentItem).As<LoyalzooUserPart>();
                    if (loyalzoo != null) {
                        if(string.IsNullOrWhiteSpace(loyalzoo.CustomerSessionId)) {
                            tempData = false;
                        } else {
                            tempData = true;
                        }
                        var registration = JObject.FromObject(new KeyValuePair<string, bool>("LoyalzooRegistrationSuccess", tempData));
                        result = new KeyValuePair<string, object>("RegisteredServices", registration);
                    }
                }
            }
            return result;
        }
    }
}