using Orchard.UI.Resources;

namespace TinyMce4 {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("TinyMcejQuery").SetUrl("jquery.tinymce.min.js").SetDependencies("jQuery");
            manifest.DefineScript("TinyMce").SetUrl("tinymce.min.js").SetDependencies("jQuery");
            manifest.DefineScript("OrchardTinyMce").SetUrl("orchard-tinymce.js").SetDependencies("TinyMce");
        }
    }
}
