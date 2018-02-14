using Laser.Orchard.Pdf.Models;
using Laser.Orchard.Pdf.ViewModels;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Pdf.Drivers {
    public class PrintButtonPartDriver : ContentPartDriver<PrintButtonPart> {
        private readonly ITokenizer _tokenizer;
        protected override string Prefix {
            get { return "Laser.Orchard.Pdf"; }
        }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public PrintButtonPartDriver(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        protected override DriverResult Editor(PrintButtonPart part, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, shapeHelper);
        }
        protected override DriverResult Editor(PrintButtonPart part, dynamic shapeHelper) {
            var settings = part.Settings.GetModel<PrintButtonPartSettings>();
            var ctx = new Dictionary<string, object>() { { "Content", part.ContentItem } };
            var fileName = _tokenizer.Replace(settings.FileNameWithoutExtension, ctx);
            var model = new PrintButtonPartVM { TemplateId = settings.TemplateId, ContentId = part.Id, FileNameWithoutExtension = fileName };
            return ContentShape("Parts_PrintButtonPart", () => 
                shapeHelper.EditorTemplate(TemplateName: "Parts/PrintButtonPart", Model: model, Prefix: Prefix));
        }
    }
}