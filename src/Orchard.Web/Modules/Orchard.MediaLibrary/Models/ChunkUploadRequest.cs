using System.Web;

namespace Orchard.MediaLibrary.Models {
    public class ChunkUploadRequest {
        public string UploadFolder { get; set; }
        public string MediaType { get; set; }
        public bool IsChunk { get; set; }
        public int ChunkNumber { get; set; }
        public bool IsFirst { get; set; }
        public bool IsLast { get; set; }
        public HttpPostedFileBase OriginalFile { get; set; }
        public bool JsonAccepted { get; set; }
    }
}