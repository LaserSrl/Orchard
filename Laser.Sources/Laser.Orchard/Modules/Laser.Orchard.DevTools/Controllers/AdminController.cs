using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.UI.Notify;
using Orchard.Localization;
using Orchard.Tasks.Scheduling;
using Orchard.Data;
using Orchard.Core.Scheduling.Models;

namespace Laser.Orchard.DevTools.Controllers {
    public class AdminController : Controller {
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IScheduledTaskManager _scheduledTaskManager;
        private readonly IRepository<ScheduledTaskRecord> _repositoryScheduledTask;
        public IOrchardServices _orchardServices { get; set; }
        private readonly INotifier _notifier;
        private Localizer T { get; set; }

        public AdminController(ICsrfTokenHelper csrfTokenHelper,
            IScheduledTaskManager scheduledTaskManager,
             IRepository<ScheduledTaskRecord> repositoryScheduledTask,
            IOrchardServices orchardServices,
             INotifier notifier) {
            _csrfTokenHelper = csrfTokenHelper;
            _orchardServices = orchardServices;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _scheduledTaskManager= scheduledTaskManager;
            _repositoryScheduledTask = repositoryScheduledTask;
        }

        [HttpGet]
        [Admin]
        public ActionResult Index() {
            return View();
        }

        [HttpGet]
        [Admin]
        public ActionResult Getcsrf() {
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser != null) {
                var authCookie = System.Web.HttpContext.Current.Request.Cookies[".ASPXAUTH"];
                if (authCookie != null) {
                    var authToken = authCookie.Value;
                    string csrfToken = _csrfTokenHelper.GenerateCsrfTokenFromAuthToken(authToken);
                    _notifier.Add(NotifyType.Information, T(csrfToken));
                }
            }
            return RedirectToAction("Index","Admin");
        }

        [HttpGet]
        [Admin]
        public ActionResult ShowScheduledTask() {
           IEnumerable<ScheduledTaskRecord> st= _repositoryScheduledTask.Fetch(t => t.ScheduledUtc > DateTime.UtcNow).OrderBy(x=>x.ScheduledUtc);
            return View((object) st);
        }
        
    }
}