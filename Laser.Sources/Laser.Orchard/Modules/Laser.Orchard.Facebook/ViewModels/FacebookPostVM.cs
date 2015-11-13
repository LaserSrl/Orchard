using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Facebook.ViewModels {
    public class FacebookPostVM {
        public bool FacebookMessageSent { get; set; }
  

        public string FacebookMessage { get; set; }
        public string FacebookCaption { get; set; }
        public string FacebookDescription { get; set; }
        public string FacebookName { get; set; }
        public string FacebookPicture { get; set; }
        public string FacebookLink { get; set; }
        public SelectList FacebookAccountList { get; set; }
        public string[] SelectedList { get; set; }
       
    }
    
}