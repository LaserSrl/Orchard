using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Orchard.MediaLibrary.Models {
    public class ChunkUploadRequestBinder : DefaultModelBinder {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
            ChunkUploadRequest model = base.BindModel(controllerContext, bindingContext) as ChunkUploadRequest;
            model.UploadFolder = controllerContext.HttpContext.Request["folderPath"];
            model.MediaType = controllerContext.HttpContext.Request["type"];
            if (controllerContext.HttpContext.Request.Files.Count > 0) {
                model.OriginalFile = controllerContext.HttpContext.Request.Files[0];
            }
            string rangeHeader = controllerContext.HttpContext.Request.Headers["Content-Range"];
            if (string.IsNullOrEmpty(rangeHeader))
                model.IsChunk = false;
            else {
                model.IsChunk = true;

                Match match = Regex.Match(rangeHeader, "^bytes ([\\d]+)-([\\d]+)\\/([\\d]+)$", RegexOptions.IgnoreCase);
                int bytesFrom = int.Parse(match.Groups[1].Value);
                int bytesTo = int.Parse(match.Groups[2].Value);
                int bytesFull = int.Parse(match.Groups[3].Value);
                // bytesFull must be the last index of the byte array and not the file size
                if (bytesTo >= bytesFull - 1) {
                    model.IsLast = true;
                } else {
                    model.IsLast = false;
                }                    

                if (bytesFrom == 0) {
                    model.ChunkNumber = 1;
                    model.IsFirst = true;
                } else {
                    int bytesSize = bytesTo - bytesFrom + 1;
                    model.ChunkNumber = (bytesFrom / bytesSize) + 1;
                    model.IsFirst = false;
                }
            }

            if (controllerContext.HttpContext.Request["HTTP_ACCEPT"] != null && controllerContext.HttpContext.Request["HTTP_ACCEPT"].Contains("application/json"))
                model.JsonAccepted = true;
            else
                model.JsonAccepted = false;

            return model;
        }
    }
}
