namespace Laser.Orchard.Pdf.Models {
    public class PdfButtonPartSettings {
        private float _headerHeight = 10f; // default value
        private float _footerHeight = 10f; // default value
        public int TemplateId { get; set; }
        public string FileNameWithoutExtension { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public float HeaderHeight {
            get {
                return _headerHeight;
            }
            set {
                _headerHeight = (value > 0) ? value : _headerHeight;
            }
        }
        public float FooterHeight {
            get {
                return _footerHeight;
            }
            set {
                _footerHeight = (value > 0) ? value : _footerHeight;
            }
        }
    }
}