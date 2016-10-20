using Laser.Orchard.FidelityGateway.Models;
using Laser.Orchard.FidelityGateway.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Http;
//Susing System.Web.Mvc;


namespace Laser.Orchard.FidelityGateway.Controllers
{
    public class FidelityBaseApiController : ApiController
    {

        private readonly IFidelityServices _fidelityService;

        public FidelityBaseApiController(IFidelityServices service)
        {
            _fidelityService = service;
        }


        [HttpGet]
        public virtual string Test(string s)
        {
            return "test" + s;
        }

        [HttpGet]
        public virtual APIResult<FidelityCustomer> CustomerRegistration()
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
                return _fidelityService.GetCustomerDetails();
            }
            catch (Exception e)
            {
                return new APIResult<FidelityCustomer> { success = false, data = null, message = e.Message };
            }
        }

        
        [HttpGet]
        public virtual APIResult<FidelityCampaign> GetCampaignData(string campaignId)
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
        public virtual APIResult<bool> AddPoints(string amount, string campaignId)
        {
            try
            {
                double points;
                if (Double.TryParse(amount, out points))
                    return _fidelityService.AddPoints(amount, campaignId);
                else
                    return new APIResult<bool> { success = false, data = false, message = "The input parameters is not in the correct format." };
            }
            catch (Exception e)
            {
                return new APIResult<bool> { success = false, data = false, message = e.Message };
            }
        }

        [HttpGet]
        public virtual APIResult<bool> GiveReward(string rewardId, string campaignId)
        {
            try
            {
                if (rewardId != null)
                    return _fidelityService.GiveReward(rewardId, campaignId);
                else
                    return new APIResult<bool> { success = false, data = false, message = "The input parameters is not in the correct format." };
            }
            catch (Exception e)
            {
                return new APIResult<bool> { success = false, data = false, message = e.Message };
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
        public virtual APIResult<bool> AddPointsFromAction(string actionId)
        {
            try
            {
                return _fidelityService.AddPointsFromAction(actionId);
            }
            catch (Exception e)
            {
                return new APIResult<bool> { success = false, data = false, message = e.Message };
            }
        }

        /*
         [System.Web.Mvc.HttpGet]
        public virtual JsonResult AddPointsFromAction(string actionId, string completionPercent)
        {
            try
            {
                return Json(_fidelityService.AddPointsFromAction(actionId, completionPercent), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult<string> { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        
        [System.Web.Mvc.HttpGet]
        public virtual JsonResult UpdateSocial(string socialToken, string tokenType)
        {
            try
            {
                if (socialToken != null && tokenType != null)
                    return Json(_fidelityService.UpdateSocial(socialToken, tokenType), JsonRequestBehavior.AllowGet);
                else
                    return Json(new APIResult<string> { success = false, data = null, message = "The input parameters is not in the correct format." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult<string> { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
         */
    }
}