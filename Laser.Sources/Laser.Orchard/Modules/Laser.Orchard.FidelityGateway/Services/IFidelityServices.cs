using Laser.Orchard.FidelityGateway.Models;
using System.Collections.Generic;

namespace Laser.Orchard.FidelityGateway.Services
{
    public interface IFidelityServices
    {
        APIResult<FidelityCustumer> CreateFidelityAccountFromCookie();
        APIResult<FidelityCustumer> CreateFidelityAccount(FidelityUserPart fidelityPart, string username, string email);
        APIResult<FidelityCustumer> GetCustomerDetails();
        APIResult<FidelityCampaign> GetCampaignData(string id);
        APIResult<FidelityCustumer> AddPoints(double numPoints, string campaignId);
        APIResult<FidelityCustumer> AddPointsFromAction(string actionId, string completionPercent); //TODO
        APIResult<FidelityReward> GiveReward(string rewardId, string campaignId);
        APIResult<FidelityCustumer> UpdateSocial(string token, string tokenType); //TODO
        APIResult<List<FidelityCampaign>> GetCampaignList();
    }

}
