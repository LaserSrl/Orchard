using System.Collections.Generic;
using Orchard.Mvc.Routes;
using Orchard.WebApi.Routes;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Mvc;

namespace Laser.Orchard.FidelityGateway.Routes
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
            new HttpRouteDescriptor {
                Name = "CustomerRegistration",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/CustomerRegistration",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "CustomerRegistration"
                }
            },

            new HttpRouteDescriptor {
                Name = "CustomerDetails",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/CustomerDetails",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "CustomerDetails"
                }
            },

             new HttpRouteDescriptor {
                Name = "CampaignList",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/CampaignList",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "CampaignList"
                }
            },

            new HttpRouteDescriptor {
                Name = "GetCampaignData",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/GetCampaignData/{campaignId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "GetCampaignData"
                }
            },

            new HttpRouteDescriptor {
                Name = "AddPoints",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/AddPoints/{amount}/{campaignId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "AddPoints"
                }
            },

            new HttpRouteDescriptor {
                Name = "GiveReward",
                Priority = -10,
                RouteTemplate = "api/FidelityAPI/GiveReward/{rewardId}/{campaignId}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "GiveReward"
                }
            },

             new HttpRouteDescriptor {
                Name = "testroute",
                Priority = -10,
                RouteTemplate = "api/Test/{s}",
                Defaults = new {
                    area = "Laser.Orchard.FidelityGateway",
                    controller = "FidelityBaseApi",
                    action = "Test"
                }
            }
        };
        }
    }
}