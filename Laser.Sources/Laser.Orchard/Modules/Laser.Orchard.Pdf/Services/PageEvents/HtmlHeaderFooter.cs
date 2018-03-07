using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

namespace Laser.Orchard.Pdf.Services.PageEvents {
    public class HtmlHeaderFooter : PdfPageEventHelper {
        private ElementList headerList;
        private ElementList footerList;
        public HtmlHeaderFooter(string htmlHeader = "", string htmlFooter = "") {
            headerList = XMLWorkerHelper.ParseToElementList(htmlHeader ?? "", null);
            footerList = XMLWorkerHelper.ParseToElementList(htmlFooter ?? "", null);
        }
        public override void OnEndPage(PdfWriter writer, Document document) {
             if (headerList.Count > 0) {
                var ctHeader = new ColumnText(writer.DirectContent);
                ctHeader.SetSimpleColumn(document.LeftMargin, document.Top, document.Right, document.Top + document.TopMargin);
                foreach(var elem in headerList) {
                    ctHeader.AddElement(elem);
                }
                ctHeader.Go();
            }
            if (footerList.Count > 0) {
                var ctFooter = new ColumnText(writer.DirectContent);
                ctFooter.SetSimpleColumn(document.LeftMargin, document.Bottom - document.BottomMargin, document.Right, document.BottomMargin);
                foreach (var elem in footerList) {
                    ctFooter.AddElement(elem);
                }
                ctFooter.Go();
            }
        }
    }
}