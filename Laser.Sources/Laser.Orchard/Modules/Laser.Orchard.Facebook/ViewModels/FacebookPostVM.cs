using System.Web.Mvc;

namespace Laser.Orchard.Facebook.ViewModels {

    public class FacebookPostVM {

        public FacebookPostVM() {
            ShowFacebookCaption = true;
            ShowFacebookDescription = true;
            ShowFacebookLink = true;
            ShowFacebookMessage = true;
            ShowFacebookName = true;
            ShowFacebookPicture = true;
        }

        public bool FacebookMessageSent { get; set; }
        public string FacebookMessage { get; set; }
        public string FacebookCaption { get; set; }
        public string FacebookDescription { get; set; }
        public string FacebookName { get; set; }
        public string FacebookPicture { get; set; }
        public string FacebookLink { get; set; }
        public SelectList FacebookAccountList { get; set; }
        public string[] SelectedList { get; set; }
        public bool ShowFacebookCaption { get; set; }
        public bool ShowFacebookDescription { get; set; }
        public bool ShowFacebookLink { get; set; }
        public bool ShowFacebookMessage { get; set; }
        public bool ShowFacebookName { get; set; }
        public bool ShowFacebookPicture { get; set; }
    }
}