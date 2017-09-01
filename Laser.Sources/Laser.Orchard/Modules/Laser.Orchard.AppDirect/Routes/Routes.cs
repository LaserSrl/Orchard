using Orchard;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.AppDirect.Routes {
    public class Routes : IHttpRouteProvider, IDependency {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            using (IEnumerator<RouteDescriptor> enumerator = this.GetRoutes().GetEnumerator()) {
                while (((IEnumerator)enumerator).MoveNext()) {
                    var current = enumerator.Current;
                    routes.Add(current);
                }
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                Priority = 19,
                Route = new Route(
                "AppDirect/Create",
                new RouteValueDictionary {
                {"area", "Laser.Orchard.AppDirect"},
                {"controller", "Subscription"},
                {"action", "Create"}
                },
                new RouteValueDictionary(),
                new RouteValueDictionary {
                {"area", "Laser.Orchard.AppDirect"}
                },
                new MvcRouteHandler())
                },

                new RouteDescriptor {
                Priority = 19,
                Route = new Route(
                "AppDirect/Edit",
                new RouteValueDictionary {
                {"area", "Laser.Orchard.AppDirect"},
                {"controller", "Subscription"},
                {"action", "Edit"}
                },
                new RouteValueDictionary(),
                new RouteValueDictionary {
                {"area", "Laser.Orchard.AppDirect"}
                },
                new MvcRouteHandler())
                },

                new RouteDescriptor {
                Priority = 19,
                Route = new Route(
                "AppDirect/Cancel",
                new RouteValueDictionary {
                {"area", "Laser.Orchard.AppDirect"},
                {"controller", "Subscription"},
                {"action", "Cancel"}
                },
                new RouteValueDictionary(),
                new RouteValueDictionary {
                {"area", "Laser.Orchard.AppDirect"}
                },
                new MvcRouteHandler())
                },

                new RouteDescriptor {
                Priority = 19,
                Route = new Route(
                "AppDirect/Status",
                new RouteValueDictionary {
                {"area", "Laser.Orchard.AppDirect"},
                {"controller", "Subscription"},
                {"action", "Status"}
                },
                new RouteValueDictionary(),
                new RouteValueDictionary {
                {"area", "Laser.Orchard.AppDirect"}
                },
                new MvcRouteHandler())
                },
            };
        }
    }
}