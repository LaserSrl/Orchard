using Orchard.Projections.Services;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Localization;
using Orchard.Logging;
using Laser.Orchard.Caligoo.Services;
using Laser.Orchard.Caligoo.Models;
using System.Globalization;
using Laser.Orchard.Caligoo.Utils;
using System.Text.RegularExpressions;

namespace Laser.Orchard.Caligoo.Projections {
    public class CaligooUserQueryFilter : IFilterProvider {
        private readonly ICaligooService _caligooService;
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        public CaligooUserQueryFilter(ICaligooService caligooService) {
            _caligooService = caligooService;
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
            string type = context.State.UserType;
            string isOnline = context.State.IsOnline;
            string dateMin = context.State.DateMin;
            string dateMax = context.State.DateMax;
            string sessionMin = context.State.SessionMin;
            string sessionMax = context.State.SessionMax;
            string visitMin = context.State.VisitMin;
            string visitMax = context.State.VisitMax;
            string locationIdList = context.State.LocationIdList;
            var filter = new CaligooUsersFilterValue();
            filter.Type = type.Trim();
            if(string.IsNullOrWhiteSpace(isOnline) == false) {
                filter.Online = (isOnline == "on");
            }
            if (string.IsNullOrWhiteSpace(dateMin) == false) {
                filter.Since = DateTime.ParseExact(dateMin, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            if (string.IsNullOrWhiteSpace(dateMax) == false) {
                filter.Until = DateTime.ParseExact(dateMax, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            if (string.IsNullOrWhiteSpace(sessionMin) == false) {
                filter.WifiSessionDurationMin = Convert.ToInt32(sessionMin);
            }
            if (string.IsNullOrWhiteSpace(sessionMax) == false) {
                filter.WifiSessionDurationMax = Convert.ToInt32(sessionMax);
            }
            if (string.IsNullOrWhiteSpace(visitMin) == false) {
                filter.VisitDurationMin = Convert.ToInt32(visitMin);
            }
            if (string.IsNullOrWhiteSpace(visitMax) == false) {
                filter.VisitDurationMax = Convert.ToInt32(visitMax);
            }
            filter.LocationList = locationIdList.Replace(" ", "");
            // cycle on result pages
            filter.Page = 1;
            bool loop = true;
            //List<string> caligooUserIds = null;
            while (loop) {
                var caligooUserIds = _caligooService.GetFilteredCaligooUsers(filter);
                if (caligooUserIds.Count == 0 && filter.Page == 1) {
                    // this condition is always false because there isn't any Caligoo user who match the criteria
                    context.Query = context.Query.Where(x => x.ContentPartRecord<CaligooUserPartRecord>(), x => x.Eq("Id", 0));
                    loop = false;
                } else if(caligooUserIds.Count == 0) {
                    loop = false;
                } else {
                    context.Query = context.Query.Where(x => x.ContentPartRecord<CaligooUserPartRecord>(), x => x.In("CaligooUserId", caligooUserIds));
                    filter.Page++;
                }
            }
        }
    }
}