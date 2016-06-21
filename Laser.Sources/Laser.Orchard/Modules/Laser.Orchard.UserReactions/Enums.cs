using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard;
using Orchard.Data;
using Orchard.Security;
using Orchard.Services;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.UserReactions.Services;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;

namespace Laser.Orchard.UserReactions 
{
    public enum StyleFileNameProviders { Reactions, Coloredflaticon };
    public enum ReactionsNames { angry, boring, exahausted, happy, joke, kiss, love, pain, sad, shocked, silent, ILike, Iwasthere };
    
    public class StyleAcroName
    {
        public string StyleAcronime = "glyph-icon flaticon-";

    }
        
}
