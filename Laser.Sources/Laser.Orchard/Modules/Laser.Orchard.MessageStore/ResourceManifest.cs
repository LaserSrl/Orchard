using Orchard.UI.Resources;

namespace Laser.Orchard.MessageStore {

    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineStyle("CssMessageStore").SetUrl("MessageStore.css");
        }
    }
}
