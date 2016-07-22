using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.Queues.Models;
using Laser.Orchard.Queues.ViewModels;
using Laser.Orchard.Queues.wsCode;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace Laser.Orchard.Queues
{
    public interface IQueuesService : IDependency
    {
        IList<QueueRecord> GetQueues();
        IList<QueueUserRecord> GetQueuesUsers();
        CodeElenco GetQueueCurrentData(string queueName);
        void UpdateQueues(IEnumerable<QueueEdit> queues);
        void SendNotificationsForQueue(QueueRecord queue, int currentNumber, string defaultMessage);
        bool RegisterNumberForUser(UserPart user, string queueName, int number);
        void ScheduleStartTask(int pollingInterval);
    }

    public class QueuesService : IQueuesService
    {
        private readonly ILocalizedStringManager _localizedStringManager;
        private readonly IPushGatewayService _pushGatewayService;
        private readonly IRepository<QueueRecord> _queueRecordRepository;
        private readonly IRepository<QueueUserRecord> _queueUserRecordRepository;
        private readonly IRepository<QueueUserPartRecord> _queueUserPartRecordRepository;
        private readonly IRepository<PushNotificationRecord> _pushNotificationRecordRepository;
        private readonly IRepository<UserDeviceRecord> _userDeviceRecordRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly IScheduledTaskManager _scheduledTaskManager;

        public ILogger Logger { get; set; }

        public QueuesService(ILocalizedStringManager localizedStringManager,
                             IPushGatewayService pushGatewayService,
                             IRepository<QueueRecord> queueRecordRepository,
                             IRepository<QueueUserRecord> queueUserRecordRepository,
                             IRepository<QueueUserPartRecord> queueUserPartRecordRepository,
                             IRepository<PushNotificationRecord> pushNotificationRecordRepository,
                             IRepository<UserDeviceRecord> userDeviceRecordRepository, 
                             IOrchardServices orchardServices,
                             IScheduledTaskManager scheduledTaskManager)
        {
            _localizedStringManager = localizedStringManager;
            _pushGatewayService = pushGatewayService;
            _queueRecordRepository = queueRecordRepository;
            _queueUserRecordRepository = queueUserRecordRepository;
            _queueUserPartRecordRepository = queueUserPartRecordRepository;
            _pushNotificationRecordRepository = pushNotificationRecordRepository;
            _userDeviceRecordRepository = userDeviceRecordRepository;
            _orchardServices = orchardServices;
            _scheduledTaskManager = scheduledTaskManager;

            Logger = NullLogger.Instance;
        }

        public IList<QueueRecord> GetQueues()
        {
            return _queueRecordRepository.Table.ToList();
        }

        public IList<QueueUserRecord> GetQueuesUsers()
        {
            return _queueUserRecordRepository.Table.ToList();
        }

        public CodeElenco GetQueueCurrentData(string queueName)
        {
            string queuesWSUrl = _orchardServices.WorkContext.CurrentSite.As<QueuesSettingsPart>().EndpointUrl;

            CodaAttuale currentQueue = new CodaAttuale();
            currentQueue.Nome = queueName;

            CodeElenco queueList = new CodeElenco();
            queueList.Code = new CodaAttuale[] { currentQueue };

            System.ServiceModel.BasicHttpBinding binding = new System.ServiceModel.BasicHttpBinding();
            EndpointAddress remoteAddress = new EndpointAddress(queuesWSUrl);
            CodeGestioneClient servizioCode = new CodeGestioneClient(binding, remoteAddress);

            return servizioCode.Leggi(queueList);
        }

        public void UpdateQueues(IEnumerable<QueueEdit> queues)
        {
            foreach (var queueData in queues)
            {
                QueueRecord queue = _queueRecordRepository.Get(queueData.Id);

                if (queueData.Delete)
                {
                    if (queue != null)
                        _queueRecordRepository.Delete(_queueRecordRepository.Get(queueData.Id));
                }
                else
                {
                    if (queue == null)
                        _queueRecordRepository.Create(new QueueRecord { QueueName = queueData.QueueName, TicketGap = (int)queueData.TicketGap, MaxTicketNumber = (int)queueData.MaxTicketNumber });
                    else
                    {
                        queue.QueueName = queueData.QueueName;
                        queue.TicketGap = (int)queueData.TicketGap;
                        queue.MaxTicketNumber = (int)queueData.MaxTicketNumber;

                        _queueRecordRepository.Update(queue);
                    }
                }
            }

            _queueRecordRepository.Flush();
        }

        public void SendNotificationsForQueue(QueueRecord queue, int currentNumber, string defaultMessage)
        {
            var pushMobileSettings = _orchardServices.WorkContext.CurrentSite.As<PushMobileSettingsPart>();
            var queuesSettings = _orchardServices.WorkContext.CurrentSite.As<QueuesSettingsPart>();

            if (pushMobileSettings != null && queuesSettings != null)
            {
                IList<QueueUserRecord> queueUserList = GetQueuesUsers();
                var queueUsers = queueUserList.Where(l => l.QueueRecord.Id == queue.Id);
                var queueUsersToNotify = queueUsers.Where(u => MathMod((u.QueueNumber - currentNumber), queue.MaxTicketNumber) <= queue.TicketGap
                                                            && u.NumNotifications < queuesSettings.MaxPushToSend).Select(u => u.QueueUserPartRecord.Id).ToList();

                var userDevices = _userDeviceRecordRepository.Table.ToList();
                var registeredDevices = _pushNotificationRecordRepository.Table.ToList();

                if (registeredDevices.Count > 0 && userDevices.Count > 0 && queueUsersToNotify.Count > 0)
                {
                    List<int> notifiedUsers = new List<int>();
                    var languages = registeredDevices.GroupBy(t => t.Language).Select(g => g.First().Language).ToList();

                    foreach (string language in languages)
                    {
                        var devicesInLanguage = registeredDevices.Where(d => d.Language == language).Select(d => d.UUIdentifier);

                        var usersToNotifyForLanguage = userDevices.Where(d => queueUsersToNotify.Contains(d.UserPartRecord.Id) && devicesInLanguage.Contains(d.UUIdentifier))
                                                                   .GroupBy(d => d.UserPartRecord)
                                                                   .Select(d => d.First().UserPartRecord.Id)
                                                                   .ToList();

                        if (usersToNotifyForLanguage.Count > 0)
                        {
                            string query = "";
                            query += "SELECT DISTINCT PNR.* ";
                            query += "FROM Laser_Orchard_Mobile_PushNotificationRecord AS PNR ";
                            query += "LEFT OUTER JOIN Laser_Orchard_Mobile_UserDeviceRecord AS UDR ON PNR.UUIdentifier = UDR.UUIdentifier ";
                            query += String.Format("WHERE UDR.UserPartRecord_Id IN ({0})", String.Join(", ", usersToNotifyForLanguage));

                            string localizedMessage = _localizedStringManager.GetLocalizedString("Laser.Orchard.Queues.Tasks.QueueScheduledTaskHandler", defaultMessage, language);
                            _pushGatewayService.SendPushService(pushMobileSettings.ShowTestOptions, "All", 0, language, localizedMessage, localizedMessage, "", localizedMessage, query);

                            notifiedUsers.AddRange(usersToNotifyForLanguage);
                        }
                    }

                    notifiedUsers = notifiedUsers.Distinct().ToList();
                    foreach (int notifiedUser in notifiedUsers)
                    {
                        QueueUserRecord queueUserData = _queueUserRecordRepository.Fetch(r => r.QueueUserPartRecord.Id == notifiedUser).FirstOrDefault();

                        if (queueUserData != null)
                        {
                            queueUserData.NumNotifications = queueUserData.NumNotifications + 1;
                            _queueUserRecordRepository.Update(queueUserData);
                        }
                    }

                    _queueUserRecordRepository.Flush();
                }
            }
        }

        public bool RegisterNumberForUser(UserPart user, string queueName, int number)
        {
            bool success = false;

            if (user != null)
            {
                try
                {
                    QueueRecord queue = _queueRecordRepository.Table.Where(q => q.QueueName == queueName).FirstOrDefault();
                    QueueUserPartRecord queueUser = _queueUserPartRecordRepository.Get(user.Id);

                    if (queue != null && queueUser != null)
                    {
                        QueueUserRecord queueUserRecord = new QueueUserRecord();
                        queueUserRecord.QueueNumber = number;
                        queueUserRecord.NumNotifications = 0;
                        queueUserRecord.RegistrationDate = DateTime.UtcNow;
                        queueUserRecord.QueueRecord = queue;
                        queueUserRecord.QueueUserPartRecord = queueUser;

                        _queueUserRecordRepository.Create(queueUserRecord);

                        success = true;
                    }
                }
                catch (Exception e)
                {
                    this.Logger.Error("An error occurred while registering a number: " + e.Message);
                }
            }

            return success;
        }

        public void ScheduleStartTask(int pollingInterval)
        {
            string taskType = "QueueManagerTask";

            var tasks = _scheduledTaskManager.GetTasks(taskType);
            if (tasks == null || tasks.Count() == 0)
            {
                var date = DateTime.UtcNow;
                if (date.Second < 50)
                    date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0).AddMinutes(pollingInterval);
                else
                    date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute + 1, 0).AddMinutes(pollingInterval);

                _scheduledTaskManager.CreateTask(taskType, date, null);
            }
        }

        private int MathMod(int num, int mod)
        {
            return ((num % mod) + mod) % mod;
        }
    }
}