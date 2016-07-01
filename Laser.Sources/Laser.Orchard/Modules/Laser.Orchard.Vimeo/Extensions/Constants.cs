
namespace Laser.Orchard.Vimeo.Extensions {
    public static class Constants {
        public const string LocalArea = "Laser.Orchard.Vimeo";

        //the following strings have to do with the Vimeo API
        public const string HeaderAcceptName = "Accept";
        public const string HeaderAcceptValue = "application/vnd.vimeo.*+json;version=3.2";
        public const string HeaderAuthorizationName = "Authorization"; 
        public const string HeaderAuthorizationValue = "Bearer "; //add the Access Token after this
    }

    public static class VimeoEndpoints {
        public const string APIEntry = "https://api.vimeo.com";
        public const string Me = APIEntry + "/me";
        public const string Authorize = APIEntry + "/oauth/authorize";
        public const string MyAlbums = Me + "/albums";
        public const string MyGroups = Me + "/groups";
        public const string MyChannels = Me + "/channels";
        public const string VideoUpload = Me + "/videos";
    }
}