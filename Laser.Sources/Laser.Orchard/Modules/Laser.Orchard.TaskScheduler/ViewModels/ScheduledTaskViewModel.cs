using Laser.Orchard.TaskScheduler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler.ViewModels {
    public class ScheduledTaskViewModel {

        //properties corresponding to the part
        public int Id;
        public string SignalName { get; set; }
        public DateTime? ScheduledStartUTC { get; set; }
        public int PeriodicityTime { get; set; }
        public TimeUnits PeriodicityUnit { get; set; }
        public int ContentItemId { get; set; }
        public int Running { get; set; }
        //boolean to mark task for deletion
        public bool Delete { get; set; }
        //boolean to mark task for scheduling/unscheduling
        public bool Scheduling;

        public ScheduledTaskViewModel() {
            ScheduledStartUTC = null; // DateTime.UtcNow;
            PeriodicityUnit = TimeUnits.Minutes;
            Running = 0;
            Delete = false;
        }

        public ScheduledTaskViewModel(ScheduledTaskPart part) {
            Id = part.Id;
            SignalName = part.SignalName;
            ScheduledStartUTC = part.ScheduledStartUTC == null ? (DateTime?)null :
                part.ScheduledStartUTC.Value.ToLocalTime();
            PeriodicityTime = part.PeriodicityTime;
            PeriodicityUnit = part.PeriodicityUnit;
            ContentItemId = part.ContentItemId;
            Running = part.RunningTaskId;
            Delete = false;
        }

        //public ScheduledTaskPart CreatePartFromVM() {
        //    return new ScheduledTaskPart {
        //        SignalName = this.SignalName,
        //        ScheduledStartUTC = this.ScheduledStartUTC,
        //        PeriodicityTime = this.PeriodicityTime,
        //        PeriodicityUnit = this.PeriodicityUnit,
        //        ContentItemId = this.ContentItemId,
        //        RunningTaskId = this.Running
        //    };
        //}

        //public LaserTaskSchedulerRecord CreateRecordFromVM() {
        //    LaserTaskSchedulerRecord ltsr = new LaserTaskSchedulerRecord();
        //    ltsr.SignalName = this.SignalName;
        //    ltsr.ScheduledStartUTC = this.ScheduledStartUTC;
        //    ltsr.PeriodicityTime = this.PeriodicityTime;
        //    ltsr.PeriodicityUnit = this.PeriodicityUnit.ToString();
        //    ltsr.ContentItemId = this.ContentItemId;
        //    ltsr.RunningTaskId = this.Running;
        //    return ltsr;
        //    //return new LaserTaskSchedulerRecord {
        //    //    SignalName = this.SignalName,
        //    //    ScheduledStartUTC = this.ScheduledStartUTC,
        //    //    PeriodicityTime = this.PeriodicityTime,
        //    //    PeriodicityUnit = this.PeriodicityUnit.ToString(),
        //    //    ContentItemId = this.ContentItemId,
        //    //    RunningTaskId = this.Running
        //    //};
        //}

        public void UpdatePart(ScheduledTaskPart part) {
            part.SignalName = this.SignalName;
            part.ScheduledStartUTC = this.ScheduledStartUTC == null ? (DateTime?)null :
                this.ScheduledStartUTC.Value.ToUniversalTime();
            part.PeriodicityTime = this.PeriodicityTime;
            part.PeriodicityUnit = this.PeriodicityUnit;
            part.ContentItemId = this.ContentItemId;
            part.RunningTaskId = this.Running;
        }
    }
}