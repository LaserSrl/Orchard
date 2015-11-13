using Facebook;
using Laser.Orchard.Facebook.Models;
using Laser.Orchard.Facebook.ViewModels;
using Orchard;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Laser.Orchard.Facebook.Services {

    public interface IFacebookService : IDependency {

        ResponseAction PostFacebook(PostToFacebookViewModel message, FacebookPostPart facebookpart = null);

        List<FacebookAccountPart> GetValidFacebookAccount();
    }

    public class ResponseAction {

        public ResponseAction() {
            this.Success = true;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class FacebookService : IFacebookService {
        public Localizer T { get; set; }
        private readonly INotifier _notifier;
        private readonly IOrchardServices _orchardServices;
        private readonly IWorkContextAccessor _workContext;

        public FacebookService(IOrchardServices orchardServices, INotifier notifier, IWorkContextAccessor workContext) {
            _orchardServices = orchardServices;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _workContext = workContext;
        }

        public List<FacebookAccountPart> GetValidFacebookAccount() {
            List<FacebookAccountVM> listaccount = new List<FacebookAccountVM>();
            Int32 currentiduser = _orchardServices.WorkContext.CurrentUser.Id;
            return _orchardServices.ContentManager.Query().ForPart<FacebookAccountPart>().List().Where(x => x.Valid == true && (x.Shared || x.IdUser == currentiduser)).ToList();
        }

        public ResponseAction PostFacebook(PostToFacebookViewModel message, FacebookPostPart facebookpart = null) {
            ResponseAction rsp = new ResponseAction();
            List<FacebookAccountPart> FacebookAccountSettings = Facebook_GetAccessToken(facebookpart);
            string accessToken = "";
            string pageId = "";
            foreach (FacebookAccountPart Faccount in FacebookAccountSettings) {
                try {
                    if (string.IsNullOrEmpty(Faccount.IdPage)) {
                        accessToken = Faccount.UserToken;
                        pageId = "";
                    }
                    else {
                        accessToken = Faccount.PageToken;
                        pageId = "";
                    }
                    var objFacebookClient = new FacebookClient(accessToken);
                    var parameters = new Dictionary<string, object>();
                    if (!string.IsNullOrEmpty(message.Message))
                        parameters["message"] = message.Message;
                    if (!string.IsNullOrEmpty(message.Caption))
                        parameters["caption"] = message.Caption;
                    if (!string.IsNullOrEmpty(message.Description))
                        parameters["description"] = message.Description;
                    if (!string.IsNullOrEmpty(message.Name))
                        parameters["name"] = message.Name;
                    if (!string.IsNullOrEmpty(message.Picture))
                        parameters["picture"] = message.Picture;
                    if (!string.IsNullOrEmpty(message.Link))
                        parameters["link"] = message.Link;
                    if (pageId != "") {
                        pageId += "/";
                    }
                    var objresponsePost = objFacebookClient.Post(pageId + "feed", parameters);
                    rsp.Message = "Posted";
                }
                catch (Exception ex) {
                    rsp.Success = false;
                    rsp.Message = "Facebook Posting Error Message: " + ex.Message;
                }
                _notifier.Add(NotifyType.Warning, T(rsp.Message));
            }
            return rsp;
        }

        private List<FacebookAccountPart> Facebook_GetAccessToken(FacebookPostPart facebookpart) {
            List<FacebookAccountPart> allparts = _orchardServices.ContentManager.Query().ForPart<FacebookAccountPart>().List().Where(x => x.Valid == true).ToList();
            return allparts.Where(x => facebookpart.AccountList.Contains(x.Id)).ToList();
        }
    }
}