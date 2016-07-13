using Laser.Orchard.Vimeo.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.Vimeo.Services;

namespace Laser.Orchard.Vimeo.Handlers {

    public class VimeoSettingsPartHandler : ContentHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly IVimeoContentServices _vimeoContentServices;

        public VimeoSettingsPartHandler(IRepository<VimeoSettingsPartRecord> repository, IOrchardServices orchardServices, IVimeoContentServices vimeoContentServices) {
            Filters.Add(new ActivatingFilter<VimeoSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));

            _orchardServices = orchardServices;
            _vimeoContentServices = vimeoContentServices;

            ////ondisplaying of a mediapart containing a vimeo video, check controller: if the request is coming from a mobile
            ////app we need to send a crypted URL, rather than the oEmbed, because we cannot whitelist apps
            //OnGetDisplayShape<OEmbedPart>((context, part) => {
            //    if (!string.IsNullOrWhiteSpace(part["provider_name"]) && part["provider_name"] == "Vimeo") {
            //        if (context.DisplayType == "Detail") {
            //            //part["html"] = "<iframe src=\"https://player.vimeo.com/video/480452\" width=\"640\" height=\"361\" frameborder=\"0\" webkitallowfullscreen mozallowfullscreen allowfullscreen></iframe>";
            //        }
            //    }
            //});
        }

        protected override void BuildDisplayShape(BuildDisplayContext context) {
            if (_orchardServices.WorkContext.HttpContext.Request.Url.AbsolutePath
                .EndsWith("Laser.Orchard.WebServices/Json/GetByAlias", StringComparison.InvariantCultureIgnoreCase)) {
                    if (context.DisplayType == "Detail") {
                        var partsWithMediaFields = context
                            .ContentItem
                            .Parts
                            .Where(p => p.Fields.Count() > 0)
                            .Where(p => p.Fields.Any(f => f.FieldDefinition.Name == "MediaLibraryPickerField"))
                            .ToList();
                        foreach (var part in partsWithMediaFields) {
                            foreach (var field in part.Fields.Where(f => f.FieldDefinition.Name == "MediaLibraryPickerField")) {
                                foreach (var mPart in ((MediaLibraryPickerField)field).MediaParts) {
                                    var oePart = mPart.As<OEmbedPart>();
                                    if (oePart != null &&
                                        !string.IsNullOrWhiteSpace(oePart["provider_name"]) &&
                                        oePart["provider_name"] == "Vimeo") {
                                        //we recompute the video stream's url, because it may have expired
                                        oePart.Source = "Vimeo|" + _vimeoContentServices.EncryptedVideoUrl(_vimeoContentServices.ExtractVimeoStreamURL(oePart));
                                    }
                                }
                            }
                        }
                    }
            }
            
            base.BuildDisplayShape(context);
        }
    }

}