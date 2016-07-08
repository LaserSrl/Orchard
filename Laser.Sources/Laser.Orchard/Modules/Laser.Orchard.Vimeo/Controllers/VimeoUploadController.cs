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
            string message = T("Everything is fine").ToString();
            if (uploadId >= 0) {
                //If there is enough quota available, open an upload ticket, by posting to VimeoEndpoints.VideoUpload
                //with parameter type=streaming
                string uploadUrl = _vimeoServices.GenerateUploadTicket(uploadId);
                //create a new MediaPart 
                int MediaPartId = _vimeoServices.GenerateNewMediaPart(uploadId);
                json = JsonConvert.SerializeObject(new { message, MediaPartId, uploadUrl });
                return Content(json);
            } else {
                //If there is not enough upload quota available, return an error or something.
                message = T("Error: Not enough upload quota available").ToString();
                json = JsonConvert.SerializeObject(new { message });
            }
            return Content(json);

        }

        
#if DEBUG
        //this method to test extracting the URL of the vimeo streams. It will not be present in the production systems
        public ActionResult ExtractVimeoStreamUrl(int ucId) {
            _vimeoServices.FinishMediaPart(ucId);
            string ret = _vimeoServices.GetVideoStatus(ucId);//_vimeoServices.ExtractVimeoStreamURL(ucId);
            return Content(ret); //JsonConvert.SerializeObject(new { ret })
        }

        public ActionResult ClearUploadRepositoryTables() {
            _vimeoServices.ClearRepositoryTables();
            return null;
        }
#endif

        //todo: change this around so that the parameter passed is the MediaPartId
        public ActionResult FinishUpload(int uploadId) {
            string error = "";
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
                        string resultsPatch = _vimeoServices.PatchVideo(ucId);
                        if (resultsPatch == "OK") {

                        } else if (resultsPatch == "Record is null") {

                        } else {
                            //malformedrequest
                        }
                        string resultsAddGroup = _vimeoServices.TryAddVideoToGroup(ucId);
                        string resultsAddChannel = _vimeoServices.TryAddVideoToChannel(ucId);
                        string resultsAddAlbum = _vimeoServices.TryAddVideoToAlbum(ucId);
                        return Content(JsonConvert.SerializeObject(new { resultsPatch, resultsAddGroup, resultsAddChannel, resultsAddAlbum }));
                    }
                    break;
                case VerifyUploadResults.Incomplete:
                    //the upload is still going on
                    string uploadIncomplete = "The upload is still in progress.";
                    return Content(JsonConvert.SerializeObject(new { uploadIncomplete }));
                    break;
                case VerifyUploadResults.NeverExisted:
                    //we never started an upload with the given Id
                    error = "The upload was not found.";
                    break;
                case VerifyUploadResults.Error:
                    //something went wrong
                    error = "Unknown error.";
                    break;
                default:
                    //we should never be here
                    break;
            }
            return Content(JsonConvert.SerializeObject(new { error }));
        }
    }
}