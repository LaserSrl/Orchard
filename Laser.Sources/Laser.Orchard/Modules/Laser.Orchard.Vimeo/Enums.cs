using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Vimeo {
    public enum VerifyUploadResults { CompletedAlready, Complete, Incomplete, NeverExisted, Error }

    public enum VimeoPrivacyViewOptions { anybody, nobody, contacts, password, users, unlisted, disable }
    public enum VimeoPrivacyCommentsOptions { anybody, nobody, contacts }
}