using Orchard.UI.Resources;

namespace Laser.Orchard.jQueryPlugins {

    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("jQuery_textcounter").SetUrl("textcounter.min.js").SetDependencies("jQuery");
        }
    }
}