using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.FidelityGateway.Services;
using Orchard;
using Orchard.Security;

namespace Laser.Orchard.FidelityLoyalzoo.Services
{
    public class FidelityLoyalzooService : FidelityBaseServices
    {
        public FidelityLoyalzooService(IOrchardServices orchardServices, IEncryptionService encryptionService,
                               IAuthenticationService authenticationService, IMembershipService membershipService,
                               ISendService sendService)
            : base(orchardServices, encryptionService,
                authenticationService, membershipService,
                sendService) { }


        public override string GetProviderName()
        {
            return "Loyalzoo";
        }
    }
}