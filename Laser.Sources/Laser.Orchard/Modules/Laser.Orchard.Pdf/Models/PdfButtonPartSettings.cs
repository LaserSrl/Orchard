using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Laser.Orchard.Pdf.Models {
    public class PdfButtonPartSettings {
        public string FileNameWithoutExtension { get; set; }
        public IEnumerable<PdfButtonSettings> PdfButtons { get; set; }
        public PdfButtonPartSettings() {
            var defaultButton= new List<PdfButtonSettings>();
            defaultButton.Add(new PdfButtonSettings());
            PdfButtons = defaultButton;
        }
        public string ParseListToString() {
            var json = JToken.FromObject(PdfButtons);
            return json.ToString();
        }
        public void LoadStringToList(string toParse) {
            var list = new List<PdfButtonSettings>();
            var json = JToken.Parse(toParse);
            foreach(var el in json) {
                list.Add(el.ToObject<PdfButtonSettings>());
            }
        }
    }
}