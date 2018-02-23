using Laser.Orchard.Pdf.Models;
using Laser.Orchard.Pdf.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tokens;

namespace Laser.Orchard.Pdf.Drivers {
    public class PdfButtonPartDriver : ContentPartDriver<PdfButtonPart> {
        private readonly ITokenizer _tokenizer;
        protected override string Prefix {
            get { return "Laser.Orchard.Pdf"; }
        }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public PdfButtonPartDriver(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        protected override DriverResult Editor(PdfButtonPart part, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, shapeHelper);
        }
        protected override DriverResult Editor(PdfButtonPart part, dynamic shapeHelper) {
            var model = new PdfButtonPartVM { ContentId = part.Id };
            return ContentShape("Parts_PdfButtonPart", () => 
                shapeHelper.EditorTemplate(TemplateName: "Parts/PdfButtonPart", Model: model, Prefix: Prefix));
        }
    }
}