using Laser.Orchard.Questionnaires.Services;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.Handlers {

    public class ScheduledTaskHandler : IScheduledTaskHandler {
        private const string TaskType = "QuestionnaireRanking";
        private readonly IScheduledTaskManager _taskManager;
        private readonly IQuestionnairesServices _questionnairesServices;

        public ILogger Logger { get; set; }

        public ScheduledTaskHandler(IScheduledTaskManager taskManager, IQuestionnairesServices questionnairesServices) {
            _questionnairesServices = questionnairesServices;
            _taskManager = taskManager;
            Logger = NullLogger.Instance;
            try {
                DateTime firstDate = DateTime.UtcNow.AddHours(6);//new DateTime().AddMinutes(5);
                ScheduleNextTask(firstDate);
            }
            catch (Exception e) {
                this.Logger.Error(e, e.Message);
            }
        }

        public void Process(ScheduledTaskContext context) {
          //  this.Logger.Error("sono dentro process");
            if (context.Task.TaskType == TaskType) {
                try {
               
                   bool sended= _questionnairesServices.SendTemplatedEmailRanking();
                }
                catch (Exception e) {
                    this.Logger.Error(e, e.Message);
                }
                finally {
                    DateTime nextTaskDate = DateTime.UtcNow.AddHours(6);
                    ScheduleNextTask(nextTaskDate);
                }
            }
        }

        private void ScheduleNextTask(DateTime date) {
            if (date > DateTime.UtcNow) {
                var tasks = this._taskManager.GetTasks(TaskType);
                if (tasks == null || tasks.Count() == 0)
                    this._taskManager.CreateTask(TaskType, date, null);
            }
        }

    }
}