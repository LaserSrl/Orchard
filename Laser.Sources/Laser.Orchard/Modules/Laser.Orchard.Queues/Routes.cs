using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.Queues
{
    public class Routes : IHttpRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (RouteDescriptor routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {

            return new[] {
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "QueuesAPI/RegisterNumber",
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Queues"},
                                                                                      {"controller", "QueuesAPI"},
                                                                                      {"action", "RegisterNumber"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "Laser.Orchard.Queues"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
             };
        }
    }
}