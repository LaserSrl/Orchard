using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace Laser.Orchard.Policy {
    public class Routes : IRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                    AddRoute("{lang}/policies", "Policies", "Index"),
                    AddRoute("{lang}/policies/Save", "Policies", "SavePolicies"),
                    AddRoute("policies", "Policies", "Index"),
                    AddRoute("policies/Save", "Policies", "SavePolicies"),
            };
        }

        private RouteDescriptor AddRoute(string routePattern, string controllerName, string action) {
            return new RouteDescriptor {
                Priority = 15,
                Route = new Route(
                    routePattern,
                    new RouteValueDictionary {
                            {"area", "Laser.Orchard.Policy"},
                            {"controller", controllerName},
                            {"action", action}
                        },
                    new RouteValueDictionary(),
                    new RouteValueDictionary {
                            {"area", "Laser.Orchard.Policy"}
                        },
                    new MvcRouteHandler())
            };

        }
    }
}