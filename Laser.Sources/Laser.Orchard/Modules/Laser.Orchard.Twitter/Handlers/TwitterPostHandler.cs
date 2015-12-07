using Laser.Orchard.Twitter.Models;
using Laser.Orchard.Twitter.Services;
using Laser.Orchard.Twitter.ViewModels;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using System.Web.Mvc;

namespace Laser.Orchard.Twitter.Handlers {

    public class TwitterPostHandler : ContentHandler {
        private readonly ITwitterService _TwitterService;
        private readonly IOrchardServices _orchardServices;

        public TwitterPostHandler(IRepository<TwitterPostPartRecord> repository, ITwitterService TwitterService, IOrchardServices orchardServices) {
            _TwitterService = TwitterService;
            _orchardServices = orchardServices;
            Filters.Add(StorageFilter.For(repository));
        //    Filters.Add(new ActivatingFilter<TwitterPostPart>("CommunicationAdvertising"));
            OnPublished<TwitterPostPart>((context, Twitterpart) => {
                PostToTwitterViewModel Fvm = new PostToTwitterViewModel();
                Fvm.Message = Twitterpart.TwitterMessage;
                Fvm.Picture = Twitterpart.TwitterPicture;
                if (Twitterpart.TwitterCurrentLink) {
                    var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                    Fvm.Link = urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(Twitterpart));// get current display link
                }
                Fvm.AccountList = Twitterpart.AccountList;
                ResponseAction rsp = _TwitterService.PostTwitter(Fvm);
                if (rsp.Success) {
                    Twitterpart.TwitterMessageSent = true;
                }
            });
        }
    }
}