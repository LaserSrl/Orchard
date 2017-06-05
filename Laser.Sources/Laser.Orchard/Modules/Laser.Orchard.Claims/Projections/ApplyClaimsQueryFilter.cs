using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Localization;
using Laser.Orchard.Claims.Services;

namespace Laser.Orchard.Claims.Projections {
    public class ApplyClaimsQueryFilter : IFilterProvider {
        private readonly IClaimsCheckerService _claimsCheckerService;
        public Localizer T { get; set; }
        public ApplyClaimsQueryFilter(IClaimsCheckerService claimsCheckerService) {
            _claimsCheckerService = claimsCheckerService;
            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeFilterContext describe) {
            describe.For("Search", T("Claims"), T("Claims"))
                .Element("Claims Filter", T("Apply Claims"), T("Apply Claims to current query."),
                    ApplyFilter,
                    DisplayFilter);
        }
        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Apply user claims to current query.");
        }
        public void ApplyFilter(FilterContext context) {
            _claimsCheckerService.CheckClaims(context);
        }
    }
}