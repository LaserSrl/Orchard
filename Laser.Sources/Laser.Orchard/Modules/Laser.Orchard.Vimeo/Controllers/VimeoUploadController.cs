using Laser.Orchard.Vimeo.Services;
using Newtonsoft.Json;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Vimeo.Controllers {
    public class VimeoUploadController : Controller {

        public Localizer T { get; set; }

        private readonly IVimeoServices _vimeoServices;

        public VimeoUploadController(IVimeoServices vimeoServices) {
            _vimeoServices = vimeoServices;
            T = NullLocalizer.Instance;
        }

        public ActionResult TryStartUpload(int fileSize) {
            int uploadId = _vimeoServices.IsValidFileSize(fileSize);
            string json = "";
            if (uploadId >= 0) {
                //If there is enough quota available, open an upload ticket, by posting to VimeoEndpoints.VideoUpload
                //with parameter type=streaming
                string uploadUrl = _vimeoServices.GenerateUploadTicket(uploadId);
                json = JsonConvert.SerializeObject(new { uploadId, uploadUrl });
                return Content(json);
            } else {
                //If there is not enough upload quota available, return an error or something.
                string message = T("Error: Not enough upload quota available").ToString();
                json = JsonConvert.SerializeObject(new { message });
            }
            return Content(json);

        }

        public ActionResult FinishUpload(int uploadId) {

            //re-verify upload
            switch (_vimeoServices.VerifyUpload(uploadId)) {
                case VerifyUploadResults.CompletedAlready:
                    //the periodic task had already verified that the upload had completed
                    break;
                case VerifyUploadResults.Complete:
                    //we just found out that the upload is complete
                    //Make the DELETE call to terminate the upload: this gives us the video URI
                    int ucId = _vimeoServices.TerminateUpload(uploadId);
                    if (ucId > 0) {
                        //Make the PATCH call to update the video settings (privacy and so on)
                        string res = _vimeoServices.PatchVideo(ucId);
                        if (res == "OK") {

                        } else if (res == "Record is null") {

                        } else {
                            //malformedrequest
                        }
                    }
                    break;
                case VerifyUploadResults.Incomplete:
                    //the upload is still going on
                    break;
                case VerifyUploadResults.NeverExisted:
                    //we never started an upload with the given Id
                    break;
                case VerifyUploadResults.Error:
                    //something went wrong
                    break;
                default:
                    //we should never be here
                    break;
            }

            return null; //just here to avoid compilation errors
        }
    }
}