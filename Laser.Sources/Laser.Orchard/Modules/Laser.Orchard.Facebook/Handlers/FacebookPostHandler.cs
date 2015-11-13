using Laser.Orchard.Facebook.Models;
using Laser.Orchard.Facebook.Services;
using Laser.Orchard.Facebook.ViewModels;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.Facebook.Handlers {

    public class FacebookPostHandler : ContentHandler {
        private readonly IFacebookService _facebookService;

        public FacebookPostHandler(IRepository<FacebookPostPartRecord> repository, IFacebookService facebookService) {
            _facebookService = facebookService;
            Filters.Add(StorageFilter.For(repository));
            OnPublished<FacebookPostPart>((context, facebookpart) => {
                PostToFacebookViewModel Fvm = new PostToFacebookViewModel();
                Fvm.Caption = facebookpart.FacebookCaption;
                Fvm.Description = facebookpart.FacebookDescription;
                Fvm.Link = facebookpart.FacebookLink;
                Fvm.Message = facebookpart.FacebookMessage;
                Fvm.Name = facebookpart.FacebookName;
                Fvm.Picture = facebookpart.FacebookPicture;
                ResponseAction rsp = _facebookService.PostFacebook(Fvm, facebookpart);
                if (rsp.Success) {
                    facebookpart.FacebookMessageSent = true;
                }
            }
         );
        }
    }
}