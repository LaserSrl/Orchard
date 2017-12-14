using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Localization;
using Orchard.Logging;
using Laser.Orchard.Caligoo.Services;

namespace Laser.Orchard.Caligoo.Projections {
    public class CaligooUserQueryFilter : IFilterProvider {
        private readonly ICaligooService _caligooservice;
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        public CaligooUserQueryFilter(ICaligooService caligooservice) {
            _caligooservice = caligooservice;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }
        public void Describe(DescribeFilterContext describe) {
            describe.For("Search", T("Caligoo"), T("Caligoo"))
                .Element("Caligoo User Filter", T("Caligoo User Filter"), T("Caligoo users filtered by available criteria."),
                    ApplyFilter,
                    DisplayFilter,
                    "CaligooUserQueryFilterForm");
        }
        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Filter Caligoo users by availble criteria.");
        }
        public void ApplyFilter(FilterContext context) {
            //TODO

        }
    }
}