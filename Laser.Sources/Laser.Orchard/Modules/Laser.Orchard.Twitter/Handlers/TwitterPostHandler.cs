using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.Twitter.Models;
using Laser.Orchard.Twitter.Services;
using Laser.Orchard.Twitter.ViewModels;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.UI.Notify;
using System;
using System.Web.Mvc;

namespace Laser.Orchard.Twitter.Handlers {

    public class TwitterPostHandler : ContentHandler {
        private readonly ITwitterService _TwitterService;
        private readonly IOrchardServices _orchardServices;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public TwitterPostHandler(IRepository<TwitterPostPartRecord> repository, ITwitterService TwitterService, IOrchardServices orchardServices, INotifier notifier) {
            _TwitterService = TwitterService;
            _orchardServices = orchardServices;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            Filters.Add(StorageFilter.For(repository));
        //    Filters.Add(new ActivatingFilter<TwitterPostPart>("CommunicationAdvertising"));
            OnPublished<TwitterPostPart>((context, Twitterpart) => {
                try {
                    PostToTwitterViewModel Fvm = new PostToTwitterViewModel();
                    Fvm.Message = Twitterpart.TwitterMessage;
                    if (Twitterpart.ContentItem.ContentType == "CommunicationAdvertising") {
                        ICommunicationService _communicationService;
                        bool tryed = _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
                        if (tryed) {
                            Fvm.Link = _communicationService.GetCampaignLink("Twitter", Twitterpart);
                        }
                        else
                            Fvm.Link = "";
                    }
                    else
                        if (Twitterpart.TwitterCurrentLink) {
                            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                            Fvm.Link = urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(Twitterpart));// get current display link
                        }


                    Fvm.Picture = Twitterpart.TwitterPicture;

                    Fvm.AccountList = Twitterpart.AccountList;
                    ResponseAction rsp = _TwitterService.PostTwitter(Fvm);
                    if (rsp.Success) {
                        Twitterpart.TwitterMessageSent = true;
                    }
                }
                catch(Exception ex) {
                    _notifier.Add(NotifyType.Error, T("Twitter error:" + ex.Message));
                }
            });
        }
    }
}