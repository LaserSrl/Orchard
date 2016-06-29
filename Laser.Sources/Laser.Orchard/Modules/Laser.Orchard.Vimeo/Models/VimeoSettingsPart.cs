using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.Models {
    public class VimeoSettingsPart : ContentPart<VimeoSettingsPartRecord> {
        public string AccessToken {
            get { return Record.AccessToken; }
            set { Record.AccessToken = value; }
        }
        public string ChannelName {
            get { return Record.ChannelName; }
            set { Record.ChannelName = value; }
        }
        public string GroupName {
            get { return Record.GroupName; }
            set { Record.GroupName = value; }
        }
        public string AlbumName {
            get { return Record.AlbumName; }
            set { Record.AlbumName = value; }
        }
    }
}