using AutoMapper;
using Laser.Orchard.ShareLink.Models;
using Laser.Orchard.ShareLink.PartSettings;
using Laser.Orchard.ShareLink.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.Mvc.Extensions;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement.Handlers;
using Laser.Orchard.ShareLink.Servicies;

namespace Laser.Orchard.ShareLink.Drivers {

    public class ShareLinkPartDriver : ContentPartCloningDriver<ShareLinkPart> {
        private readonly ITokenizer _tokenizer;
        private readonly IOrchardServices _orchardServices;
        private readonly IShareLinkService _sharelinkservice;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        private readonly IContentManager _contentManager;

        protected override string Prefix {
            get { return "Laser.Orchard.ShareLink"; }
        }


        public ShareLinkPartDriver(IOrchardServices orchardServices, ITokenizer tokenizer, IContentManager contentManager, IShareLinkService sharelinkService) {
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _tokenizer = tokenizer;
            _contentManager = contentManager;
            _sharelinkservice = sharelinkService;
        }



        protected override DriverResult Editor(ShareLinkPart part, dynamic shapeHelper) {
            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);

            ShareLinkVM vm = new ShareLinkVM();
            Mapper.Initialize(cfg => {
                cfg.CreateMap<ShareLinkPart, ShareLinkVM>();
            });
            Mapper.Map<ShareLinkPart, ShareLinkVM>(part, vm);
            var moduleSetting = _orchardServices.WorkContext.CurrentSite.As<ShareLinkModuleSettingPart>();
            var partSetting = part.Settings.GetModel<ShareLinkPartSettingVM>();
            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
            if ((!partSetting.ShowTextChoise) || part.SharedText == "") {
                if (!string.IsNullOrEmpty(partSetting.SharedText)) {
 //                   vm.SharedText = _tokenizer.Replace(partSetting.SharedText, tokens);
                    vm.ShowSharedText = false;
                }
                else {
                    if (!string.IsNullOrEmpty(moduleSetting.SharedText)) {
 //                       vm.SharedText = _tokenizer.Replace(moduleSetting.SharedText, tokens);
                        vm.ShowSharedText = false;
                    }
                }
            }
            if (partSetting.ShowTextChoise) {
                vm.ShowSharedText = true;
            }

            if ((!partSetting.ShowLinkChoise) || part.SharedLink == "") {
                if (!string.IsNullOrEmpty(partSetting.SharedLink)) {
//                    vm.SharedLink = _tokenizer.Replace(partSetting.SharedLink, tokens);
                    vm.ShowSharedLink = false;
                }
                else {
                    if (!string.IsNullOrEmpty(moduleSetting.SharedLink)) {
//                        vm.SharedLink = _tokenizer.Replace(moduleSetting.SharedLink, tokens);
                        vm.ShowSharedLink = false;
                    }
                }
            }
            if (partSetting.ShowLinkChoise) {
                vm.ShowSharedLink = true;
            }

            string ListId = "";
            if (!string.IsNullOrEmpty(partSetting.SharedImage)) {
                ListId = _tokenizer.Replace(partSetting.SharedImage, tokens);
                vm.SharedImage = _sharelinkservice.GetImgUrl(ListId);
                vm.ShowSharedImage = false;
            }
            else {
                if (!string.IsNullOrEmpty(moduleSetting.SharedImage)) {
                   ListId = _tokenizer.Replace(moduleSetting.SharedImage, tokens);
                    vm.SharedImage = _sharelinkservice.GetImgUrl(ListId);
                    vm.ShowSharedImage = false;
                }
            }

            if (partSetting.ShowImageChoise) {
                vm.ShowSharedImage = true;

                List<OptionList> optionList = new List<OptionList>();
                var mylist = ListId.Replace("{", "").Replace("}", "").Split(',');
                foreach (string myid in mylist) {
                    //  lSelectList.Insert(0, new SelectListItem() { Value = fa.Id.ToString(), Text = fa.AccountType + " - " + fa.DisplayAs });
                    OptionList ol = new OptionList {
                        Value = myid,
                        Text = "",
                        ImageUrl = _sharelinkservice.GetImgUrl(myid),
                        Selected = part.SharedIdImage == myid ? "selected=\"selected\"" : ""
                    };
                    optionList.Add(ol);
                }
                vm.ListOption = optionList;
            }

            return ContentShape("Parts_ShareLink",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/ShareLink",
                                    Model: vm,
                                    Prefix: Prefix));
        }




        protected override DriverResult Editor(ShareLinkPart part, IUpdateModel updater, dynamic shapeHelper) {
            ShareLinkVM vm = new ShareLinkVM();
            updater.TryUpdateModel(vm, Prefix, null, null);
            Mapper.Initialize(cfg => {
                cfg.CreateMap<ShareLinkVM, ShareLinkPart>()
                .ForSourceMember(src => src.ShowSharedImage, opt => opt.Ignore())
                .ForSourceMember(src => src.ShowSharedLink, opt => opt.Ignore())
                .ForSourceMember(src => src.ShowSharedText, opt => opt.Ignore());
                    });
            Mapper.Map(vm, part);

            if (vm.SharedImage != null) {
                part.SharedIdImage = vm.SharedImage.Replace("{", "").Replace("}", "");
                part.SharedImage = _sharelinkservice.GetImgUrl(part.SharedIdImage);
            }

            return Editor(part, shapeHelper);
        }

        protected override void Cloning(ShareLinkPart originalPart, ShareLinkPart clonePart, CloneContentContext context) {
            clonePart.SharedLink = originalPart.SharedLink;
            clonePart.SharedText = originalPart.SharedText;
            //SharedImage is set based on SharedIdImage
            clonePart.SharedIdImage = originalPart.SharedIdImage;
        }

        protected override void Importing(ShareLinkPart part, ImportContentContext context) 
        {
            var importedSharedLink = context.Attribute(part.PartDefinition.Name, "SharedLink");
            if (importedSharedLink != null) {
                part.SharedLink = importedSharedLink;
            }

            var importedSharedText = context.Attribute(part.PartDefinition.Name, "SharedText");
            if (importedSharedText != null) {
                part.SharedText = importedSharedText;
            }

            var importedSharedImage = context.Attribute(part.PartDefinition.Name, "SharedImage");
            if (importedSharedImage != null) {
                part.SharedImage = importedSharedImage;
            }

            var importedSharedIdImage = context.Attribute(part.PartDefinition.Name, "SharedIdImage");
            if (importedSharedIdImage != null) {
                part.SharedIdImage = importedSharedIdImage;
            }
        }


        protected override void Exporting(ShareLinkPart part, ExportContentContext context) 
        {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("SharedIdImage", part.SharedIdImage);
            root.SetAttributeValue("SharedLink", part.SharedLink);
            root.SetAttributeValue("SharedText", part.SharedText);
            root.SetAttributeValue("SharedImage", part.SharedImage);
        }
    }
}