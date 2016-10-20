using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.FidelityGateway.Services;
using Orchard;
using Orchard.Security;
using Orchard.Data;
using Laser.Orchard.FidelityGateway.Models;

namespace Laser.Orchard.FidelityLoyalzoo.Services
{
    public class FidelityLoyalzooService : FidelityBaseServices
    {
        public FidelityLoyalzooService(IOrchardServices orchardServices, IEncryptionService encryptionService,
                               IAuthenticationService authenticationService, IMembershipService membershipService,
                               ISendService sendService, IRepository<ActionInCampaignRecord> repository)
            : base(orchardServices, encryptionService,
                authenticationService, membershipService,
                sendService, repository)
        {
                    if (settingsPart.AccountID == null)
                    {
                        settingsPart.AccountID = _sendService.SendGetMerchantId(settingsPart).data;
                    }
       }

        public override string GetProviderName()
        {
            return "Loyalzoo";
        }

        public override APIResult<IEnumerable<ActionInCampaignRecord>> GetActions()
        {
            throw new NotImplementedException();
        }

        public override APIResult<bool> AddPointsFromAction(string action)
        {
            throw new NotImplementedException();
        }
    }
}