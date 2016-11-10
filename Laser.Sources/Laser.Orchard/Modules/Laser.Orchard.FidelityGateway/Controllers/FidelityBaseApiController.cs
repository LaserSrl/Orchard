using Laser.Orchard.FidelityGateway.Models;
using Laser.Orchard.FidelityGateway.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Http;
using System.Linq;
using Laser.Orchard.FidelityGateway.Activities;

namespace Laser.Orchard.FidelityGateway.Controllers
{
    public class FidelityBaseApiController : ApiController
    {

        private readonly IFidelityServices _fidelityService;

        public FidelityBaseApiController(IEnumerable<IFidelityServices> services)
        {
            if (services.Count() > 0)
            {
                _fidelityService = services.OrderBy(a => a.GetProviderName()).ToList()[0];
            }

        }

        [HttpGet]
        public virtual string Test(string optional = "")
        {
            if (optional == "")
            {
                return "testnooptional";
            }
            else
            {
                return "test" + optional;
            }

        }

        [HttpGet]
        public virtual APIResult<FidelityCustomer> CustomerRegistrationInDefaultCampaign()
        {
            try
            {
                return _fidelityService.CreateFidelityAccountFromCookie();
            }
            catch (Exception e)
            {
                return new APIResult<FidelityCustomer> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<FidelityCustomer> CustomerDetails()
        {
            try
            {
                return _fidelityService.GetCustomerDetails(null);
            }
            catch (Exception e)
            {
                return new APIResult<FidelityCustomer> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<FidelityCustomer> CustomerDetailsFromId(string customerId)
        {
            try
            {
                return _fidelityService.GetCustomerDetails(customerId);
            }
            catch (Exception e)
            {
                return new APIResult<FidelityCustomer> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<FidelityCampaign> GetCampaignData(string campaignId = null)
        {
            try
            {
                return _fidelityService.GetCampaignData(campaignId);
            }
            catch (Exception e)
            {
                return new APIResult<FidelityCampaign> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<FidelityCampaign> GetDefaultCampaignData()
        {
            try
            {
                return _fidelityService.GetCampaignData();
            }
            catch (Exception e)
            {
                return new APIResult<FidelityCampaign> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<IEnumerable<FidelityCampaign>> CampaignList()
        {
            try
            {
                return _fidelityService.GetCampaignList();
            }
            catch (Exception e)
            {
                return new APIResult<IEnumerable<FidelityCampaign>> { success = false, data = null, message = e.Message };
            }
        }


        [HttpGet]
        public virtual APIResult<CardPointsCampaign> AddPoints(string amount, string campaignId)
        {
            try
            {
                double points;
                if (Double.TryParse(amount, out points))
                    return _fidelityService.AddPoints(amount, campaignId, null);
                else
                    return new APIResult<CardPointsCampaign> { success = false, data = null, message = "The input parameters is not in the correct format." };
            }
            catch (Exception e)
            {
                return new APIResult<CardPointsCampaign> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<FidelityReward> GiveReward(string rewardId, string campaignId)
        {
            try
            {
                if (rewardId != null)
                    return _fidelityService.GiveReward(rewardId, campaignId, null);
                else
                    return new APIResult<FidelityReward> { success = false, data = null, message = "The input parameters is not in the correct format." };
            }
            catch (Exception e)
            {
                return new APIResult<FidelityReward> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<IEnumerable<ActionInCampaignRecord>> GetActions()
        {
            try
            {
                return _fidelityService.GetActions();
            }
            catch (Exception e)
            {
                return new APIResult<IEnumerable<ActionInCampaignRecord>> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<CardPointsCampaign> AddPointsFromAction(string actionId)
        {
            try
            {
                return _fidelityService.AddPointsFromAction(actionId, null);
            }
            catch (Exception e)
            {
                return new APIResult<CardPointsCampaign> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<FidelityReward> GiveRewardInDefaultCampaign(string rewardId)
        {
            try
            {
                if (rewardId != null)
                    return _fidelityService.GiveReward(rewardId, null);
                else
                    return new APIResult<FidelityReward> { success = false, data = null, message = "The input parameters is not in the correct format." };
            }
            catch (Exception e)
            {
                return new APIResult<FidelityReward> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<CardPointsCampaign> AddPointsInDefaultCampaign(string amount)
        {
            try
            {
                double points;
                if (Double.TryParse(amount, out points))
                    return _fidelityService.AddPoints(amount, null);
                else
                    return new APIResult<CardPointsCampaign> { success = false, data = null, message = "The input parameters is not in the correct format." };
            }
            catch (Exception e)
            {
                return new APIResult<CardPointsCampaign> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<FidelityCustomer> CustomerRegistration(string campaignId)
        {
            try
            {
                return _fidelityService.CreateFidelityAccountFromCookie(campaignId);
            }
            catch (Exception e)
            {
                return new APIResult<FidelityCustomer> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<CardPointsCampaign> AddPointsToCustomerInDefaultCampaign(string amount, string customerId)
        {
            try
            {
                double points;
                if (Double.TryParse(amount, out points))
                {
                    return _fidelityService.AddPoints(amount, customerId);
                }
                else
                {
                    return new APIResult<CardPointsCampaign> { success = false, data = null, message = "The input parameters is not in the correct format." };
                }                               
            }
            catch (Exception e)
            {
                return new APIResult<CardPointsCampaign> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<CardPointsCampaign> AddPointsToCustomer(string amount, string campaignId, string customerId)
        {
            try
            {
                double points;
                if (Double.TryParse(amount, out points))
                {
                    return _fidelityService.AddPoints(amount, campaignId, customerId);
                }
                else
                {
                    return new APIResult<CardPointsCampaign> { success = false, data = null, message = "The input parameters is not in the correct format." };
                }    
            }
            catch (Exception e)
            {
                return new APIResult<CardPointsCampaign> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<FidelityReward> GiveRewardToCustomerInDefaultCampaign(string rewardId, string customerId)
        {
            try
            {
                return _fidelityService.GiveReward(rewardId, customerId);
            }
            catch (Exception e)
            {
                return new APIResult<FidelityReward> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<FidelityReward> GiveRewardToCustomer(string rewardId, string campaignId, string customerId)
        {
            try
            {
                return _fidelityService.GiveReward(rewardId, campaignId, customerId);
            }
            catch (Exception e)
            {
                return new APIResult<FidelityReward> { success = false, data = null, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<CardPointsCampaign> AddPointsFromActionToCustomer(string actionId, string customerId)
        {
            try
            {
                return _fidelityService.AddPointsFromAction(actionId, customerId);
            }
            catch (Exception e)
            {
                return new APIResult<CardPointsCampaign> { success = false, data = null, message = e.Message };
            }
        }
    }
}