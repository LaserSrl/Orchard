using System.Web;
using System.Web.Mvc;
using Laser.Orchard.UserReactions.Models;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using System;
using Orchard.Security;

namespace Laser.Orchard.UserReactions.Controllers {
    
    public class ReactionAjaxController : Controller     
    {
        private readonly IUserReactionsService _userReactionService;

        public ReactionAjaxController(IUserReactionsService userReactionService) {
            _userReactionService = userReactionService;
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetReactionClicked(int reactionTypeId, int pageId) {

            UserReactionsVM typeClick = null;
            typeClick =_userReactionService.CalculateTypeClick(reactionTypeId, pageId); 
            return Json(typeClick) ;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetSummaryReaction(int pageId) {

            UserReactionsVM[] typeClick = null;
            typeClick = _userReactionService.GetSummaryReaction(pageId);

            return Json(typeClick) ;
        }


    }
}