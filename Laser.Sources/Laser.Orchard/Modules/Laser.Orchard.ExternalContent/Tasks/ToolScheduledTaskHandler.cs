
using Laser.Orchard.ExternalContent.Fields;
using Laser.Orchard.ExternalContent.Settings;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Hosting;

namespace Laser.Orchard.ExternalContent.Tasks {
    public class ToolScheduledTaskHandler : IScheduledTaskHandler {

        private const string TaskType = "FieldExternalTask";
        private readonly IOrchardServices _orchardServices;
        private readonly IScheduledTaskManager _scheduledTaskManager;
        private readonly ShellSettings _shellSettings;

        public ILogger Logger { get; set; }

        public ToolScheduledTaskHandler(
            IOrchardServices orchardServices,
            IScheduledTaskManager scheduledTaskManager,
            ShellSettings shellSettings) {
            _orchardServices = orchardServices;
            _scheduledTaskManager = scheduledTaskManager;
            Logger = NullLogger.Instance;
            _shellSettings = shellSettings;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType == TaskType) {
                try {
                    Logger.Information("ExternalContent task item #{0} version {1} scheduled at {2} utc",
                         context.Task.ContentItem.Id,
                         context.Task.ContentItem.Version,
                         context.Task.ScheduledUtc);
                    var displayalias = context.Task.ContentItem.As<AutoroutePart>().DisplayAlias;
                    var CallUrl = HostingEnvironment.MapPath("~/") + _shellSettings.Name + "\\Webservices\\Alias?displayalias=" + displayalias;
                    WebClient myClient = new WebClient();
                    Stream response = myClient.OpenRead(CallUrl);
                    response.Close();
                }
                catch (Exception e) {
                    Logger.Error(e, e.Message);
                }
                finally {
                    try {
                        var fields = context.Task.ContentItem.Parts.SelectMany(x => x.Fields.Where(f => f.FieldDefinition.Name == typeof(FieldExternal).Name)).Cast<FieldExternal>();
                        Int32 minuti = 0;
                        foreach (var field in fields) {
                            var settings = field.PartFieldDefinition.Settings.GetModel<FieldExternalSetting>();
                            if (settings.ScheduledMinute > 0) {
                                minuti = settings.ScheduledMinute;
                            }
                        }
                        if (minuti > 0)
                            this.ScheduleNextTask(minuti, context.Task.ContentItem);
                    }
                    catch (Exception e) {
                        Logger.Error(e, e.Message);
                    }
                }
            }
        }

        private void ScheduleNextTask(Int32 minute, ContentItem ci) {
            if (minute > 0) {
                DateTime date = DateTime.UtcNow.AddMinutes(minute);
                _scheduledTaskManager.CreateTask(TaskType, date, ci);
            }
        }
    }
}