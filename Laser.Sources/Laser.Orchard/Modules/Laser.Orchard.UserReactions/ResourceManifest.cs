using Orchard.UI.Resources;

namespace Laser.Orchard.UserReactions {
    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            // CSS
            manifest.DefineStyle("Reactions").SetUrl("flaticon.css");
            manifest.DefineStyle("Coloredflaticon").SetUrl("coloredflaticon.css");

            // Script
            manifest.DefineScript("Autocomplete").SetUrl("jquery.textcomplete.js");
        }
    }
}
    