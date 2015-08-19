using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.UI.Resources;
namespace Laser.Orchard.StartupConfig
{
    public class ResourceManifest : IResourceManifestProvider {

        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            //manifest.DefineStyle("FontAwesome").SetUrl("font-awesome/css/font-awesome.min.css");
            manifest.DefineStyle("FontAwesome430").SetUrl("//maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome.min.css");

            //maxcdn.bootstrapcdn.com/font-awesome/4.3.0/css/font-awesome.min.css
        }
    }
}