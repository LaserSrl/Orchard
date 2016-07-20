using Orchard.MediaLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoMediaPart : MediaPart {
        public string StreamUrl {
            get {
                if (Convert.ToString(this.As<InfosetPart>().Get<MediaPart>("LogicalType")) == "OEmbed") {
                    return Convert.ToString(this.As<InfosetPart>().Get<OEmbedPart>("Source"));
                } else {
                    return "";
                }
            }
        }
    }
}