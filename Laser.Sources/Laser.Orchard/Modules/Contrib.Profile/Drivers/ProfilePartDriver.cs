using Contrib.Profile.Models;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.Profile.Drivers {
    //need a ContentPartDriver for a ContentPart to actually work
    public class ProfilePartDriver : ContentPartDriver<ProfilePart> {
    }
}