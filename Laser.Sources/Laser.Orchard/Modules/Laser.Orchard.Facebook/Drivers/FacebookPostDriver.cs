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
using Orchard.Environment.Configuration;
using Orchard.UI.Admin;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.Facebook.Drivers {

    public class FacebookPostDriver : ContentPartCloningDriver<FacebookPostPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IFacebookService _facebookService;
        private readonly ITokenizer _tokenizer;
        private readonly ShellSettings _shellSettings;
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.Facebook"; }
        }

        public FacebookPostDriver(ShellSettings shellSettings, IOrchardServices orchardServices, IFacebookService facebookService, ITokenizer tokenizer) {
            _orchardServices = orchardServices;
            _facebookService = facebookService;
            _shellSettings = shellSettings;
            _tokenizer = tokenizer;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(FacebookPostPart part, string displayType, dynamic shapeHelper) {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin) {
                if (displayType == "Summary") {
                    return ContentShape("Parts_FacebookPost",
                        () => shapeHelper.Parts_FacebookPost(SendOnNextPublish: part.SendOnNextPublish, Sent: part.FacebookMessageSent));
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }
        }

        protected override DriverResult Editor(FacebookPostPart part, dynamic shapeHelper) {


            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            FacebookPostVM vm = new FacebookPostVM();
            Mapper.Initialize(cfg => {
                cfg.CreateMap<FacebookPostPart, FacebookPostVM>();
            });
            Mapper.Map(part, vm);
            if (string.IsNullOrEmpty(vm.FacebookType.ToString()))
                vm.FacebookType = FacebookType.Post;
            FacebookPostPartSettingVM setting = part.Settings.GetModel<FacebookPostPartSettingVM>();
            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
            if (!string.IsNullOrEmpty(setting.FacebookCaption)) {
                vm.ShowFacebookCaption = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookDescription)) {
                vm.ShowFacebookDescription = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookLink)) {
                vm.ShowFacebookLink = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookMessage)) {
                vm.ShowFacebookMessage = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookName)) {
                vm.ShowFacebookName = false;
            }
            if (!string.IsNullOrEmpty(setting.FacebookPicture)) {
                vm.ShowFacebookPicture = false;
            }
            else
                vm.FacebookPicture = part.FacebookPicture;
            List<FacebookAccountPart> listaccount = _facebookService.GetValidFacebookAccount();
            //   List<SelectListItem> lSelectList = new List<SelectListItem>();
            List<OptionList> optionList = new List<OptionList>();
            foreach (FacebookAccountPart fa in listaccount) {
                //  lSelectList.Insert(0, new SelectListItem() { Value = fa.Id.ToString(), Text = fa.AccountType + " - " + fa.DisplayAs });
                OptionList ol = new OptionList {
                    Value = fa.Id.ToString(),
                    Text = fa.AccountType + " - " + fa.DisplayAs,
                    ImageUrl = urlHelper.Content("~/Media/" + _shellSettings.Name + "/facebook_" + fa.UserIdFacebook + ".jpg"),
                    Selected = part.AccountList.Contains(fa.Id) ? "selected=\"selected\"" : ""
                };
                optionList.Add(ol);
            }
            vm.ListOption = optionList;

            return ContentShape("Parts_FacebookPost",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/FacebookPost",
                                    Model: vm,
                                    Prefix: Prefix));
        }



        protected override DriverResult Editor(FacebookPostPart part, IUpdateModel updater, dynamic shapeHelper) {
            FacebookPostPartSettingVM setting = part.Settings.GetModel<FacebookPostPartSettingVM>();
            var tokens = new Dictionary<string, object> { { "Content", part.ContentItem } };
            FacebookPostVM vm = new FacebookPostVM();
            updater.TryUpdateModel(vm, Prefix, null, null);
            Mapper.Initialize(cfg => {
                cfg.CreateMap<FacebookPostVM, FacebookPostPart>();
            });
            Mapper.Map(vm, part);
            if (_orchardServices.WorkContext.HttpContext.Request.Form["FacebookType"] != null && _orchardServices.WorkContext.HttpContext.Request.Form["FacebookType"] == "1")
                part.FacebookType = FacebookType.Post;
            else
                part.FacebookType = FacebookType.ShareLink;

            if (vm.SelectedList != null && vm.SelectedList.Count() > 0) {
                part.AccountList = vm.SelectedList.Select(x => Int32.Parse(x)).ToArray();
            }
            return Editor(part, shapeHelper);
        }

        protected override void Cloning(FacebookPostPart originalPart, FacebookPostPart clonePart, CloneContentContext context) {
            clonePart.FacebookMessage = originalPart.FacebookMessage;
            //do not clone FacebookMessageSent so that we can send it in the cloned post
            clonePart.FacebookMessageSent = false;
            clonePart.FacebookCaption = originalPart.FacebookCaption;
            clonePart.FacebookDescription = originalPart.FacebookDescription;
            clonePart.FacebookName = originalPart.FacebookName;
            clonePart.FacebookPicture = originalPart.FacebookPicture;
            clonePart.FacebookIdPicture = originalPart.FacebookIdPicture;
            clonePart.FacebookLink = originalPart.FacebookLink;
            clonePart.SendOnNextPublish = false;
            clonePart.AccountList = originalPart.AccountList;
            clonePart.FacebookType = originalPart.FacebookType;
            clonePart.FacebookMessageToPost = originalPart.FacebookMessageToPost;
            clonePart.HasImage = originalPart.HasImage;
        }
    }
}