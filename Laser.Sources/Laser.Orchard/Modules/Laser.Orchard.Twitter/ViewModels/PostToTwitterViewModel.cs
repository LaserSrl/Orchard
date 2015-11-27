using System;
namespace Laser.Orchard.Twitter.ViewModels {

    public class PostToTwitterViewModel {

        //public PostToTwitterViewModel() {
        //  //  PageId = "";
        //}

   //     public string PageId { get; set; }
        public string Message { get; set; }
   //     public string Title { get; set; }
   //     public string Description { get; set; }
//        public string Name { get; set; }
        public string Picture { get; set; }
        public string Link { get; set; }

        public Int32[] AccountList { get; set; }
    }
}