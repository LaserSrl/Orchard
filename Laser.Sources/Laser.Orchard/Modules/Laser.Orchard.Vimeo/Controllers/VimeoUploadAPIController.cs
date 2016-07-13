using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Laser.Orchard.Vimeo.Services;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.Vimeo.Controllers {
    [WebApiKeyFilter(true)]
    public class VimeoUploadAPIController : ApiController {

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private readonly IVimeoUploadServices _vimeoUploadServices;
        private readonly IUtilsServices _utilsServices;

        public VimeoUploadAPIController(IVimeoUploadServices vimeoUploadServices, IUtilsServices utilsServices) {
            _vimeoUploadServices = vimeoUploadServices;
            _utilsServices = utilsServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Response TryStartUpload() {

            int uploadId = _vimeoUploadServices.IsValidFileSize(fileSize);
            string message = T("Everything is fine").ToString();
            if (uploadId >= 0) {
                //If there is enough quota available, open an upload ticket, by posting to VimeoEndpoints.VideoUpload
                //with parameter type=streaming
                string uploadUrl = _vimeoUploadServices.GenerateUploadTicket(uploadId);
                //create a new MediaPart 
                int MediaPartId = _vimeoUploadServices.GenerateNewMediaPart(uploadId);
                object data = new { MediaPartId, uploadUrl };
                return _utilsServices.GetResponse(ResponseType.Success, message, data);
            } else {
                //If there is not enough upload quota available, return an error or something.
                message = T("Error: Not enough upload quota available").ToString();
                return _utilsServices.GetResponse(ResponseType.InvalidXSRF, message);
            }
        }
    }
}