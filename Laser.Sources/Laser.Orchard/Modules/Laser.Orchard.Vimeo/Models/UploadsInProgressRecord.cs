using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class UploadsInProgressRecord : ContentPartRecord {
        public virtual int UploadSize { get; set; } //total size of the upload, in Bytes
        public virtual int UploadedSize { get; set; } //number of Bytes we have uploaded already
        //Information from the response to the streaming upload request
        public virtual string Uri { get; set; } //API endpoint of the upload ticket
        public virtual string CompleteUri { get; set; } //URI to use when finishing the upload
        public virtual string TicketId { get; set; } //Unique ticket ID
        public virtual string UploadLinkSecure { get; set; } //https upload url

        public UploadsInProgressRecord() {
            this.ContentItemRecord = new ContentItemRecord();
        }
    }
}