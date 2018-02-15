using Laser.Orchard.Pdf.Services;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;
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
        public PdfController(IPdfServices pdfServices, ITemplateService templateService, IOrchardServices orchardServices) {
            _pdfServices = pdfServices;
            _templateService = templateService;
            _orchardServices = orchardServices;
        }
        public ActionResult Generate(int tid, int cid, string fn) {
            ParseTemplateContext templateCtx = new ParseTemplateContext();
            var template = _templateService.GetTemplate(tid);
            var ci = _orchardServices.ContentManager.Get(cid);
            var editModel = new Dictionary<string, object>();
            editModel.Add("Content", ci);
            templateCtx.Model = editModel;
            var html = _templateService.ParseTemplate(template, templateCtx);
            var headerFooter = _pdfServices.GetHtmlHeaderFooterPageEvent("<img src=\"http://localhost/Laser.Orchard/Media/AppRetailTest/menu/titolo.png\" style=\"width:300px;padding-top:20px\" />", 
                "<img src=\"http://localhost/Laser.Orchard/Media/AppRetailTest/menu/gallery.png\" />");
            var buffer = _pdfServices.PdfFromHtml(html, "A4", 50, 50, 100, 200, false, headerFooter);
            var fileName = string.Format("{0}.pdf", (fn ?? "page"));
            return File(buffer, "application/pdf", fileName);
        }
        public ActionResult Preview(int tid, int cid) {
            ParseTemplateContext templateCtx = new ParseTemplateContext();
            var template = _templateService.GetTemplate(tid);
            var ci = _orchardServices.ContentManager.Get(cid);
            var editModel = new Dictionary<string, object>();
            editModel.Add("Content", ci);
            templateCtx.Model = editModel;
            var html = _templateService.ParseTemplate(template, templateCtx);
            return Content(html, "text/html", Encoding.UTF8);
        }
    }
}