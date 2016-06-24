using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.ViewModels;

namespace Laser.Orchard.UserReactions.Controllers {
    public class ReactionApiController : ApiController {

        private readonly IUserReactionsService _userReactionService;

        public ReactionApiController(IUserReactionsService userReactionService) {
            _userReactionService = userReactionService;
        }

        public UserReactionsVM[] Get(int pageId) {
            UserReactionsVM[] typeClick = null;
            typeClick = _userReactionService.GetSummaryReaction(pageId);
            return typeClick;

        }
        public UserReactionsVM Post(int reactionTypeId, int pageId) {
            UserReactionsVM typeClick = null;
            typeClick = _userReactionService.CalculateTypeClick(reactionTypeId, pageId);
            return typeClick;
        }

    }
}