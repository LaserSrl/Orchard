using Laser.Orchard.Facebook.Models;
using Laser.Orchard.Facebook.Services;
using Laser.Orchard.Facebook.ViewModels;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Laser.Orchard.CommunicationGateway.Services;
using Orchard.ContentManagement;
using Orchard.UI.Notify;
using Orchard.Localization;
using System;

namespace Laser.Orchard.Facebook.Handlers {

    public class FacebookPostHandler : ContentHandler {
        private readonly IFacebookService _facebookService;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }
        private readonly IOrchardServices _orchardServices;
        public FacebookPostHandler(IRepository<FacebookPostPartRecord> repository, IFacebookService facebookService, IOrchardServices orchardServices,INotifier notifier) {
            _facebookService = facebookService;
            _orchardServices = orchardServices;
            _notifier=notifier;
            T = NullLocalizer.Instance;
            Filters.Add(StorageFilter.For(repository));
            OnPublished<FacebookPostPart>((context, facebookpart) => {
                try {
                    bool publishEnabled = true;
                    string linktosend = "";
                    if (facebookpart.ContentItem.ContentType == "CommunicationAdvertising") {
                            ICommunicationService _communicationService;
                            bool tryed = _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
                            publishEnabled = _communicationService.AdvertisingIsAvailable(facebookpart.Id);
                            if (tryed) {
                                linktosend = _communicationService.GetCampaignLink("Facebook", facebookpart);
                            }
                            else
                                linktosend = "";
                            if (!publishEnabled) {
                                _notifier.Add(NotifyType.Error, T("Advertising can't be published, see campaign validation date"));
                            }
                    }

                    if (publishEnabled) {
                        PostToFacebookViewModel Fvm = new PostToFacebookViewModel();
                        Fvm.Caption = facebookpart.FacebookCaption;
                        Fvm.Description = facebookpart.FacebookDescription;
                        if (facebookpart.ContentItem.ContentType == "CommunicationAdvertising") {
                            Fvm.Link = linktosend;
                        }
                        else
                            Fvm.Link = facebookpart.FacebookLink;
                        Fvm.Message = facebookpart.FacebookMessage;
                        Fvm.Name = facebookpart.FacebookName;
                        Fvm.Picture = facebookpart.FacebookPicture;
                        if (facebookpart.SendOnNextPublish && !facebookpart.FacebookMessageSent) {
                            ResponseAction rsp = _facebookService.PostFacebook(Fvm, facebookpart);
                            if (rsp.Success) {
                                facebookpart.FacebookMessageSent = true;
                            }
                        }
                    }
                }
                catch(Exception ex) {
                    _notifier.Add(NotifyType.Error,T("Facebook error:"+ex.Message));
                }
            }
         );
        }
    }
}