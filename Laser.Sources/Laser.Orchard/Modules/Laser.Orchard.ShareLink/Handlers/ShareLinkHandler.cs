using Laser.Orchard.ShareLink.Models;
using Laser.Orchard.ShareLink.PartSettings;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.MediaLibrary.Models;

namespace Laser.Orchard.ShareLink.Handlers {

    public class ShareLinkHandler : ContentHandler {
        private readonly ITokenizer _tokenizer;
        private readonly IOrchardServices _orchardServices;
        public ShareLinkHandler(IRepository<ShareLinkPartRecord> repository, ITokenizer tokenizer, IOrchardServices orchardServices) {
            Filters.Add(StorageFilter.For(repository));
            _orchardServices = orchardServices;
            _tokenizer = tokenizer;
            OnGetDisplayShape<ShareLinkPart>((context, part) => {
                var moduleSetting = _orchardServices.WorkContext.CurrentSite.As<ShareLinkModuleSettingPart>();
                var partSetting = part.Settings.GetModel<ShareLinkPartSettingVM>();
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                if ((!partSetting.ShowTextChoise) || part.SharedText == "") {
                    if (!string.IsNullOrEmpty(partSetting.SharedText)) {
                        part.SharedText = _tokenizer.Replace(partSetting.SharedText, tokens);
                    }
                    else {
                        if (!string.IsNullOrEmpty(moduleSetting.SharedText)) {
                            part.SharedText = _tokenizer.Replace(moduleSetting.SharedText, tokens);
                        }
                    }
                }
                if ((!partSetting.ShowLinkChoise) || part.SharedLink == "") {
                    if (!string.IsNullOrEmpty(partSetting.SharedLink)) {
                        part.SharedLink = _tokenizer.Replace(partSetting.SharedLink, tokens);

                    }
                    else {
                        if (!string.IsNullOrEmpty(moduleSetting.SharedLink)) {
                            part.SharedLink = _tokenizer.Replace(moduleSetting.SharedLink, tokens);
                        }
                    }
                }

                string ListId = "";
                if (!(partSetting.ShowImageChoise)) {
                    if (!string.IsNullOrEmpty(partSetting.SharedImage)) {
                        ListId = _tokenizer.Replace(partSetting.SharedImage, tokens);
                        part.SharedImage = getimgurl(ListId);

                    }
                    else {
                        if (!string.IsNullOrEmpty(moduleSetting.SharedImage)) {
                            ListId = _tokenizer.Replace(moduleSetting.SharedImage, tokens);
                            part.SharedImage = getimgurl(ListId);

                        }
                    }

                    part.SharedIdImage = part.SharedImage.Replace("{", "").Replace("}", "");
                    part.SharedImage = getimgurl(part.SharedIdImage);
                }
            });

        }
        private string getimgurl(string idimg) {
            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            Int32 idimage = 0;
            Int32.TryParse(idimg.Replace("{", "").Replace("}", "").Split(',')[0], out idimage); ;
            if (idimage > 0) {
                var ContentImage = _orchardServices.ContentManager.Get(idimage, VersionOptions.Published);
                return urlHelper.MakeAbsolute(ContentImage.As<MediaPart>().MediaUrl);
            }
            else
                return idimg; // non ho passato un id e quindi sarà un link
        }
    }
}