using Laser.Orchard.Maps.Models;
using Orchard;
using Orchard.UI.Resources;
using Orchard.ContentManagement;


namespace Laser.Orchard.Maps {

    public class ResourceManifest : IResourceManifestProvider {
        private readonly IOrchardServices _orchardServices;
        public ResourceManifest(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public void BuildManifests(ResourceManifestBuilder builder) {
            var apiKey = "AIzaSyBFtkJDbWgDxEDJDfYvVu93L1W9z_IHHOg";
            var mapsSettings = _orchardServices.WorkContext.CurrentSite.As<MapsSiteSettingsPart>();
            if (!string.IsNullOrWhiteSpace(mapsSettings.GoogleApiKey)) {
                apiKey = mapsSettings.GoogleApiKey;
            }
            var manifest = builder.Add();
            manifest.DefineScript("LaserOrchardMaps")
                .SetUrl("maps.js");
            // Google Maps
            //Scripts
            manifest.DefineScript("GoogleMapsAPI")
              .SetUrl("https://maps.googleapis.com/maps/api/js?v=3&key=" + apiKey + "&sensor=false");
            manifest.DefineScript("GoogleMapsPlacesLib")
        .SetUrl("https://maps.googleapis.com/maps/api/js?v=3.exp&sensor=false&libraries=places");
            manifest.DefineScript("MarkerClusterer").SetUrl("MarkerClusterer.js");

            // CSS
            manifest.DefineStyle("GoogleMaps").SetUrl("GoogleMaps.css");

            // OSM Maps
            manifest.DefineScript("OpenLayersAPI")
              .SetUrl("http://www.openlayers.org/api/OpenLayers.js");

            manifest.DefineScript("OpenStreetMapAPI")
             .SetUrl("http://www.openstreetmap.org/openlayers/OpenStreetMap.js");
        }
    }
}