using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;

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
                }
            };
        }
    }
}