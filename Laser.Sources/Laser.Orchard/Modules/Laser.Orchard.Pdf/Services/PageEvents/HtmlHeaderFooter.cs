using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

namespace Laser.Orchard.Pdf.Services.PageEvents {
    public class HtmlHeaderFooter : PdfPageEventHelper {
        private ElementList header;
        private ElementList footer;
        public HtmlHeaderFooter(string htmlHeader = "", string htmlFooter = "") {
            header = XMLWorkerHelper.ParseToElementList(htmlHeader ?? "", null);
            if (string.IsNullOrWhiteSpace(htmlHeader) == false && header.Count == 0) {
                // header was not in html format so bring it in html format
                var htmlHeader2 = string.Format("<p>{0}</p>", htmlHeader);
                header = XMLWorkerHelper.ParseToElementList(htmlHeader2, null);
            }
            footer = XMLWorkerHelper.ParseToElementList(htmlFooter ?? "", null);
            if (string.IsNullOrWhiteSpace(htmlFooter) == false && footer.Count == 0) {
                // footer was not in html format so bring it in html format
                var htmlFooter2 = string.Format("<p>{0}</p>", htmlFooter);
                footer = XMLWorkerHelper.ParseToElementList(htmlFooter2, null);
            }
        }
        public override void OnEndPage(PdfWriter writer, Document document) {
            var ct = new ColumnText(writer.DirectContent);
            if (header.Count > 0) {
                ct.SetSimpleColumn(document.LeftMargin, document.Top, document.Right, document.Top + document.TopMargin);
                foreach (var el in header) {
                    ct.AddElement(el);
                }
                ct.Go();
            }
            if (footer.Count > 0) {
                ct.SetSimpleColumn(document.LeftMargin, document.Bottom - document.BottomMargin, document.Right, document.BottomMargin);
                foreach (var el in footer) {
                    ct.AddElement(el);
                }
                ct.Go();
            }
        }
    }
}