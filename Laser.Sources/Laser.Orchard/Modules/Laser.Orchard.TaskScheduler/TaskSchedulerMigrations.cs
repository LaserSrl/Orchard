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
                    .Column<int>("ContentItemId")
                    .Column<int>("RunningTask_id")
                );

            return 1;
        }

        public int UpdateFrom1() {
            SchemaBuilder.DropTable("ScheduledTaskRecord");

            SchemaBuilder.CreateTable("LaserTaskSchedulerRecord",
                table => table.ContentPartRecord()
                    .Column<string>("SignalName")
                    .Column<DateTime>("ScheduledStartUTC")
                    .Column<int>("PeriodicityTime")
                    .Column<string>("PeriodicityUnit")
                    .Column<int>("ContentItem_id")
                    .Column<int>("RunningTask_id")
                );
            return 2;
        }

        public int UpdateFrom2() {
            SchemaBuilder.DropTable("LaserTaskSchedulerRecord");

            SchemaBuilder.CreateTable("LaserTaskSchedulerRecord",
                table => table.ContentPartRecord()
                    .Column<string>("SignalName")
                    .Column<DateTime>("ScheduledStartUTC")
                    .Column<int>("PeriodicityTime")
                    .Column<string>("PeriodicityUnit")
                    .Column<int>("ContentItemId")
                    .Column<int>("RunningTaskId")
                );
            return 3;
        }
    }
}