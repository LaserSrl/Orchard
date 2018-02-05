using Laser.Orchard.HID.Extensions;
using Laser.Orchard.HID.Models;
using Laser.Orchard.HID.Services;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laser.Orchard.HID.Handlers {
    public class BulkCredentialOperationsTaskHandler : IScheduledTaskHandler {

        private readonly IRepository<BulkCredentialsOperationsRecord> _repository;
        private readonly IHIDCredentialsService _HIDCredentialsService;
        private readonly IContentManager _contentManager;

        public BulkCredentialOperationsTaskHandler(
            IRepository<BulkCredentialsOperationsRecord> repository,
            IHIDCredentialsService HIDCredentialsService,
            IContentManager contentManager) {

            _repository = repository;
            _HIDCredentialsService = HIDCredentialsService;
            _contentManager = contentManager;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;

            _taskRecords = new Dictionary<int, IEnumerable<BulkCredentialsOperationsRecord>>();
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public void Process(ScheduledTaskContext context) {
            // task name for this type of task is:
            // "HIDBulkCredentialsOperationsTask_{TASK_ID}"
            var taskType = context.Task.TaskType;
            if (taskType.StartsWith(Constants.HIDBulkCredentialsOperationsTaskName)) {
                // Get the task ID
                int taskId;
                if (int.TryParse(
                        taskType.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries).Last(),
                        out taskId
                    )) {
                    var log = new StringBuilder();
                    log.AppendLine(T("Starting {0}", Constants.HIDBulkCredentialsOperationsTaskName).Text);
                    // Get the actions to perform from the records and parse them into a context
                    var credentialsContext = ParseRecords(taskId);
                    foreach (var ua in credentialsContext.UserActions.Values) {
                        var user = _contentManager.Get<UserPart>(ua.UserId);
                        log.AppendLine(T("\t For user \"{0}\":",
                            user != null ? user.Email : ua.UserId.ToString()).Text);
                        if (ua.IssueList.Any()) {
                            log.AppendLine(T("\t\t Issue: {0}", 
                                string.Join(", ", ua.IssueList)).Text);
                        }
                        if (ua.RevokeList.Any()) {
                            log.AppendLine(T("\t\t Revoke: {0}",
                                string.Join(", ", ua.RevokeList)).Text);
                        }
                    }
                    // Perform the actions
                    _HIDCredentialsService.ProcessUserCredentialActions(credentialsContext);
                    if (credentialsContext.UserErrors.Any()) {
                        log.AppendLine(T("Errors:").Text);
                        log.AppendLine(credentialsContext.ErrorSummary());
                    }

                    Logger.Debug(log.ToString());

                    DeleteRecords(taskId);
                }
            }
        }

        BulkCredentialsOperationsContext ParseRecords(int taskId) {
            var context = new BulkCredentialsOperationsContext(true);

            var taskRecords = FetchRecords(taskId);

            context.PopulateFromRecords(taskRecords);

            return context;
        }


        void DeleteRecords(int taskId) {
            var taskRecords = FetchRecords(taskId);
            foreach (var rec in taskRecords) {
                _repository.Delete(rec);
            }
        }

        // Cache the read
        Dictionary<int, IEnumerable<BulkCredentialsOperationsRecord>> _taskRecords;

        IEnumerable<BulkCredentialsOperationsRecord> FetchRecords(int taskId) {
            if (!_taskRecords.ContainsKey(taskId)) {
                _taskRecords.Add(taskId, _repository
                    .Fetch(br => br.TaskId == taskId)
                    .ToList());
            }
            return _taskRecords[taskId];
        }
    }
}