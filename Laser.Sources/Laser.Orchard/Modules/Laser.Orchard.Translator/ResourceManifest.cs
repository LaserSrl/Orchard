using Orchard.UI.Resources;

namespace Laser.Orchard.Cookies
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest.DefineStyle("Laser.Orchard.Translator")
                .SetUrl("laser-orchard-translator.css?v=1.0")
                .SetVersion("1.0.0");
        }
    }
}
