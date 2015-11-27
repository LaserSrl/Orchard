using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Twitter.ViewModels {
    public class TwitterPostVM {
        public TwitterPostVM() {
            ShowDescription = true;
            ShowTitle = true;
            ShowPicture = true;
        }
        public bool TwitterMessageSent { get; set; }
  

        public string TwitterMessage { get; set; }

        public string TwitterTitle { get; set; }
        public string TwitterDescription { get; set; }
        //public string TwitterName { get; set; }
        public string TwitterPicture { get; set; }
        public string TwitterLink { get; set; }
        public SelectList TwitterAccountList { get; set; }
        public string[] SelectedList { get; set; }
        public bool ShowTitle { get; set; }
        public bool ShowDescription { get; set; }
        public bool ShowPicture { get; set; }
        public bool TwitterCurrentLink { get; set; }
    }
    
}