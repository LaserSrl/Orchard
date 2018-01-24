using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Web.Routing;
using System.Web.Mvc;

namespace Laser.Orchard.HID {
    public class HIDRoutes : IHttpRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[]{
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "API/Laser.Orchard.HID/HIDAPI/GetInvitation",
                    Defaults = new {
                        area = "Laser.Orchard.HID",
                        controller = "HIDAPI",
                        action = "GetInvitation"
                    }
                },
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "API/Laser.Orchard.HID/HIDAPI/IssueCredentials",
                    Defaults = new {
                        area = "Laser.Orchard.HID",
                        controller = "HIDAPI",
                        action = "IssueCredentials"
                    }
                }
            };
        }

    }
}