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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="html">HTML content to populate pdf document.</param>
        /// <param name="pageSize">Size of pdf page. Valid values: A5, A4, A3, A2, A1, A0. Default: "A4"</param>
        /// <param name="marginLeft">Default 50.</param>
        /// <param name="marginRight">Default 50.</param>
        /// <param name="marginTop">Default 10.</param>
        /// <param name="marginBottom">Default 10.</param>
        /// <param name="landscape">Default false.</param>
        /// <returns></returns>
        byte[] PdfFromHtml(string html, string pageSize = "A4", int marginLeft = 50, int marginRight = 50, int marginTop = 10, int marginBottom = 10, bool landscape = false);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="html">HTML content to populate pdf document.</param>
        /// <param name="width">Page width.</param>
        /// <param name="height">Page height.</param>
        /// <param name="marginLeft">Default 50.</param>
        /// <param name="marginRight">Default 50.</param>
        /// <param name="marginTop">Default 10.</param>
        /// <param name="marginBottom">Default 10.</param>
        /// <returns></returns>
        byte[] PdfFromHtml(string html, float width, float height, int marginLeft = 50, int marginRight = 50, int marginTop = 10, int marginBottom = 10);
    }
    public class PdfServices : IPdfServices {
        public byte[] PdfFromHtml(string html, string pageSize = "A4", int marginLeft = 50, int marginRight = 50, int marginTop = 10, int marginBottom = 10, bool landscape = false) {
            var effectivePageSize = PageSize.A4;
            switch (pageSize.ToUpper()) {
                case "A5":
                    effectivePageSize = PageSize.A5;
                    break;
                case "A4":
                    effectivePageSize = PageSize.A4;
                    break;
                case "A3":
                    effectivePageSize = PageSize.A3;
                    break;
                case "A2":
                    effectivePageSize = PageSize.A2;
                    break;
                case "A1":
                    effectivePageSize = PageSize.A1;
                    break;
                case "A0":
                    effectivePageSize = PageSize.A0;
                    break;
            }
            if (landscape) {
                effectivePageSize = effectivePageSize.Rotate();
            }
            return PdfFromHtml(html, effectivePageSize.Width, effectivePageSize.Height, marginLeft, marginRight, marginTop, marginBottom);
        }
        public byte[] PdfFromHtml(string html, float width, float height, int marginLeft = 50, int marginRight = 50, int marginTop = 10, int marginBottom = 10) {
            var effectivePageSize = new Rectangle(width, height);
            byte[] buffer = null;
            using (var memoryStream = new MemoryStream()) {
                using (var document = new Document(effectivePageSize, 50, 50, 10, 10)) {
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