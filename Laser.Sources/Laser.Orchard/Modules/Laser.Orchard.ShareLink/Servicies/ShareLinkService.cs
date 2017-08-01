using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using Laser.Orchard.ShareLink.Models;
using Laser.Orchard.ShareLink.PartSettings;
using Orchard;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Models;
using Orchard.Mvc.Extensions;
using Orchard.Tokens;

namespace Laser.Orchard.ShareLink.Servicies {

    public interface IShareLinkService : IDependency {
        string GetImgUrl(string idimg);
        void FillPart(ShareLinkPart part);
    }

    public class ShareLinkService : IShareLinkService {
        private readonly IOrchardServices _orchardServices;
        private readonly ITokenizer _tokenizer;
        public ShareLinkService(IOrchardServices orchardServicies, ITokenizer tokenizer) {
            _orchardServices = orchardServicies;
            _tokenizer = tokenizer;
        }

        private string RemoveHtmlTag(string text) {
            if (string.IsNullOrEmpty(text))
                return "";
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(text);
            return (htmlDoc.DocumentNode.InnerText);
        }
        public void FillPart(ShareLinkPart part) {
            var moduleSetting = _orchardServices.WorkContext.CurrentSite.As<ShareLinkModuleSettingPart>();
            var partSetting = part.Settings.GetModel<ShareLinkPartSettingVM>();
            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
            if ((!partSetting.ShowBodyChoise) || part.SharedBody == "") {
                if (!string.IsNullOrEmpty(partSetting.SharedBody)) {
                    part.SharedBody = _tokenizer.Replace(partSetting.SharedBody, tokens);
                }
                else {
                    if (!string.IsNullOrEmpty(moduleSetting.SharedBody)) {
                        part.SharedBody = _tokenizer.Replace(moduleSetting.SharedBody, tokens);
                    }
                }
                var s=HttpUtility.HtmlDecode(part.SharedBody);
                part.SharedBody = RemoveHtmlTag(s);
            }

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
                    part.SharedImage = GetImgUrl(ListId);

                }
                else {
                    if (!string.IsNullOrEmpty(moduleSetting.SharedImage)) {
                        ListId = _tokenizer.Replace(moduleSetting.SharedImage, tokens);
                        part.SharedImage = GetImgUrl(ListId);

                    }
                }

                part.SharedIdImage = part.SharedImage.Replace("{", "").Replace("}", "");
                part.SharedImage = GetImgUrl(part.SharedIdImage);
            }
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