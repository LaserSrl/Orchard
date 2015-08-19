using System;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.Calendarizer.Models {
    public class SchedulerPartRecord : ContentPartRecord {
        public SchedulerPartRecord() {
            FromDate = DateTime.Now;
            ToDate = DateTime.Now;
        }
        public virtual bool AllDay { get; set; }
        public virtual DateTime FromDate { get; set; }
        public virtual DateTime ToDate { get; set; }
    }
}