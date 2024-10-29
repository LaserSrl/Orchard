using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Upgrade.Handlers;

namespace Upgrade.Controllers {
    [Admin]
    public class EnumerationFieldsController : Controller {
        private readonly IFeatureManager _featureManager;
        private readonly INotifier _notifier;
        private readonly IScheduledTaskManager _scheduledTaskManager;

        public EnumerationFieldsController(IFeatureManager featureManager,
            INotifier notifier,
            IScheduledTaskManager scheduledTaskManager) {

            T = NullLocalizer.Instance;
            _featureManager = featureManager;
            _notifier = notifier;
            _scheduledTaskManager = scheduledTaskManager;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            ViewBag.CanMigrate = false;

            if (_featureManager.GetEnabledFeatures().All(x => x.Id != "Orchard.Fields")) {
                _notifier.Warning(T("You need to enable Orchard.Fields in order to migrate Enumeration Field values."));
            } else {
                ViewBag.CanMigrate = true;
            }

            return View();
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            _scheduledTaskManager.CreateTask(EnumerationFieldsUpgradeScheduledTaskHandler.TaskType,
                DateTime.UtcNow.AddMinutes(1),
                null);

            _notifier.Information(T("Upgrade scheduled. It may take several minutes to complete."));

            return RedirectToAction("Index");
        }
    }
}