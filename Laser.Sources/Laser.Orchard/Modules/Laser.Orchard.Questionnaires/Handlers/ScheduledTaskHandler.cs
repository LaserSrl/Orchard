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


        /*
         * as this is now, the task does not get eliminated after being completed
         * */
        public ScheduledTaskHandler(IScheduledTaskManager taskManager, IQuestionnairesServices questionnairesServices) {
            _questionnairesServices = questionnairesServices;
            _taskManager = taskManager;
            Logger = NullLogger.Instance;
            try {
                DateTime firstDate = DateTime.UtcNow.AddHours(6);//DateTime.UtcNow.AddSeconds(30);//new DateTime().AddMinutes(5);
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
                    //The following line does not work here, because the task does not contain the ContentItem
                   //_questionnairesServices.SendTemplatedEmailRanking(context.Task.ContentItem.Id);
                }
                catch (Exception e) {
                    this.Logger.Error(e, e.Message);
                }
                finally {
                    DateTime nextTaskDate = DateTime.UtcNow.AddHours(6); //DateTime.UtcNow.AddSeconds(30);//
                    ScheduleNextTask(nextTaskDate);
                }
            }
        }

        private void ScheduleNextTask(DateTime date) {
            if (date > DateTime.UtcNow) {
                var tasks = this._taskManager.GetTasks(TaskType);
                if (tasks == null || tasks.Count() == 0) //this prevents from scheduling an email task if another email task is already scheduled
                    this._taskManager.CreateTask(TaskType, date, null);
            }
        }

    }
}