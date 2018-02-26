using Laser.Orchard.Pdf.Models;
using Laser.Orchard.Pdf.Services;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Core.Contents;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;


namespace Laser.Orchard.Pdf.Controllers {
    public class PdfController : Controller {
        private readonly IPdfServices _pdfServices;
        private readonly ITemplateService _templateService;
        private readonly IOrchardServices _orchardServices;
        private readonly ITokenizer _tokenizer;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public PdfController(IPdfServices pdfServices, ITemplateService templateService, IOrchardServices orchardServices, ITokenizer tokenizer) {
            _pdfServices = pdfServices;
            _tokenizer = tokenizer;
            _templateService = templateService;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        private ActionResult GeneratePdf(ContentItem ci) {
            if (ci != null) {
                if (_orchardServices.Authorizer.Authorize(Permissions.ViewContent, ci)) {
                   var part = ci.Parts.FirstOrDefault(x => x.PartDefinition.Name == typeof(PdfButtonPart).Name);
                    if (part != null) {
                        //var settings = part.Settings.GetModel<PdfButtonPartSettings>();
                        //ParseTemplateContext templateCtx = new ParseTemplateContext();
                        //var template = _templateService.GetTemplate(settings.TemplateId);
                        //var editModel = new Dictionary<string, object>();
                        //editModel.Add("Content", ci);
                        //templateCtx.Model = editModel;
                        //var html = _templateService.ParseTemplate(template, templateCtx);
                        var settings = part.Settings.GetModel<PdfButtonPartSettings>();
                        var html = _templateService.RitornaParsingTemplate(ci, settings.TemplateId);
                        var editModel = new Dictionary<string, object>();
                        editModel.Add("Content", ci);
                        var header = _tokenizer.Replace(settings.Header, editModel);
                        var footer = _tokenizer.Replace(settings.Footer, editModel);
                        byte[] buffer = null;
                        if (string.IsNullOrWhiteSpace(header) && string.IsNullOrWhiteSpace(footer)) {
                            buffer = _pdfServices.PdfFromHtml(html, settings.PageWidth, settings.PageHeight, settings.LeftMargin, settings.RightMargin, settings.HeaderHeight, settings.FooterHeight);
                        }
                        else {
                            var headerFooter = _pdfServices.GetHtmlHeaderFooterPageEvent(header, footer);
                            buffer = _pdfServices.PdfFromHtml(html, settings.PageWidth, settings.PageHeight, settings.LeftMargin, settings.RightMargin, settings.HeaderHeight, settings.FooterHeight, headerFooter);
                        }
                        var fileName = _tokenizer.Replace(settings.FileNameWithoutExtension, editModel);
                        fileName = string.Format("{0}.pdf", (string.IsNullOrWhiteSpace(fileName) ? "page" : fileName.Trim()));
                        return File(buffer, "application/pdf", fileName);
                    }
                }
                else {
                    return new HttpUnauthorizedResult();
                }
            }
            // fallback
            var htmlError = "Please save your content to generate PDF.";
            return Content(htmlError, "text/html", Encoding.UTF8);
        }


        public ActionResult Generate(int id) {
            var ci = _orchardServices.ContentManager.Get(id, VersionOptions.Published);
            return GeneratePdf(ci);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Preview(int id) {
            var ci = _orchardServices.ContentManager.Get(id, VersionOptions.Latest);
            return GeneratePdf(ci);
        }

        //public ActionResult Preview(int id) {
        //    ContentPart part = null;
        //    var ci = _orchardServices.ContentManager.Get(id, VersionOptions.Latest);
        //    if (ci != null) {
        //        if (_orchardServices.Authorizer.Authorize(Permissions.ViewContent, ci)) {
        //            part = ci.Parts.FirstOrDefault(x => x.PartDefinition.Name == typeof(PdfButtonPart).Name);
        //            if (part != null) {
        //                var settings = part.Settings.GetModel<PdfButtonPartSettings>();
        //                var html = _templateService.RitornaParsingTemplate(ci, settings.TemplateId);
        //                return Content(html, "text/html", Encoding.UTF8);
        //            }
        //        }
        //        else {
        //            return new HttpUnauthorizedResult();
        //        }
        //    }
        //    // fallback
        //    var htmlError = "Please save your content to see the preview.";
        //    return Content(htmlError, "text/html", Encoding.UTF8);
        //}
    }
}