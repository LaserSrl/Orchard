using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Laser.Orchard.Questionnaires {
    public class Routes : IHttpRouteProvider {
        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes()) {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] { 
                new HttpRouteDescriptor {
                    Priority = 5,
                    RouteTemplate = "api/laser.questionnaireresponse/{qContext}",
                    Defaults = new {
                        area = "Laser.Orchard.Questionnaires",
                        controller = "QuestionnaireResponse",
                        action = "PostContext"
                    }
                }
            };
        }
    }
}