using AutoMapper;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.Twitter.Models;
using Laser.Orchard.Twitter.Services;
using Laser.Orchard.Twitter.Settings;
using Laser.Orchard.Twitter.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Fields;
using Orchard.MediaLibrary.Models;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.Twitter.Drivers {

    public class TwitterPostDriver : ContentPartDriver<TwitterPostPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly ITwitterService _TwitterService;
        private readonly IProviderConfigurationService _providerConfigurationService;
        private readonly ITokenizer _tokenizer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IStorageProvider _storageProvider;
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.Twitter"; }
        }

        public TwitterPostDriver(IStorageProvider storageProvider, IOrchardServices orchardServices, ITwitterService TwitterService, IProviderConfigurationService providerConfigurationService, ITokenizer tokenizer, IHttpContextAccessor httpContextAccessor, IControllerContextAccessor controllerContextAccessor) {
            _storageProvider = storageProvider;
            _httpContextAccessor = httpContextAccessor;
            _controllerContextAccessor = controllerContextAccessor;
            _tokenizer = tokenizer;
            _orchardServices = orchardServices;
            _TwitterService = TwitterService;
            _providerConfigurationService = providerConfigurationService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(TwitterPostPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "Detail") {
                ProviderConfigurationRecord pcr = _providerConfigurationService.Get("Twitter");
                TwitterOgVM vm = new TwitterOgVM();
                vm.Site = pcr.UserIdentifier;
                TwitterPostPartSettingVM setting = part.Settings.GetModel<TwitterPostPartSettingVM>();
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                if (!string.IsNullOrEmpty(setting.Description))
                    vm.Description = _tokenizer.Replace(setting.Description, tokens);
                else
                    vm.Description = part.TwitterDescription;
                if (!string.IsNullOrEmpty(setting.Image))
                    vm.Image = _tokenizer.Replace(setting.Image, tokens);
                else
                    vm.Image = part.TwitterPicture;
                if (!string.IsNullOrEmpty(setting.Title))
                    vm.Title = _tokenizer.Replace(setting.Title, tokens);
                else
                    vm.Title = part.TwitterTitle;
                return ContentShape("Parts_TwitterPost_Detail",
                    () => shapeHelper.Parts_TwitterPost_Detail(Twitter: vm));
            }
            else
                return null;
        }

        protected override DriverResult Editor(TwitterPostPart part, dynamic shapeHelper) {
            TwitterPostVM vm = new TwitterPostVM();
            Mapper.CreateMap<TwitterPostPart, TwitterPostVM>();
            Mapper.Map(part, vm);
            TwitterPostPartSettingVM setting = part.Settings.GetModel<TwitterPostPartSettingVM>();
            if (!string.IsNullOrEmpty(setting.Title))
                vm.ShowTitle = false;
            if (!string.IsNullOrEmpty(setting.Description))
                vm.ShowDescription = false;
            if (!string.IsNullOrEmpty(setting.Image))
                vm.ShowPicture = false;
            _controllerContextAccessor.Context.Controller.TempData["ShowPicture"] = vm.ShowPicture;
            //Url.ItemDisplayUrl
            List<TwitterAccountPart> listaccount = _TwitterService.GetValidTwitterAccount();
            List<SelectListItem> lSelectList = new List<SelectListItem>();
            foreach (TwitterAccountPart fa in listaccount) {
                lSelectList.Insert(0, new SelectListItem() { Value = fa.Id.ToString(), Text = fa.AccountType + " - " + fa.DisplayAs });
            }
            if (lSelectList.Count > 0) {
                vm.SelectedList = part.AccountList.Select(x => x.ToString()).ToArray();
                vm.TwitterAccountList = new SelectList((IEnumerable<SelectListItem>)lSelectList, "Value", "Text", vm.SelectedList);
            }

            return ContentShape("Parts_TwitterPost",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/TwitterPost",
                                    Model: vm,
                                    Prefix: Prefix));
        }

        protected override DriverResult Editor(TwitterPostPart part, IUpdateModel updater, dynamic shapeHelper) {
            TwitterPostVM vm = new TwitterPostVM();
            updater.TryUpdateModel(vm, Prefix, null, null);
            Mapper.CreateMap<TwitterPostVM, TwitterPostPart>();
            Mapper.Map(vm, part);
            if (vm.SelectedList != null)
                part.AccountList = vm.SelectedList.Select(x => Int32.Parse(x)).ToArray();
            else
                part.AccountList = new Int32[] { };
            TwitterPostPartSettingVM setting = part.Settings.GetModel<TwitterPostPartSettingVM>();
            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (string.IsNullOrEmpty(setting.Image)) {
                MediaLibraryPickerField mediaPicker = (MediaLibraryPickerField)part.Fields.Where(f => f.Name == "TwitterImage").FirstOrDefault();
                if (mediaPicker != null && mediaPicker.Ids.Count() > 0) {
                    try {
                        var ContentImage = _orchardServices.ContentManager.Get(mediaPicker.Ids[0], VersionOptions.Published);
                        var pathdocument = Path.Combine(ContentImage.As<MediaPart>().FolderPath, ContentImage.As<MediaPart>().FileName);
                        part.TwitterPicture = pathdocument;//  urlHelper.MakeAbsolute(ContentImage.As<MediaPart>().MediaUrl);
                    }
                    catch {
                        part.TwitterPicture = "";
                    }
                }
                else
                    part.TwitterPicture = "";
            }
            else {
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };

                part.TwitterPicture = urlHelper.MakeAbsolute(_tokenizer.Replace(setting.Image, tokens));
            }
            return Editor(part, shapeHelper);
        }
    }
}