using AutoMapper;
using Laser.Orchard.Facebook.Models;
using Laser.Orchard.Facebook.Services;
using Laser.Orchard.Facebook.Settings;
using Laser.Orchard.Facebook.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.MediaLibrary.Models;

namespace Laser.Orchard.Facebook.Drivers {

    public class FacebookPostDriver : ContentPartDriver<FacebookPostPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IFacebookService _facebookService;
        private readonly ITokenizer _tokenizer;
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.Facebook"; }
        }

        public FacebookPostDriver(IOrchardServices orchardServices, IFacebookService facebookService, ITokenizer tokenizer) {
            _orchardServices = orchardServices;
            _facebookService = facebookService;
            _tokenizer = tokenizer;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(FacebookPostPart part, dynamic shapeHelper) {
            FacebookPostVM vm = new FacebookPostVM();
            Mapper.CreateMap<FacebookPostPart, FacebookPostVM>();
            Mapper.Map(part, vm);
            FacebookPostPartSettingVM setting = part.Settings.GetModel<FacebookPostPartSettingVM>();
            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
            if (!string.IsNullOrEmpty(setting.FacebookCaption)) {
                vm.FacebookCaption = _tokenizer.Replace(setting.FacebookCaption, tokens);
                vm.ShowFacebookCaption = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookDescription)) {
                vm.FacebookDescription = _tokenizer.Replace(setting.FacebookDescription, tokens);
                vm.ShowFacebookDescription = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookLink)) {
                vm.FacebookLink = _tokenizer.Replace(setting.FacebookLink, tokens);
                vm.ShowFacebookLink = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookMessage)) {
                vm.FacebookMessage = _tokenizer.Replace(setting.FacebookMessage, tokens);
                vm.ShowFacebookMessage = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookName)) {
                vm.FacebookName = _tokenizer.Replace(setting.FacebookName, tokens);
                vm.ShowFacebookName = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookPicture)){
                vm.ShowFacebookPicture = false;
                string idimg = _tokenizer.Replace(setting.FacebookPicture, tokens);
                Int32 idimage = 0;
             
                    Int32.TryParse(idimg.Replace("{", "").Replace("}", "").Split(',')[0], out idimage); ;
                    if (idimage > 0) { 
                // _orchardServices.ContentManager.Get(id);
                    // vm.Image = Url.ItemDisplayUrl(_orchardServices.ContentManager.Get(id));
                    var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                    //       vm.Image = urlHelper.ItemDisplayUrl(_orchardServices.ContentManager.Get(id));// get current display link
                    //   Fvm.Link = urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(Twitterpart));// get current display link
                    var ContentImage = _orchardServices.ContentManager.Get(idimage, VersionOptions.Published);
                    //   var pathdocument = Path.Combine(ContentImage.As<MediaPart>().FolderPath, ContentImage.As<MediaPart>().FileName);
                    //  part.TwitterPicture = pathdocument;// 
                    //  vm.Image =
                    //   .ResizeMediaUrl(Width: previewWidth, Height: previewHeight, Mode: "crop", Alignment: "middlecenter", Path: Model.MediaData.MediaUrl)');

                    vm.FacebookPicture = urlHelper.MakeAbsolute(ContentImage.As<MediaPart>().MediaUrl);
                }
                else
                    vm.FacebookPicture = "";

            }
            else
                vm.FacebookPicture = part.FacebookPicture;
            List<FacebookAccountPart> listaccount = _facebookService.GetValidFacebookAccount();
            List<SelectListItem> lSelectList = new List<SelectListItem>();
            foreach (FacebookAccountPart fa in listaccount) {
                lSelectList.Insert(0, new SelectListItem() { Value = fa.Id.ToString(), Text = fa.AccountType + " - " + fa.DisplayAs });
            }
            if (lSelectList.Count > 0) {
                vm.SelectedList = part.AccountList.Select(x => x.ToString()).ToArray();

                vm.FacebookAccountList = new SelectList((IEnumerable<SelectListItem>)lSelectList, "Value", "Text", vm.SelectedList);
            }

            return ContentShape("Parts_FacebookPost",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/FacebookPost",
                                    Model: vm,
                                    Prefix: Prefix));
        }

        protected override DriverResult Editor(FacebookPostPart part, IUpdateModel updater, dynamic shapeHelper) {
            FacebookPostVM vm = new FacebookPostVM();
            updater.TryUpdateModel(vm, Prefix, null, null);
            Mapper.CreateMap<FacebookPostVM, FacebookPostPart>();
            Mapper.Map(vm, part);
            part.AccountList = vm.SelectedList.Select(x => Int32.Parse(x)).ToArray();
            return Editor(part, shapeHelper);
        }
    }
}