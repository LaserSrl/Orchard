using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Security;
using Laser.Orchard.FidelityGateway.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Laser.Orchard.FidelityGateway.Services
{
    public abstract class FidelityBaseServices : IFidelityServices
    {

        private readonly IOrchardServices _orchardServices;
        private readonly IEncryptionService _encryptionService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly ISendService _sendService;
        private readonly FidelitySettingsPart settingsPart;


        public FidelityBaseServices(IOrchardServices orchardServices, IEncryptionService encryptionService,
                               IAuthenticationService authenticationService, IMembershipService membershipService,
                               ISendService sendService)
        {
            _orchardServices = orchardServices;
            _encryptionService = encryptionService;
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _sendService = sendService;
            settingsPart = _orchardServices.WorkContext.CurrentSite.As<FidelitySettingsPart>();
            if (String.IsNullOrWhiteSpace(settingsPart.AccountID))
            {
                settingsPart.AccountID = _sendService.SendGetMerchantId(settingsPart).data;
            }
        }

        public APIResult<FidelityCustumer> CreateFidelityAccountFromCookie()
        {
            var authenticatedUser = _authenticationService.GetAuthenticatedUser();
            if (authenticatedUser != null)
            {
                FidelityUserPart fideliyPart = authenticatedUser.As<FidelityUserPart>();

                if (fideliyPart != null)
                {
                    return CreateFidelityAccount(fideliyPart, authenticatedUser.UserName, authenticatedUser.Email);
                }
                else
                    return new APIResult<FidelityCustumer> { success = false, data = null, message = "The user is not configured to use " + GetProviderName() };
            }
            else
                return new APIResult<FidelityCustumer> { success = false, data = null, message = "Cookie not provided or not valid." };
        }

        public APIResult<FidelityCustumer> CreateFidelityAccount(FidelityUserPart fidelityPart, string username, string email)
        {
            if (fidelityPart != null && !String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(email))
            {
                if (String.IsNullOrWhiteSpace(fidelityPart.FidelityUsername) && String.IsNullOrWhiteSpace(fidelityPart.FidelityPassword) && String.IsNullOrWhiteSpace(fidelityPart.CustomerId))
                {
                    //FidelitySettingsPart settingsPart = _orchardServices.WorkContext.CurrentSite.As<FidelitySettingsPart>();
                    FidelityCustumer customer = new FidelityCustumer(email, username, Membership.GeneratePassword(12, 4));

                    APIResult<FidelityCustumer> creationRequest = _sendService.SendCustomerRegistration(settingsPart, customer);

                    if (creationRequest.success)
                    {
                        fidelityPart.FidelityUsername = customer.username;
                        fidelityPart.FidelityPassword = Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(customer.password)));
                    }
                    return creationRequest;
                }
                else
                    return new APIResult<FidelityCustumer> { success = false, data = null, message = "There is already some " + GetProviderName() + " data associated to the user. If you want to register a new account, delete the existing Loyalzoo data and then call this method again (any previous data associated to the user will be lost)." };
            }
            else
                return new APIResult<FidelityCustumer> { success = false, data = null, message = "The user is not configured to use " + GetProviderName() };
        }

        public APIResult<FidelityCustumer> GetCustomerDetails()
        {
            APIResult<FidelityCustumer> result = new APIResult<FidelityCustumer>();
            FidelityCustumer customer = GetCustomerId();
            if (customer != null)
            {
                result = _sendService.SendCustomerDetails(settingsPart, customer);
            }
            else
            {
                result.success = false;
                result.message = "The user is not configured to use " + GetProviderName();
                result.data = null;
            }

            return result;
        }

        public APIResult<FidelityCampaign> GetCampaignData(string id)
        {
            FidelityCampaign campaign = new FidelityCampaign();
            campaign.id = id;
            return _sendService.SendCampaignData(settingsPart, campaign);
        }

        public APIResult<FidelityCustumer> AddPoints(double numPoints, string campaignId)
        {
            FidelityCustumer customer = GetCustomerId();
            FidelityCampaign campaign = new FidelityCampaign();
            campaign.id = campaignId;
            return _sendService.SendAddPoints(settingsPart, customer, campaign, numPoints);
        }

        public APIResult<FidelityCustumer> AddPointsFromAction(string actionId, string completionPercent)
        {
            throw new NotImplementedException();
        }

        public APIResult<FidelityReward> GiveReward(string rewardId, string campaignId)
        {
            FidelityCustumer customer = GetCustomerId();
            FidelityReward reward = new FidelityReward();
            reward.id = rewardId;
            FidelityCampaign campaign = new FidelityCampaign();
            campaign.id = campaignId;
            return _sendService.SendGiveReward(settingsPart, customer, reward, campaign);
        }

        public APIResult<FidelityCustumer> UpdateSocial(string token, string tokenType)
        {
            throw new NotImplementedException();
        }

        public APIResult<List<FidelityCampaign>> GetCampaignList()
        {
            return _sendService.SendCampaignList(settingsPart);
        }

        /*Controlla se l'utente è loggato ad Orchard, 
         *se è gia stato registrato sul provider, richiede
         *al provider l'id del cliente e restituisce un oggetto
         *FidelityCustomer con l'id settato.
         *Se fallisce uno dei due controlli  restituisce null*/
        public virtual FidelityCustumer GetCustomerId()
        {
            var authenticatedUser = _authenticationService.GetAuthenticatedUser();
            if (authenticatedUser != null)
            {
                FidelityUserPart fidelityPart = authenticatedUser.ContentItem.As<FidelityUserPart>();
                if (fidelityPart != null && !String.IsNullOrWhiteSpace(fidelityPart.CustomerId))
                {
                    string pass = Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(fidelityPart.FidelityPassword)));
                    FidelityCustumer customer = new FidelityCustumer(authenticatedUser.Email, fidelityPart.FidelityUsername, pass);
                    customer.id = fidelityPart.CustomerId;
                    return customer;
                }
                else if (!String.IsNullOrWhiteSpace(fidelityPart.FidelityUsername) && !String.IsNullOrWhiteSpace(fidelityPart.FidelityPassword))
                {
                    string pass = Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(fidelityPart.FidelityPassword)));
                    FidelityCustumer customer = new FidelityCustumer(authenticatedUser.Email, fidelityPart.FidelityUsername, pass);
                    APIResult<FidelityCustumer> loginRequest = _sendService.SendGetCustomerId(settingsPart, customer);

                    if (loginRequest.success)
                    {
                        fidelityPart.CustomerId = customer.id;
                        return customer;
                    }
                }
            } return null;
        }

        public virtual FidelityCampaign GetCampaignId(string name)
        {
            FidelityCampaign camp = new FidelityCampaign();
            camp.name = name;
            APIResult<FidelityCampaign> respCampaignId = _sendService.SendGetCampaignId(settingsPart, camp);
            if (respCampaignId.success)
            {
                return camp;
            }

            return null;
        }

        public abstract string GetProviderName();
    }
}