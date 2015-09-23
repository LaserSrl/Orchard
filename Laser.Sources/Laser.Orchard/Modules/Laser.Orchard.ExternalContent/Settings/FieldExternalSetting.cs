
using System.Web.Mvc;
namespace Laser.Orchard.ExternalContent.Settings {
    public class FieldExternalSetting {
        public bool Required { get; set; }
        public string ExternalURL { get; set; }
        public bool NoFollow { get; set; }
        public bool GenerateL { get; set; }
        public HttpVerbOptions HttpVerb { get; set; }
    }
}