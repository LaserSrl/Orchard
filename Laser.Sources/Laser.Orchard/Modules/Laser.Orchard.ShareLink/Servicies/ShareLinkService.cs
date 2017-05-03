using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Models;
using Orchard.Mvc.Extensions;

namespace Laser.Orchard.ShareLink.Servicies {

    public interface IShareLinkService : IDependency {
        string GetImgUrl(string idimg);
    }

    public class ShareLinkService : IShareLinkService{
        private readonly IOrchardServices _orchardServices;

        public ShareLinkService(IOrchardServices orchardServicies) {
            _orchardServices = orchardServicies;
        }

        public string GetImgUrl(string idimg) {
            if (idimg != null) {
                var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                Int32 idimage = 0;
                string[] contentListId = idimg.Replace("{", "").Replace("}", "").Split(',');
                for (int i = 0; i < contentListId.Length; i++) {
                    Int32.TryParse(contentListId[i], out idimage);
                    if (idimage > 0) {
                        var ContentImage = _orchardServices.ContentManager.Get(idimage, VersionOptions.Published);
                        if (ContentImage != null) {
                            return urlHelper.MakeAbsolute(ContentImage.As<MediaPart>().MediaUrl);
                        }
                    }
                    else {
                        return idimg;   // non ho passato un id e quindi sarà un link  
                    }
                }
            }
            return ""; //idimg null o non ci sono immagini pubblicate
        }
    }
}