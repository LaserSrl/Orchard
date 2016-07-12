using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;

namespace Laser.Orchard.UserReactions.Controllers {
    
    [WebApiKeyFilter(true)]
    public class ReactionApiController : ApiController {

        private readonly IUserReactionsService _userReactionService;

        public ReactionApiController(IUserReactionsService userReactionService) {
            _userReactionService = userReactionService;
        }

        public UserReactionsVM[] Get(int pageId) {
            if (pageId < 1) throw new Exception("incorrect input parameter.");
            UserReactionsVM[] typeClick = null;
            typeClick = _userReactionService.GetSummaryReaction(pageId);
            return typeClick;

        }
        public UserReactionsVM Post(ReactionUpdateModel reactionUpdateModel) {
            if (reactionUpdateModel == null || reactionUpdateModel.TypeId < 1 || reactionUpdateModel.PageId < 1) {
                throw new Exception("incorrect input parameter.");
            }
            UserReactionsVM typeClick = null;
            typeClick = _userReactionService.CalculateTypeClick(reactionUpdateModel.TypeId, reactionUpdateModel.PageId);
            return typeClick;
        }

    }
    /// <summary>
    /// json example {"TypeId": 1, "PageId": 2817 }
    /// </summary>
    public class ReactionUpdateModel {
        public int TypeId { get; set; }
        public int PageId { get; set; }
    }
}