using Laser.Orchard.TaskScheduler.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laser.Orchard.TaskScheduler.Services {
    interface IScheduledTaskServices : IDependency {

        List<ScheduledTaskPart> GetAllTasks();
    }
}
