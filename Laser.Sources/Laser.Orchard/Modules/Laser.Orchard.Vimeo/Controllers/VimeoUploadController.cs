using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Vimeo.Controllers {
    public class VimeoUploadController : Controller {

        public ActionResult TryStartUpload(int fileSize) {
            //Check the available quota against the file size.
            //The Client calling this action should send it the filesize in bytes.
            //The action should check the UploadsInProgress (could this be from System.Collections.Concurrent? Like a ConcurrentBag)
            //Should UploadsInProgress be on the db?
            //If there is not enough upload quota available, return an error or something.

            return null; //just here to avoid compilation errors
        }
    }
}