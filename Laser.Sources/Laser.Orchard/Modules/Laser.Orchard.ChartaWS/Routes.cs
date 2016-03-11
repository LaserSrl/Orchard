using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Collections.Generic;

namespace Laser.Orchard.ChartaWS
{
    public class Routes : IHttpRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                new HttpRouteDescriptor {
                    Priority = 5,
                    //RouteTemplate = "charta/{controller}/{action}",
                    RouteTemplate = "charta",
                    Defaults = new {
                        area = "Laser.Orchard.ChartaWS",
                        controller = "ChartaApi",
                        action = "Get"
                    }
                },
                new HttpRouteDescriptor {
                    Priority = 6,
                    RouteTemplate = "charta-upload",
                    Defaults = new {
                        area = "Laser.Orchard.ChartaWS",
                        controller = "ChartaApi",
                        action = "Post"
                    }
                },
                new HttpRouteDescriptor {
                    Priority = 6,
                    RouteTemplate = "charta-token",
                    Defaults = new {
                        area = "Laser.Orchard.ChartaWS",
                        controller = "ChartaApi",
                        action = "GetClientToken"
                    }
                },
                new HttpRouteDescriptor {
                    Priority = 6,
                    RouteTemplate = "charta-pay",
                    Defaults = new {
                        area = "Laser.Orchard.ChartaWS",
                        controller = "ChartaApi",
                        action = "Pay"
                    }
                },
                new HttpRouteDescriptor {
                    Priority = 7,
                    RouteTemplate = "charta-paypalnotification",
                    Defaults = new {
                        area = "Laser.Orchard.ChartaWS",
                        controller = "ChartaApi",
                        action = "PaypalNotification"
                    }
                }
            };
        }
    }
}