using System.Web;
using System.Web.Mvc;
using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace Laser.Orchard.UserReactions.Controllers {
    
    public class ReactionAjaxController : Controller     
    {

        [HttpPost]
        [ValidateAntiForgeryToken]
        public bool GetReactionClicked(int pageId, int reactionTypeId) {
         //public int GetReactionClicked() {
            
            ////int ret1 = pageId;
            ////int ret2 = reactionTypeId;
            //CalculateTypeClick(UserId, ReactionType)
            //Inserting data in table

            return true;
        }



    }
}