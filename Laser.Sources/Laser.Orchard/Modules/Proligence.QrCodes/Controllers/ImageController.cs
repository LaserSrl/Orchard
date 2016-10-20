namespace Proligence.QrCodes.Controllers
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Web.Mvc;

    using Gma.QrCodeNet.Encoding;
    using Gma.QrCodeNet.Encoding.Windows.Render;

    using Orchard.ContentManagement;
    using Orchard.Localization;
    using Orchard.Themes;
    using Orchard.UI.Admin;

    using Proligence.QrCodes.Models;

    [Themed(false)]
    public class ImageController : Controller
    {
        private readonly IContentManager _contentManager;

        public ImageController(IContentManager contentManager)
        {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Render(int id)
        {
            var item = _contentManager.Get(id);

            if (item == null || !item.Has<QrCodePart>())
                return HttpNotFound();

            var qrEncoder = new QrEncoder(ErrorCorrectionLevel.M);
            var qrCode = new QrCode();
            if(!qrEncoder.TryEncode(item.As<QrCodePart>().ActualValue, out qrCode))
                return HttpNotFound();

            var renderer = new GraphicsRenderer(new FixedCodeSize(item.As<QrCodePart>().Size, QuietZoneModules.Zero), Brushes.Black, Brushes.White);
            var stream = new MemoryStream();
            renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);
            stream.Position = 0;

            return new FileStreamResult(stream, "image/png");
        }
    }
}