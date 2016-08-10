using Orchard.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler {
    public class TaskSchedulerMigrations : DataMigrationImpl {

        public int Create() {
            SchemaBuilder.CreateTable("ScheduledTaskRecord",
                table => table.ContentPartRecord()
                    .Column<string>("SignalName")
                    .Column<DateTime>("ScheduledStartUTC")
                    .Column<int>("PeriodicityTime")
                    .Column<string>("PeriodicityUnit")
                    .Column<int>("ContentItem_id")
                );

            return 1;
        }
    }
}