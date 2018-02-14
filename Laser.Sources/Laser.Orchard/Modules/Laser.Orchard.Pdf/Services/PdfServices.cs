using Orchard;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Laser.Orchard.Pdf.Services {
    public interface IPdfServices : IDependency {
        byte[] PdfFromHtml(string html);
    }
    public class PdfServices : IPdfServices {
        public byte[] PdfFromHtml(string html) {
            byte[] buffer = null;
            using (var memoryStream = new MemoryStream()) {
                using (var document = new Document(PageSize.A4, 50, 50, 10, 10)) {
                    using (var writer = PdfWriter.GetInstance(document, memoryStream)) {
                        using (var sr = new StringReader(html)) {
                            document.Open();
                            XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, sr);
                            document.Close();
                            buffer = memoryStream.ToArray();
                        }
                    }
                }
            }
            return buffer;
        }
    }
}