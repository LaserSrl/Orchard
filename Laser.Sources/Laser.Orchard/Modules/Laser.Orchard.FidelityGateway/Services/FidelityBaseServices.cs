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
using System.Xml.Linq;
using Orchard.Data;

namespace Laser.Orchard.FidelityGateway.Services
{
    public abstract class FidelityBaseServices : IFidelityServices
    {

        protected readonly IOrchardServices _orchardServices;
        protected readonly IEncryptionService _encryptionService;
        protected readonly IAuthenticationService _authenticationService;
        protected readonly IMembershipService _membershipService;
        protected readonly ISendService _sendService;
        protected readonly FidelitySettingsPart settingsPart;
        protected readonly IRepository<ActionInCampaignRecord> _actionInCampaign;

        public FidelityBaseServices(IOrchardServices orchardServices, IEncryptionService encryptionService,
                               IAuthenticationService authenticationService, IMembershipService membershipService,
                               ISendService sendService, IRepository<ActionInCampaignRecord> repository)
        {
            _orchardServices = orchardServices;
            _encryptionService = encryptionService;
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _sendService = sendService;
            _actionInCampaign = repository;
            settingsPart = _orchardServices.WorkContext.CurrentSite.As<FidelitySettingsPart>();
        }

        public abstract string GetProviderName();

        public abstract APIResult<IEnumerable<ActionInCampaignRecord>> GetActions();

        public abstract APIResult<bool> AddPointsFromAction(string action);

        public virtual APIResult<FidelityCustomer> CreateFidelityAccountFromCookie()
        {

            var authenticatedUser = _authenticationService.GetAuthenticatedUser();
            if (authenticatedUser != null)
            {

                FidelityUserPart fideliyPart = (FidelityUserPart)(((dynamic)authenticatedUser.ContentItem).FidelityUserPart);

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

        public virtual APIResult<FidelityCustomer> CreateFidelityAccount(FidelityUserPart fidelityPart, string username, string email)
        {
            if (fidelityPart != null && !String.IsNullOrWhiteSpace(username))
            {
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
                return new APIResult<FidelityCustomer> { success = false, data = null, message = "The user is not configured to use " + GetProviderName() };
        }

        public virtual APIResult<FidelityCustomer> GetCustomerDetails()
        {
            FidelityCustomer customer = GetCustomerFromAuthenticatedUser();
            if (customer != null)
            {
                return _sendService.SendCustomerDetails(settingsPart, customer); ;
            }
            else
            {
                return new APIResult<FidelityCustomer> { success = false, data = null, message = "The user is not configured to use " + GetProviderName() };
            }
        }

        public virtual APIResult<FidelityCampaign> GetCampaignData(string id)
        {
            FidelityCampaign campaign = new FidelityCampaign();
            campaign.Id = id;
            return _sendService.SendCampaignData(settingsPart, campaign);
        }

        public virtual APIResult<bool> AddPoints(string numPoints, string campaignId)
        {
            FidelityCustomer customer = GetCustomerFromAuthenticatedUser();
            FidelityCampaign campaign = new FidelityCampaign();
            campaign.Id = campaignId;
            return _sendService.SendAddPoints(settingsPart, customer, campaign, numPoints);
        }

        public virtual APIResult<bool> GiveReward(string rewardId, string campaignId)
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
                return new APIResult<bool> { success = false, data = false, message = "The user is not configured to use " + GetProviderName() };
            }

        }

        public virtual APIResult<FidelityCustomer> UpdateSocial(string token, string tokenType)
        {
            throw new NotImplementedException();
        }

        public virtual APIResult<IEnumerable<FidelityCampaign>> GetCampaignList()
        {
            return _sendService.SendCampaignList(settingsPart);
        }




        /// <summary>
        /// Ritorna l'il FidelityCustomer associato all'User autenticato su Orchard
        /// </summary>
        /// <returns>FidelityCustomer se esiste un utente autenticato, null altrimenti</returns>
        public virtual FidelityCustomer GetCustomerFromAuthenticatedUser()
        {
            var authenticatedUser = _authenticationService.GetAuthenticatedUser();
            if (authenticatedUser != null)
            {
                FidelityUserPart fidelityPart = (FidelityUserPart)(((dynamic)authenticatedUser.ContentItem).FidelityUserPart);

                if (fidelityPart != null && !String.IsNullOrWhiteSpace(fidelityPart.FidelityUsername)
                    && !String.IsNullOrWhiteSpace(fidelityPart.FidelityPassword)
                    )
                {
                    string pass = Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(fidelityPart.FidelityPassword)));
                    FidelityCustomer customer = new FidelityCustomer(authenticatedUser.Email, fidelityPart.FidelityUsername, pass);
                    if (String.IsNullOrWhiteSpace(fidelityPart.CustomerId)) //TODO vedere se funziona anche su simsol, se non spostarlo in Loayalzoo Service (fatto per il primo utente al quale non era stato salvato in fase di registrazione
                    {
                        fidelityPart.CustomerId = _sendService.SendCustomerDetails(settingsPart, customer).data.Id;
                    }
                    customer.Id = fidelityPart.CustomerId;
                    return customer;
                }
            }
            return null;
        }
    }
}