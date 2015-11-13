namespace Laser.Orchard.Facebook.ViewModels {

    public class PostToFacebookViewModel {

        public PostToFacebookViewModel() {
            PageId = "";
        }

        public string PageId { get; set; }
        public string Message { get; set; }
        public string Caption { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Link { get; set; }
    }
}