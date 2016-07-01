using Laser.Orchard.Vimeo.Services;
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
            if (uploadId >= 0) {
                //If there is enough quota available, open an upload ticket, by posting to VimeoEndpoints.VideoUpload
                //with parameter type=streaming
                string uploadUrl = _vimeoServices.GenerateUploadTicket(uploadId);
            } else {

                //If there is not enough upload quota available, return an error or something.
            }


            return null; //just here to avoid compilation errors
        }
    }
}