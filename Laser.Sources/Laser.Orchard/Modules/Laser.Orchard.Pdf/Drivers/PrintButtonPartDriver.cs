using Laser.Orchard.Pdf.Models;
using Laser.Orchard.Pdf.ViewModels;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Pdf.Drivers {
    public class PrintButtonPartDriver : ContentPartDriver<PrintButtonPart> {
        protected override string Prefix {
            get { return "Laser.Orchard.Pdf"; }
        }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public PrintButtonPartDriver(ITemplateService templateservice) {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        //protected override DriverResult Display(PrintButtonPart part, string displayType, dynamic shapeHelper) {
        //    if (displayType == "Detail") {
        //        return ContentShape("Parts_PrintButtonPart",
        //            () => shapeHelper.Parts_PrintButtonPart());

        //    } else {
        //        return null;
        //    }
        //}
        protected override DriverResult Editor(PrintButtonPart part, IUpdateModel updater, dynamic shapeHelper) {
            //if(HttpContext.Current.Request.Form["submit.Save"] == "PdfPrintButton") {
            //    var settings = part.Settings.GetModel<PrintButtonPartSettings>();
            //    ParseTemplateContext templateCtx = new ParseTemplateContext();
            //    var template = _templateService.GetTemplate(settings.TemplateId);
            //    var editModel = new Dictionary<string, object>();
            //    editModel.Add("Content", part.ContentItem);
            //    templateCtx.Model = editModel;
            //    var body = _templateService.ParseTemplate(template, templateCtx);
            //    HttpContext.Current.Response.
            //}
            return Editor(part, shapeHelper);
        }
        protected override DriverResult Editor(PrintButtonPart part, dynamic shapeHelper) {
            var settings = part.Settings.GetModel<PrintButtonPartSettings>();
            var model = new PrintButtonPartVM { TemplateId = settings.TemplateId, ContentId = part.Id };
            return ContentShape("Parts_PrintButtonPart", () => 
                shapeHelper.EditorTemplate(TemplateName: "Parts/PrintButtonPart", Model: model, Prefix: Prefix));
        }
    }
}