using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.Vimeo.Services;
using Newtonsoft.Json;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Vimeo.Controllers {
    public class VimeoUploadController : Controller {

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private readonly IVimeoUploadServices _vimeoUploadServices;
        private readonly IUtilsServices _utilsServices;

        public VimeoUploadController(IVimeoUploadServices vimeoUploadServices, IUtilsServices utilsServices) {
            _vimeoUploadServices = vimeoUploadServices;
            _utilsServices = utilsServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ActionResult TryStartUpload(int fileSize) {
            //TODO: make all the ticket creation in a single call from here. This is mostly so we do not send record ids
            //or anything like that out of the services
            int uploadId = _vimeoUploadServices.IsValidFileSize(fileSize);
            string message = T("Everything is fine").ToString();
            if (uploadId >= 0) {
                //If there is enough quota available, open an upload ticket, by posting to VimeoEndpoints.VideoUpload
                //with parameter type=streaming
                string uploadUrl = _vimeoUploadServices.GenerateUploadTicket(uploadId);
                //create a new MediaPart 
                int MediaPartId = _vimeoUploadServices.GenerateNewMediaPart(uploadId);
                object data = new {MediaPartId, uploadUrl};
                return Json(_utilsServices.GetResponse(ResponseType.Success, message, data));
            } else {
                //If there is not enough upload quota available, return an error or something.
                message = T("Error: Not enough upload quota available").ToString();
                return Json(_utilsServices.GetResponse(ResponseType.InvalidXSRF, message));
            }

        }

        
#if DEBUG
        //this method to test extracting the URL of the vimeo streams. It will not be present in the production systems
        //NOTE: at any time, these methods here in this region may not be functional, as they are continuosly tweaked to 
        //test different things.
        public ActionResult ExtractVimeoStreamUrl(int ucId) {
            //_vimeoUploadServices.FinishMediaPart(ucId);
            string ret = _vimeoUploadServices.ExtractVimeoStreamURL(ucId);//_vimeoUploadServices.GetVideoStatus(ucId);//
            return Content(ret); //JsonConvert.SerializeObject(new { ret })
        }

        public ActionResult ClearUploadRepositoryTables() {
            _vimeoUploadServices.ClearRepositoryTables();
            return null;
        }
#endif

        //todo: change this around so that the parameter passed is the MediaPartId
        public ActionResult FinishUpload(int mediaPartId) {
            string message="";
            //re-verify upload
            switch (_vimeoUploadServices.VerifyUpload(mediaPartId)) {
                case VerifyUploadResults.CompletedAlready:
                    //the periodic task had already verified that the upload had completed
                    message = T("The upload process has finished.").ToString();
                    return Json(_utilsServices.GetResponse(ResponseType.Success, message));
                    break;
                case VerifyUploadResults.Complete:
                    //we just found out that the upload is complete
                    if (_vimeoUploadServices.TerminateUpload(mediaPartId)) {
                        //Make sure the finisher task exists
                        message = T("The upload process has finished.").ToString();
                        return Json(_utilsServices.GetResponse(ResponseType.Success, message));
                    }
                    message = T("The upload has completed, but there was an error while handling the finishing touches.").ToString();
                    return Json(_utilsServices.GetResponse(ResponseType.InvalidXSRF, message));
                    break;
                case VerifyUploadResults.Incomplete:
                    //the upload is still going on
                    message = T("The upload is still in progress.").ToString();
                    return Json(_utilsServices.GetResponse(ResponseType.InvalidXSRF, message));
                    break;
                case VerifyUploadResults.StillUploading:
                    //the upload is still going on
                    message = T("The upload is still in progress.").ToString();
                    return Json(_utilsServices.GetResponse(ResponseType.InvalidXSRF, message));
                    break;
                case VerifyUploadResults.NeverExisted:
                    //we never started an upload with the given Id
                    message = T("The upload was never started, or the MediaPart is not for a Vimeo video.").ToString();
                    break;
                case VerifyUploadResults.Error:
                    //something went wrong
                    message = T("Unknown error.").ToString();
                    break;
                default:
                    //we should never be here
                    message = T("Unknown error.").ToString();
                    break;
            }
            return Json(_utilsServices.GetResponse(ResponseType.InvalidXSRF, message));
        }

        /// <summary>
        /// Endpoint where the applications may send their error messages when there are upload issues. We accept the following messages:
        /// 3001: User stopped the upload
        /// 3002: Upload stopped, will not resume
        /// 3003: Upload stopped, but may resume
        /// </summary>
        /// <param name="msgJson">A JSON representing a <type>Response</type> object describing the error.</param>
        /// <returns></returns>
        public ActionResult ErrorHandler() {
            string msgJson = new StreamReader(Request.InputStream).ReadToEnd();
            VimeoResponse resp = JsonConvert.DeserializeObject<VimeoResponse>(msgJson);
            int mpId = resp.Data.id; //the id of the MediaPart for whom we were doing the upload
            Laser.Orchard.StartupConfig.ViewModels.Response response;
            string msg = "";
            switch (resp.ErrorCode) {
                case VimeoErrorCode.NoError:
                    //nothing to do here. Honestly, the app should not have called the error action
                    response = _utilsServices.GetResponse(ResponseType.None, "");
                    break;
                case VimeoErrorCode.GenericError:
                    //Something happened, but we don not know what.
                    //Just log this
                    response = _utilsServices.GetResponse(ResponseType.None, "");
                    break;
                case VimeoErrorCode.UserStopped:
                    //The user stopped the upload, with no intention of resuming it.
                    //clear the records and destroy the MediaPart we were creating
                    msg = _vimeoUploadServices.DestroyUpload(mpId);
                    response = _utilsServices.GetResponse(ResponseType.Success, msg);
                    break;
                case VimeoErrorCode.UploadStopped:
                    //The upload stopped for an error, and there is no way to resume it.
                    //clear the records and destroy the MediaPart we were creating
                    msg = _vimeoUploadServices.DestroyUpload(mpId);
                    response = _utilsServices.GetResponse(ResponseType.Success, msg);
                    break;
                case VimeoErrorCode.UploadMayResume:
                    //The upload stopped for an error, but may resume later.
                    //Do not clear or destroy anything.
                    response = _utilsServices.GetResponse(ResponseType.None, "");
                    break;
                default:
                    //No reason why we should be here
                    response = _utilsServices.GetResponse(ResponseType.None, "");
                    break;
            }

            Logger.Error(msgJson);
            if (!string.IsNullOrWhiteSpace(msg))
                Logger.Information(msg);

            return Json(response);
        }
    }

    //We extend Laser.Orchard.StartupConfig.ViewModels.Response because we have specific error codes for Vimeo
    public enum VimeoErrorCode { NoError = 0, GenericError = 1, UserStopped = 3001, UploadStopped = 3002, UploadMayResume = 3003 }
    public class VimeoResponse : Laser.Orchard.StartupConfig.ViewModels.Response {
        public VimeoErrorCode ErrorCode {get;set;}

        public VimeoResponse() {
            this.ErrorCode = VimeoErrorCode.GenericError;
            this.Success = false;
            this.Message = "Generic Error";
            this.ResolutionAction = ResolutionAction.NoAction;
        }
    }
}