using Laser.Orchard.IXMSD.Models;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Models;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.IXMSD.Controllers {

    public class VideoController : ApiController {
        private readonly IOrchardServices _orchardServices;
        private readonly IUtilsServices _utilsServices;
        public VideoController(IOrchardServices orchardServices,IUtilsServices utilsServices) {
            _orchardServices = orchardServices;
            _utilsServices = utilsServices;
        }

        public Response Get(string nomefile, string NewUrl) {
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.Headers.GetValues("x-frame-options").ToString() )) {
                if (HttpContext.Current.Request.Headers.GetValues("x-frame-options")[0].ToString() == "SAMEORIGIN") {
                    var allmediapart = _orchardServices.ContentManager.Query<MediaPart, MediaPartRecord>().Where(x => x.FileName == nomefile).List().ToList();
                    foreach (MediaPart mp in allmediapart) {
                        mp.ContentItem.As<IXMSDPart>().ExternalMediaUrl = NewUrl;
                    }
                    return (_utilsServices.GetResponse(ResponseType.Success));
                }
                else
                    return (_utilsServices.GetResponse(ResponseType.UnAuthorized));
            }
            else
                return (_utilsServices.GetResponse(ResponseType.UnAuthorized));

        }
    }
}