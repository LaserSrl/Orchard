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
using Orchard.Logging;


namespace Laser.Orchard.IXMSD.Controllers {

    public class VideoController : ApiController {
        private readonly IOrchardServices _orchardServices;
        private readonly IUtilsServices _utilsServices;
        public ILogger _Logger { get; set; }

        public VideoController(IOrchardServices orchardServices,IUtilsServices utilsServices) {
            _orchardServices = orchardServices;
            _utilsServices = utilsServices;
            _Logger = NullLogger.Instance;
        }

        public Response Get(string nomefile, string NewUrl) {
#if DEBUG
         _Logger.Error("Richiesta modifica del video" +nomefile +"con url"+NewUrl);            
#endif
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.Headers.GetValues("x-frame-options").ToString() )) {
#if DEBUG
         _Logger.Error("x-frame-options:"+HttpContext.Current.Request.Headers.GetValues("x-frame-options").ToString());            
#endif
                if (HttpContext.Current.Request.Headers.GetValues("x-frame-options")[0].ToString() == "SAMEORIGIN") {
#if DEBUG
         _Logger.Error("x-frame-options[0]:"+HttpContext.Current.Request.Headers.GetValues("x-frame-options")[0].ToString());            
#endif
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