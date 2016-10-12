using Laser.Orchard.FidelityGateway.Models;
using System.Collections.Generic;

namespace Laser.Orchard.FidelityGateway.Services
{
    public interface IFidelityServices
    {
        APIResult<FidelityCustomer> CreateFidelityAccountFromCookie();
        APIResult<FidelityCustomer> CreateFidelityAccount(FidelityUserPart fidelityPart, string username, string email);
        APIResult<FidelityCustomer> GetCustomerDetails();
        APIResult<FidelityCampaign> GetCampaignData(string id);
        APIResult<FidelityCustomer> AddPoints(double numPoints, string campaignId);
        APIResult<FidelityCustomer> AddPointsFromAction(string actionId, string completionPercent); //TODO
        APIResult<FidelityReward> GiveReward(string rewardId, string campaignId);
        APIResult<FidelityCustomer> UpdateSocial(string token, string tokenType); //TODO
        APIResult<List<FidelityCampaign>> GetCampaignList();
    }

}
