
using System.Web.Mvc;
namespace Laser.Orchard.ExternalContent.Settings {
    public class FieldExternalSetting {
        public bool Required { get; set; }
        public string ExternalURL { get; set; }
        public bool NoFollow { get; set; }
        public bool GenerateL { get; set; }
        public HttpVerbOptions HttpVerb { get; set; }
        public HttpDataTypeOptions HttpDataType { get; set; }
        public string BodyRequest { get; set; }
        public bool CertificateRequired { get; set; }
        public string CerticateFileName { get; set; }
        public string CertificatePrivateKey { get; set; }
    }
}