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
        public int GetReactionClicked(int reactionTypeId, int pageId ) {

            int typeClick = 0;
            IUser userId = _userReactionService.CurrentUser();
                         
            typeClick = _userReactionService.CalculateTypeClick(userId, reactionTypeId, pageId);

            return typeClick;       
        }



    }
}