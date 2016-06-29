using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoSettingsPartRecord : ContentPartRecord {
        public virtual string AccessToken { get; set; }
        public virtual string ChannelName { get; set; }
        public virtual string GroupName { get; set; }
        public virtual string AlbumName { get; set; }
    }
}