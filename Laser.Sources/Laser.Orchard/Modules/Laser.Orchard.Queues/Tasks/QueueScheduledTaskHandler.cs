using Laser.Orchard.Queues.Models;
using Laser.Orchard.Queues.wsCode;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Queues.Tasks
{
    public class QueueScheduledTaskHandler : IScheduledTaskHandler
    {
        private const string DefaultMessage = "The queue {0} is currently serving the number {1}. Your number will be served soon.";
        private const string TaskType = "QueueManagerTask";

        private readonly ILoggerFactory _loggerFactory;
        private readonly IOrchardServices _orchardServices;
        private readonly IQueuesService _queuesService;
        private readonly IScheduledTaskManager _scheduledTaskManager;

        public ILogger Logger { get; set; }

        public QueueScheduledTaskHandler(ILoggerFactory loggerFactory,
                                         IOrchardServices orchardServices,
                                         IQueuesService queuesService,
                                         IScheduledTaskManager scheduledTaskManager)
        {
            _loggerFactory = loggerFactory;
            _orchardServices = orchardServices;
            _queuesService = queuesService;
            _scheduledTaskManager = scheduledTaskManager;

            Logger = _loggerFactory.CreateLogger(typeof(QueueScheduledTaskHandler));
        }

        public void Process(ScheduledTaskContext context)
        {
            if (context.Task.TaskType == TaskType)
            {
                try
                {
                    var queueList = _queuesService.GetQueues();

                    foreach (var queue in queueList)
                    {
                        CodeElenco queueCurrentDataList = _queuesService.GetQueueCurrentData(queue.QueueName);
                        if (queueCurrentDataList != null)
                        {
                            var queueCurrentData = queueCurrentDataList.Code.ToList().Where(q => q.Nome == queue.QueueName).FirstOrDefault();

                            if (queueCurrentData != null)
                            {
                                if (queueCurrentData.Stato == StatiCoda.Attivo)
                                {
                                    _queuesService.SendNotificationsForQueue(queue, Int32.Parse(queueCurrentData.Contatore), DefaultMessage);

                                    //TODO:
                                    //Cancellare record relativi a numeri già serviti
                                }
                            }
                        }
                    }

                    //this.Logger.Error("Scheduled task: " + DateTime.UtcNow);
                }
                catch (Exception e)
                {
                    this.Logger.Error(e, e.Message);
                }
                finally
                {
                    this.ScheduleNextTask();
                }
            }
        }

        private void ScheduleNextTask()
        {
            int pollingInterval = _orchardServices.WorkContext.CurrentSite.As<QueuesSettingsPart>().PollingInterval;

            if (pollingInterval > 0)
            {
                var date = DateTime.UtcNow;
                if (date.Second < 50)
                    date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0).AddMinutes(pollingInterval);
                else
                    date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute + 1, 0).AddMinutes(pollingInterval);

                _scheduledTaskManager.DeleteTasks(null, t => t.TaskType == TaskType);
                _scheduledTaskManager.CreateTask(TaskType, date, null);
            }
        }
    }
}