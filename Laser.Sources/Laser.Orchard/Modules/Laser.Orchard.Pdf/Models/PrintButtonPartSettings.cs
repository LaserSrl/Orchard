namespace Laser.Orchard.Pdf.Models {
    public class PrintButtonPartSettings {
        public int TemplateId { get; set; }
        public string FileNameWithoutExtension { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
    }
}