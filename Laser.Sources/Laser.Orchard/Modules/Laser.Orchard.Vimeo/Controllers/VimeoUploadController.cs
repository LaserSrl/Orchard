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
            _vimeoServices.VerifyUpload(uploadId);
            //terminate upload

            //update the task that creates the content items

            return null; //just here to avoid compilation errors
        }
    }
}