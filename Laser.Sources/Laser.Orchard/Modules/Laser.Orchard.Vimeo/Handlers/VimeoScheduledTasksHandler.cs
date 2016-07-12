using Laser.Orchard.Vimeo.Services;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Handlers {
    public class VimeoScheduledTasksHandler : IScheduledTaskHandler {
        //move these to an interface, to avoid having to inherit this class in services
        public const string TaskTypeBase = "Laser.Orchard.Vimeo.Task";
        public const string TaskSubTypeInProgress = ".UploadsInProgress";
        public const string TaskSubTypeComplete = ".CompleteUploads";

        private readonly IScheduledTaskManager _taskManager;
        private readonly IVimeoServices _vimeoServices;

        public VimeoScheduledTasksHandler(IScheduledTaskManager taskManager, IVimeoServices vimeoServices) {
            _taskManager = taskManager;
            _vimeoServices = vimeoServices;
        }

        public void Process(ScheduledTaskContext context){
            string taskTypeStr = context.Task.TaskType;
            if (taskTypeStr == TaskTypeBase+TaskSubTypeInProgress) {
                //call service to verify the state of the uploads
                if (_vimeoServices.VerifyAllUploads() > 0) {
                    //there is still stuff in the repository, so we should reschedule the task
                    _vimeoServices.ScheduleUploadVerification();
                }
            } else if (taskTypeStr == TaskTypeBase + TaskSubTypeComplete) {
                //call service to verify status of completed uploads
                if (_vimeoServices.TerminateUploads() > 0) {
                    //there is still stuff in the repository, so we should reschedule the task
                    _vimeoServices.ScheduleVideoCompletion();
                }
            }
        }
    }
}