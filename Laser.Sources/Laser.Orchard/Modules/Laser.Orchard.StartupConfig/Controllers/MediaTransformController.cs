using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;
using Orchard.MediaLibrary.Models;
using Orchard.MediaProcessing.Models;
using Orchard.MediaProcessing.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.Controllers {

    [OrchardFeature("Laser.Orchard.StartupConfig.MediaExtensions")]
    public class MediaTransformController : Controller {
        private readonly Work<IImageProfileManager> _imageProfileManager;
        private readonly IOrchardServices _orchardServices;

        public MediaTransformController(Work<IImageProfileManager> imageProfileManager, IOrchardServices orchardServices) {
            _imageProfileManager = imageProfileManager;
            _orchardServices = orchardServices;
        }

        public RedirectResult Image(string Path, int Width, int Height, string Mode, string Alignment, string PadColor) {
            int n = 0;
            bool isNumeric = int.TryParse(Path, out n);
            if (isNumeric) {
                MediaPart mediaPart = ((ContentItem)_orchardServices.ContentManager.Get(n)).As<MediaPart>();
                Path = mediaPart.MediaUrl;
            }
            var state = new Dictionary<string, string> {
                {"Width", Width.ToString(CultureInfo.InvariantCulture)},
                {"Height", Height.ToString(CultureInfo.InvariantCulture)},
                {"Mode", Mode},
                {"Alignment", Alignment},
                {"PadColor", PadColor},
            };

            var filter = new FilterRecord {
                Category = "Transform",
                Type = "Resize",
                State = FormParametersHelper.ToString(state)
            };

            var profile = "Transform_Resize"
                + "_w_" + Convert.ToString(Width)
                + "_h_" + Convert.ToString(Height)
                + "_m_" + Convert.ToString(Mode)
                + "_a_" + Convert.ToString(Alignment)
                + "_c_" + Convert.ToString(PadColor);
            Path = HttpUtility.UrlDecode(Path);
            var url = _imageProfileManager.Value.GetImageProfileUrl(Path, profile, filter);

            return Redirect(url);
        }
    }
}