using System.ComponentModel;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;

namespace Laser.Orchard.Calendarizer.Models {
    public class SchedulerPart : ContentPart<SchedulerPartRecord> {
        [DisplayName("All day")]
        public bool AllDay {
            get { return Record.AllDay; }
            set { Record.AllDay = value; }
        }
        [DisplayName("From Date")]
        public virtual DateTime FromDate {
            get { return Record.FromDate; }
            set { Record.FromDate = value; }
        }
        [DisplayName("To Date")]
        public virtual DateTime ToDate {
            get { return Record.ToDate; }
            set { Record.ToDate = value; }
        }

    }
}