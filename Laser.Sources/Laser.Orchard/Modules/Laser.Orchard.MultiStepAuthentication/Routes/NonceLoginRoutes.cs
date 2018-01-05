using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MultiStepAuthentication.Routes {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    public class NonceLoginRoutes : IHttpRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (RouteDescriptor routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "api/noncelogin",
                    Defaults = new {
                        area = "Laser.Orchard.MultiStepAuthentication",
                        controller = "NonceLoginApi"
                    }
                }
            };
        }
    }
}