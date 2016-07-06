using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

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

        //The following settings are used to set default values for uploaded videos
        public string License {
            get { return Record.License; }
            set { Record.License = value; }
        }
        public VimeoVideoPrivacy Privacy {
            get {
                return Record.Privacy != null ?
                    JsonConvert.DeserializeObject<VimeoVideoPrivacy>(Record.Privacy) :
                    new VimeoVideoPrivacy();
            }
            set {
                Record.Privacy = JsonConvert.SerializeObject(value);
            }
        }
        public string Password {
            get { return Record.Password; }
            set { Record.Password = value; }
        }
        public bool ReviewLink {
            get { return Record.ReviewLink; }
            set { Record.ReviewLink = value; }
        }
        public string Locale {
            get { return Record.Locale; }
            set { Record.Locale = value; }
        }
        public List<string> ContentRatings {
            get {
                return Record.ContentRatings != null ?
                    JsonConvert.DeserializeObject<List<string>>(Record.ContentRatings) :
                    new List<string>();
            }
            set {
                Record.ContentRatings = JsonConvert.SerializeObject(value);
            }
        }
        public List<string> Whitelist {
            get {
                return Record.Whitelist != null ?
                    JsonConvert.DeserializeObject<List<string>>(Record.Whitelist) :
                    new List<string>();
            }
            set {
                Record.Whitelist = JsonConvert.SerializeObject(value);
            }
        }

        public bool AlwaysUploadToGroup {
            get { return Record.AlwaysUploadToGroup; }
            set { Record.AlwaysUploadToGroup = value; }
        }
        public bool AlwaysUploadToAlbum {
            get { return Record.AlwaysUploadToAlbum; }
            set { Record.AlwaysUploadToAlbum = value; }
        }
        public bool AlwaysUploadToChannel {
            get { return Record.AlwaysUploadToChannel; }
            set { Record.AlwaysUploadToChannel = value; }
        }
    }
}