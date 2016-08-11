using Laser.Orchard.TaskScheduler.Models;
using Laser.Orchard.TaskScheduler.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Scheduling.Models;
using Orchard.Data;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler.Handlers {
    public class ScheduledTaskTasksHandler : IScheduledTaskHandler {

        private readonly IOrchardServices _orchardServices;
        private readonly IScheduledTaskManager _taskManager;
        private readonly IScheduledTaskService _scheduledTaskService;
        private readonly IRepository<ScheduledTaskRecord> _repoTasks;

        public ScheduledTaskTasksHandler(IOrchardServices orchardServices,
            IScheduledTaskManager taskManager,
            IScheduledTaskService scheduledTaskService,
            IRepository<ScheduledTaskRecord> repoTasks) {
            _orchardServices = orchardServices;
            _taskManager = taskManager;
            _scheduledTaskService = scheduledTaskService;
            _repoTasks = repoTasks;
        }

        public void Process(ScheduledTaskContext context) {
            string taskTypeStr = context.Task.TaskType;
            if (taskTypeStr.IndexOf(Constants.TaskTypeBase) == 0) {
                int tId = _repoTasks.Get(str => str.TaskType.Equals(taskTypeStr)).Id;
                ScheduledTaskPart part = (ScheduledTaskPart)_orchardServices
                    .ContentManager.Query().ForPart<ScheduledTaskPart>()
                    .Where<LaserTaskSchedulerRecord>(stp => stp.RunningTaskId == tId)
                    .List().FirstOrDefault();
            }
        }
    }
}