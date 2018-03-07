using System.IO;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline;

namespace Laser.Orchard.Pdf.Services.PageEvents {
    public class HtmlHeaderFooter : PdfPageEventHelper {
        private string originalheader;
        private string originalfooter;
        public HtmlHeaderFooter(string htmlHeader = "", string htmlFooter = "") {
            originalheader = HttpUtility.HtmlDecode(htmlHeader);
            originalfooter= HttpUtility.HtmlDecode(htmlFooter);       
        }
        public override void OnEndPage(PdfWriter writer, Document document) {
            var ct = new ColumnText(writer.DirectContent);
             if (!string.IsNullOrEmpty(originalheader)) {
                XMLWorkerHelper.GetInstance().ParseXHtml(new ColumnTextElementHandler(ct), new StringReader(originalheader));
                ct.SetSimpleColumn(document.LeftMargin, document.Top, document.Right, document.Top + document.TopMargin);
                ct.Go();
            }
            if (!string.IsNullOrEmpty(originalfooter)) {
                XMLWorkerHelper.GetInstance().ParseXHtml(new ColumnTextElementHandler(ct), new StringReader(originalfooter));
                ct.SetSimpleColumn(document.LeftMargin, document.Bottom - document.BottomMargin, document.Right, document.BottomMargin);
                ct.Go();
            }
        }
        public class ColumnTextElementHandler : IElementHandler {
            public ColumnTextElementHandler(ColumnText ct) {
                this.ct = ct;
            }
            ColumnText ct = null;
            public void Add(IWritable w) {
                if (w is WritableElement) {
                    foreach (IElement e in ((WritableElement)w).Elements()) {
                        ct.AddElement(e);
                    }
                }
            }
        }
    }
}