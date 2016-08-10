using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler.Models {
    public class ScheduledTaskPart : ContentPart<ScheduledTaskRecord> {

        public string SignalName {
            get { return this.Retrieve(x => x.SignalName); }
            set { this.Store(x => x.SignalName, value); }
        }

        public DateTime? ScheduledStartUTC {
            get { return this.Retrieve(x => x.ScheduledStartUTC); }
            set { this.Store(x => x.ScheduledStartUTC, value); }
        }

        public int PeriodicityTime {
            get { return this.Retrieve(x => x.PeriodicityTime); }
            set { this.Store(x => x.PeriodicityTime, value); }
        }

        public TimeUnits PeriodicityUnit {
            get { return EnumExtension.ParseEnum(this.Retrieve(x => x.PeriodicityUnit)); }
            set { this.Store(x => x.PeriodicityUnit, value.ToString()); }
        }

        public int ContentItem_id {
            get { return this.Retrieve(x => x.ContentItem_id); }
            set { this.Store(x => x.ContentItem_id, value); }
        }
    }
}