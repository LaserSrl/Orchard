using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class UploadsCompleteRecord {
        public virtual int Id { get; set; } //primary key provided because we do not inherit from ContentItemRecord
        public virtual string Uri { get; set; } //uri (relative to the base vimeo entrypoint) where our video can be reached
        public virtual int ProgressId { get; set; } //this is the Id this upload had when it was an upload in progress
    }
}