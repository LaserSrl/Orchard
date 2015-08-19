using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.Calendarizer.Models;
using Orchard.Localization;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Services;

namespace Laser.Orchard.Calendarizer.Projections {
    public class TimeSpanSortCriterionProvider : ISortCriterionProvider {
        public TimeSpanSortCriterionProvider() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeSortCriterionContext describe) {
            var descriptor = describe.For("Calendar", T("Calendar"), T("Calendarizer module"));
            descriptor.Element("StartingDate", T("Starting date"), T("Sort contents using starting date information"),
                context => ApplySortCriterion(context),
                context => DisplaySortCriterion(context)
            );
        }

        public void ApplySortCriterion(SortCriterionContext context) {

            context.Query = context.Query.OrderBy(alias => alias.ContentPartRecord(typeof(SchedulerPartRecord)), x => x.Asc("FromDate"));
        }

        public LocalizedString DisplaySortCriterion(SortCriterionContext context) {
            return T("Ordered by {0}, ascending", T("Event starting date").Text);
        }
    }
}