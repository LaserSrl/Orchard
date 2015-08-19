using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Laser.Orchard.Calendarizer;
using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Mvc.Filters;

namespace Laser.Orchard.Calendarizer.Pojections {

    public interface IFilterProvider : IEventHandler {
        void Describe(dynamic describe);
    }

    public class TimeSpanFilter : IFilterProvider {
        public TimeSpanFilter() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }


        public void Describe(dynamic describe) {
            /// For
            /// 1st: NomeUnivoco
            /// 2nd: Categoria Localizzata
            /// 3rd: ??
            /// Element
            /// 1st: Nome del tipo
            /// 2nd: Titolo di pagina di Editing
            /// 3rd: Sottotitolo di pagina di Editing
            describe.For("CalendarTimeSpan", T("Calendar"), T("Calendar"))
                .Element("TimeSpan", T("DateTime range"), T("Search contents scheduled in a range of date/time"),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "TimeSpanForm"
                );
        }

        public void ApplyFilter(dynamic context) {
            float lat = 0, lon = 0;
            int DaysToAddFrom = 0, ForDays = 0;
            if (int.TryParse(context.State.FromDate.ToString(), out DaysToAddFrom) && int.TryParse(context.State.ForDays.ToString(), out ForDays)) {

                var query = (IHqlQuery)context.Query;
                context.Query = query.Where(x => x.ContentPartRecord<Models.SchedulerPartRecord>(), x => x.Le("FromDate", DateTime.Today.AddDays(DaysToAddFrom + ForDays))).Where(x => x.ContentPartRecord<Models.SchedulerPartRecord>(), x => x.Ge("ToDate", DateTime.Today.AddDays(DaysToAddFrom)));
            }
            return;


        }
        public LocalizedString DisplayFilter(dynamic context) {
            return T("Content Items scheduled in a range of date/time");
        }
    }

}
