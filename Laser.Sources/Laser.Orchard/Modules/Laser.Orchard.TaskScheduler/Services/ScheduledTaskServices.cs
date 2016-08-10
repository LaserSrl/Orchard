using Laser.Orchard.TaskScheduler.Models;
using Orchard;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler.Services {
    public class ScheduledTaskServices : IScheduledTaskServices {

        private readonly IRepository<ScheduledTaskRecord> _repoScheduledTask;
        private readonly IOrchardServices _orchardServices;

        public ScheduledTaskServices(IRepository<ScheduledTaskRecord> repoScheduledTask, IOrchardServices orchardServices) {
            _repoScheduledTask = repoScheduledTask;
            _orchardServices = orchardServices;
        }

        public List<ScheduledTaskPart> GetAllTasks() {
            return _orchardServices.ContentManager.Query().ForPart<ScheduledTaskPart>().List().ToList();
        }
    }
}