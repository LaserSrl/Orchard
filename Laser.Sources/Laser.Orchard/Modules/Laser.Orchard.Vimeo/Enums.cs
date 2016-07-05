using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Vimeo {
    public enum VerifyUploadResults { CompletedAlready, Complete, Incomplete, NeverExisted, Error }

    public enum VimeoPrivacyViewOptions { anybody, nobody, contacts, password, users, unlisted, disable }
    public enum VimeoPrivacyCommentsOptions { anybody, nobody, contacts }
    //public enum VimeoPrivacyEmbedOptions { _public, _private, _whitelist }

    public sealed class VimeoPrivacyEmbedOptions {
        private readonly string name;

        public static readonly VimeoPrivacyEmbedOptions Public = new VimeoPrivacyEmbedOptions("public");
        public static readonly VimeoPrivacyEmbedOptions Private = new VimeoPrivacyEmbedOptions("private");
        public static readonly VimeoPrivacyEmbedOptions Whitelist = new VimeoPrivacyEmbedOptions("whitelist");

        private VimeoPrivacyEmbedOptions(string n) {
            name = n;
        }

        public override string ToString() {
            return name;
        }

        public static List<SelectListItem> GetValues() {
            return (new SelectListItem[] { 
                new SelectListItem{Selected=false, Text = Public.ToString(), Value=Public.ToString()},
                new SelectListItem{Selected=false, Text = Private.ToString(), Value=Private.ToString()},
                new SelectListItem{Selected=false, Text = Whitelist.ToString(), Value=Whitelist.ToString()},
            }).ToList();
        }
    }
}