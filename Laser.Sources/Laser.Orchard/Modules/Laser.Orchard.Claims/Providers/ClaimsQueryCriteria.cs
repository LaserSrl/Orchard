using Orchard.ContentManagement;
using NHibernate;
using Orchard.UI.Admin;
using System.Web;
using Laser.Orchard.Claims.Services;
using Orchard;

namespace Laser.Orchard.Claims.Providers {
    public class ClaimsQueryCriteria : IGlobalCriteriaProvider {
        private readonly IClaimsCheckerService _claimsCheckerService;
        public ClaimsQueryCriteria(IClaimsCheckerService claimsCheckerService) {
            _claimsCheckerService = claimsCheckerService;
        }
        public void AddCriteria(ICriteria criteria) {
            _claimsCheckerService.CheckClaims(criteria);
        }
    }
}