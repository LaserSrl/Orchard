using Laser.Orchard.FidelityGateway.Services;
using System;
using System.Globalization;
using System.Web.Mvc;

namespace Laser.Orchard.FidelityGateway.Models
{
    public class FidelityBaseApiController : Controller
    {

        private readonly IFidelityServices _fidelityService;

        public FidelityBaseApiController(IFidelityServices service)
        {
            _fidelityService = service;
        }

        [System.Web.Mvc.HttpGet]
        public virtual JsonResult CustomerRegistration()
        {
            try
            {
                return Json(_fidelityService.CreateFidelityAccountFromCookie(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult<string> { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [System.Web.Mvc.HttpGet]
        public virtual JsonResult CustomerDetails()
        {
            try
            {
                return Json(_fidelityService.GetCustomerDetails(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult<string> { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [System.Web.Mvc.HttpGet]
        public virtual JsonResult CampaignData(string campaign_id)
        {
            try
            {
                return Json(_fidelityService.GetCampaignData(campaign_id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult<string> { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [System.Web.Mvc.HttpGet]
        public virtual JsonResult CampaignList()
        {
            try
            {
                return Json(_fidelityService.GetCampaignList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult<string> { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [System.Web.Mvc.HttpGet]
        public virtual JsonResult AddPoints(string amount, string campaignId)
        {
            try
            {
                double points;
                if (Double.TryParse(amount, out points))
                    return Json(_fidelityService.AddPoints(points, campaignId), JsonRequestBehavior.AllowGet);
                else
                    return Json(new APIResult<string> { success = false, data = null, message = "The input parameters is not in the correct format." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new APIResult<string> { success = false, data = null, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

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
        public virtual JsonResult GiveReward(string rewardId, string campaignId)
        {
            try
            {
                if (rewardId != null)
                    return Json(_fidelityService.GiveReward(rewardId, campaignId), JsonRequestBehavior.AllowGet);
                else
                    return Json(new APIResult<string> { success = false, data = null, message = "The input parameters is not in the correct format." }, JsonRequestBehavior.AllowGet);
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
    }
}