using Laser.Orchard.Vimeo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo.ViewModels {
    public class VimeoSettingsPartViewModel {
        public string AccessToken { get; set; }
        public string ChannelName { get; set; }
        public string GroupName { get; set; }
        public string AlbumName { get; set; }

        public VimeoSettingsPartViewModel() {

        }

        public VimeoSettingsPartViewModel(VimeoSettingsPart part) {
            AccessToken = part.AccessToken;
            ChannelName = part.ChannelName;
            GroupName = part.GroupName;
            AlbumName = part.AlbumName;
        }

        public string CensoredAccessToken {
            get {
                if (String.IsNullOrWhiteSpace(AccessToken))
                    return "";
                else
                    return AccessToken
                        .Substring(AccessToken.Length - 4)
                        .PadLeft(AccessToken.Length, 'X');
            }
        }
    }
}