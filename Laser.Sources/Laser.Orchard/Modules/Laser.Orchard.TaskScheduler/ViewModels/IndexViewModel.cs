using Laser.Orchard.TaskScheduler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.TaskScheduler.ViewModels {
    public class IndexViewModel {

        public List<ScheduledTaskViewModel> allTasks;

        public IndexViewModel() {
            allTasks = new List<ScheduledTaskViewModel>();
        }

        public IndexViewModel(List<ScheduledTaskPart> parts) {
            allTasks = parts.Select(p => new ScheduledTaskViewModel(p)).ToList();

        }
    }
}