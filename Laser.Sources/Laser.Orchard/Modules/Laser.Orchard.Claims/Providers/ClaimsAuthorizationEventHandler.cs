using Laser.Orchard.Claims.Services;
using Orchard.Security;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Claims.Models;

namespace Laser.Orchard.Claims.Security {
    public class ClaimsAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        private readonly IClaimsCheckerService _claimsCheckerService;
        public ClaimsAuthorizationEventHandler(IClaimsCheckerService claimsCheckerService) {
            _claimsCheckerService = claimsCheckerService;
        }
        public void Adjust(CheckAccessContext context) {
        }

        public void Checking(CheckAccessContext context) {
        }

        public void Complete(CheckAccessContext context) {
            if (context.Content != null) {
                if (context.Content.ContentItem.ContentType == "Prodotto") {
                    var aux = 0;
                }
                var ci = _claimsCheckerService.CheckClaims(context.Content.ContentItem);
                if (ci == null) {
                    context.Granted = false;
                    context.Adjusted = true;
                }
            }
        }
    }
}