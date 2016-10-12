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

        public APIResult<FidelityCustomer> CreateFidelityAccountFromCookie()
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
                    return new APIResult<FidelityCustomer> { success = false, data = null, message = "The user is not configured to use " + GetProviderName() };
            }
            else
                return new APIResult<FidelityCustomer> { success = false, data = null, message = "Cookie not provided or not valid." };
        }

        public APIResult<FidelityCustomer> CreateFidelityAccount(FidelityUserPart fidelityPart, string username, string email)
        {
            if (fidelityPart != null && !String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(email))
            {
                if (String.IsNullOrWhiteSpace(fidelityPart.FidelityUsername) && String.IsNullOrWhiteSpace(fidelityPart.FidelityPassword) && String.IsNullOrWhiteSpace(fidelityPart.CustomerId))
                {
                    //FidelitySettingsPart settingsPart = _orchardServices.WorkContext.CurrentSite.As<FidelitySettingsPart>();
                    FidelityCustomer customer = new FidelityCustomer(email, username, Membership.GeneratePassword(12, 4));

                    APIResult<FidelityCustomer> creationRequest = _sendService.SendCustomerRegistration(settingsPart, customer);

                    if (creationRequest.success)
                    {
                        fidelityPart.FidelityUsername = customer.Username;
                        fidelityPart.FidelityPassword = Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(customer.Password)));
                        if (!string.IsNullOrWhiteSpace(customer.Id))
                        {
                            fidelityPart.CustomerId = customer.Id;
                        }
                    }
                    return creationRequest;
                }
                else
                    return new APIResult<FidelityCustomer> { success = false, data = null, message = "There is already some " + GetProviderName() + " data associated to the user. If you want to register a new account, delete the existing Loyalzoo data and then call this method again (any previous data associated to the user will be lost)." };
            }
            else
                return new APIResult<FidelityCustomer> { success = false, data = null, message = "The user is not configured to use " + GetProviderName() };
        }

        public APIResult<FidelityCustomer> GetCustomerDetails()
        {
            FidelityCustomer customer = GetCustomerFromAuthenticatedUser();
            if (customer != null)
            {
                return _sendService.SendCustomerDetails(settingsPart, customer);
            }
            else
            {
                return new APIResult<FidelityCustomer> { success = false, data = null, message = "The user is not configured to use " + GetProviderName() };
            }
        }

        public APIResult<FidelityCampaign> GetCampaignData(string id)
        {
            FidelityCampaign campaign = new FidelityCampaign();
            campaign.Id = id;
            return _sendService.SendCampaignData(settingsPart, campaign);
        }

        public APIResult<FidelityCustomer> AddPoints(double numPoints, string campaignId)
        {
            FidelityCustomer customer = GetCustomerFromAuthenticatedUser();
            FidelityCampaign campaign = new FidelityCampaign();
            campaign.Id = campaignId;
            return _sendService.SendAddPoints(settingsPart, customer, campaign, numPoints);
        }

        public APIResult<FidelityCustomer> AddPointsFromAction(string actionId, string completionPercent)
        {
            throw new NotImplementedException();
        }

        public APIResult<FidelityReward> GiveReward(string rewardId, string campaignId)
        {
            FidelityCustomer customer = GetCustomerFromAuthenticatedUser();
            if (customer != null)
            {
                FidelityReward reward = new FidelityReward();
                reward.Id = rewardId;
                FidelityCampaign campaign = new FidelityCampaign();
                campaign.Id = campaignId;
                return _sendService.SendGiveReward(settingsPart, customer, reward, campaign);
            }
            else
            {
                return new APIResult<FidelityReward> { success = false, data = null, message = "The user is not configured to use " + GetProviderName() };
            }

        }

        public APIResult<FidelityCustomer> UpdateSocial(string token, string tokenType)
        {
            throw new NotImplementedException();
        }

        public APIResult<List<FidelityCampaign>> GetCampaignList()
        {
            return _sendService.SendCampaignList(settingsPart);
        }


        // se è gia stato registrato sul provider, richiede
        // al provider l'id della sessione del cliente (logga quindi l'utente sul server remoto).
        // Se fallisce uno dei due controlli  restituisce null*/
        public virtual string LoginCustomerToRemoteProvider(FidelityCustomer customer)
        {
            string id = _sendService.SendGetCustomerSessionId(settingsPart, customer);

            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }
            return null;
        }

        public virtual FidelityCampaign GetCampaignId(string name)
        {
            FidelityCampaign camp = new FidelityCampaign();
            camp.Name = name;
            APIResult<FidelityCampaign> respCampaignId = _sendService.SendGetCampaignId(settingsPart, camp);
            if (respCampaignId.success)
            {
                return camp;
            }

            return null;
        }

        //Controlla se l'utente è loggato ad Orchard, e ne restituisce l'oggetto FidelityCustomer relativo
        public FidelityCustomer GetCustomerFromAuthenticatedUser()
        {
            var authenticatedUser = _authenticationService.GetAuthenticatedUser();
            if (authenticatedUser != null)
            {
                FidelityUserPart fidelityPart = authenticatedUser.ContentItem.As<FidelityUserPart>();

                if (fidelityPart != null && !String.IsNullOrWhiteSpace(fidelityPart.FidelityUsername)
                    && !String.IsNullOrWhiteSpace(fidelityPart.FidelityPassword)
                    && !String.IsNullOrWhiteSpace(fidelityPart.CustomerId)
                    )
                {
                    string pass = Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(fidelityPart.FidelityPassword)));
                    FidelityCustomer customer = new FidelityCustomer(authenticatedUser.Email, fidelityPart.FidelityUsername, pass);
                    customer.Id = fidelityPart.CustomerId;
                    return customer;
                }
            }
            return null;
        }

        public abstract string GetProviderName();
    }
}