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
using Laser.Orchard.ShareLink.Servicies;

namespace Laser.Orchard.ShareLink.Handlers {

    public class ShareLinkHandler : ContentHandler {
        private readonly ITokenizer _tokenizer;
        private readonly IOrchardServices _orchardServices;
        private readonly IShareLinkService _sharelinkService;
        public ShareLinkHandler(IRepository<ShareLinkPartRecord> repository, ITokenizer tokenizer, IOrchardServices orchardServices, IShareLinkService sharelinkService) {
            Filters.Add(StorageFilter.For(repository));
            _orchardServices = orchardServices;
            _tokenizer = tokenizer;
            _sharelinkService = sharelinkService;
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
                        part.SharedImage = _sharelinkService.GetImgUrl(ListId);

                    }
                    else {
                        if (!string.IsNullOrEmpty(moduleSetting.SharedImage)) {
                            ListId = _tokenizer.Replace(moduleSetting.SharedImage, tokens);
                            part.SharedImage = _sharelinkService.GetImgUrl(ListId);

                        }
                    }

                    part.SharedIdImage = part.SharedImage.Replace("{", "").Replace("}", "");
                    part.SharedImage = _sharelinkService.GetImgUrl(part.SharedIdImage);
                }
            });

        }
    }
}