using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler.Models {
    public class ScheduledTaskRecord : ContentPartRecord {

        public virtual string SignalName { get; set; }
        public virtual DateTime? ScheduledStartUTC { get; set; }
        public virtual int PeriodicityTime { get; set; }
        public virtual string PeriodicityUnit { get; set; }
        public virtual int ContentItem_id { get; set; }
    }
}