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
using Orchard.Mvc.Html;
using System.Globalization;
using Orchard.MediaProcessing.Services;
using Orchard.MediaProcessing.Models;



namespace Laser.Orchard.Twitter.Drivers {

    public class TwitterPostDriver : ContentPartDriver<TwitterPostPart> {
        private readonly IImageProfileManager _imageProfileManager;
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

        public TwitterPostDriver(IImageProfileManager imageProfileManager, IStorageProvider storageProvider, IOrchardServices orchardServices, ITwitterService TwitterService, IProviderConfigurationService providerConfigurationService, ITokenizer tokenizer, IHttpContextAccessor httpContextAccessor, IControllerContextAccessor controllerContextAccessor) {
            _storageProvider = storageProvider;
            _imageProfileManager = imageProfileManager;
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
                if (pcr != null)
                    vm.Site = pcr.UserIdentifier;
                TwitterPostPartSettingVM setting = part.Settings.GetModel<TwitterPostPartSettingVM>();
                var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                if (!string.IsNullOrEmpty(setting.Description))
                    vm.Description = _tokenizer.Replace(setting.Description, tokens);
                else
                    vm.Description = part.TwitterDescription;
                if (!string.IsNullOrEmpty(setting.Image)) {
                    string ids = _tokenizer.Replace(setting.Image, tokens);

                    int idimage;
                    Int32.TryParse(ids.Replace("{", "").Replace("}", "").Split(',')[0], out idimage); ;
                    // _orchardServices.ContentManager.Get(id);
                    // vm.Image = Url.ItemDisplayUrl(_orchardServices.ContentManager.Get(id));
                    var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                    //       vm.Image = urlHelper.ItemDisplayUrl(_orchardServices.ContentManager.Get(id));// get current display link
                    //   Fvm.Link = urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(Twitterpart));// get current display link
                    var ContentImage = _orchardServices.ContentManager.Get(idimage, VersionOptions.Published);
                    //   var pathdocument = Path.Combine(ContentImage.As<MediaPart>().FolderPath, ContentImage.As<MediaPart>().FileName);
                    //  part.TwitterPicture = pathdocument;// 
                    vm.Image = urlHelper.MakeAbsolute(ContentImage.As<MediaPart>().MediaUrl);
                    //   .ResizeMediaUrl(Width: previewWidth, Height: previewHeight, Mode: "crop", Alignment: "middlecenter", Path: Model.MediaData.MediaUrl)');
                }
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
        //private string DoTheResize(int Width, int Height, string path)
        //{
        //    var Mode = "pad";
        //    var Alignment = "middlecenter";
        //    var PadColor = "000000";

        //    var state = new Dictionary<string, string> {
        //        {"Width", Width.ToString(CultureInfo.InvariantCulture)},
        //        {"Height", Height.ToString(CultureInfo.InvariantCulture)},
        //        {"Mode", Mode},
        //        {"Alignment", Alignment},
        //        {"PadColor", PadColor},
        //    };

        //    var filter = new FilterRecord
        //    {
        //        Category = "Transform",
        //        Type = "Resize",
        //        State = FormParametersHelper.ToString(state)
        //    };

        //    var profile = "Transform_Resize"
        //        + "_w_" + Convert.ToString(Width)
        //        + "_h_" + Convert.ToString(Height)
        //        + "_m_" + Convert.ToString(Mode)
        //        + "_a_" + Convert.ToString(Alignment)
        //        + "_c_" + Convert.ToString(PadColor);

        //    var resizedImagePath = _imageProfileManager.GetImageProfileUrl(path, profile, filter);
        //    return resizedImagePath;
        //}
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
            //_controllerContextAccessor.Context.Controller.TempData["ShowPicture"] = vm.ShowPicture;
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
                //MediaLibraryPickerField mediaPicker = (MediaLibraryPickerField)part.Fields.Where(f => f.Name == "TwitterImage").FirstOrDefault();
                //if (mediaPicker != null && mediaPicker.Ids.Count() > 0) {
                //    try {
                //        var ContentImage = _orchardServices.ContentManager.Get(mediaPicker.Ids[0], VersionOptions.Published);
                //        var pathdocument = Path.Combine(ContentImage.As<MediaPart>().FolderPath, ContentImage.As<MediaPart>().FileName);
                //        part.TwitterPicture = pathdocument;//  urlHelper.MakeAbsolute(ContentImage.As<MediaPart>().MediaUrl);

                //    }
                //    catch {
                //        part.TwitterPicture = "";
                //    }
                //}
                //else
                    part.TwitterPicture = "";
            }
            else {
                    var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
                    string listid = _tokenizer.Replace(setting.Image, tokens);
                    listid = listid.Replace("{", "").Replace("}", "");
                    Int32 idimage = 0;
                    Int32.TryParse(listid.Split(',')[0], out idimage);
                    if (idimage > 0) {
                        var ContentImage = _orchardServices.ContentManager.Get(idimage, VersionOptions.Published);
                        var pathdocument = Path.Combine(ContentImage.As<MediaPart>().FolderPath, ContentImage.As<MediaPart>().FileName);
                        part.TwitterPicture = pathdocument;
                    }
                    else
                        part.TwitterPicture = "";
                
                // _orchardServices.ContentManager.Get(
              //  part.TwitterPicture = urlHelper.MakeAbsolute(_tokenizer.Replace(setting.Image, tokens));
            }
            return Editor(part, shapeHelper);
        }
    }
}