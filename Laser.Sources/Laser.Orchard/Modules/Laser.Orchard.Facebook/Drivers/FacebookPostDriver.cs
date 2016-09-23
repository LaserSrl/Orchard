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

    public class FacebookPostDriver : ContentPartDriver<FacebookPostPart> {
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
            Mapper.CreateMap<FacebookPostPart, FacebookPostVM>();
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
            Mapper.CreateMap<FacebookPostVM, FacebookPostPart>();
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


        protected override void Importing(FacebookPostPart part, ImportContentContext context) 
        {
            var root = context.Data.Element(part.PartDefinition.Name);

            var importedFacebookMessage = context.Attribute(part.PartDefinition.Name, "FacebookMessage");
            if (importedFacebookMessage != null) {
                part.FacebookMessage = importedFacebookMessage;
            }

            var importedFacebookMessageSent = context.Attribute(part.PartDefinition.Name, "FacebookMessageSent");
            if (importedFacebookMessageSent != null) {
                part.FacebookMessageSent = bool.Parse(importedFacebookMessageSent);
            }

            var importedFacebookCaption = context.Attribute(part.PartDefinition.Name, "FacebookCaption");
            if (importedFacebookCaption != null) {
                part.FacebookCaption = importedFacebookCaption;
            }

            var importedFacebookDescription = context.Attribute(part.PartDefinition.Name, "FacebookDescription");
            if (importedFacebookDescription != null) {
                part.FacebookDescription = importedFacebookDescription;
            }

            var importedFacebookName = context.Attribute(part.PartDefinition.Name, "FacebookName");
            if (importedFacebookName != null) {
                part.FacebookName = importedFacebookName;
            }

            var importedFacebookPicture = context.Attribute(part.PartDefinition.Name, "FacebookPicture");
            if (importedFacebookPicture != null) {
                part.FacebookPicture = importedFacebookPicture;
            }

            var importedFacebookIdPicture = context.Attribute(part.PartDefinition.Name, "FacebookIdPicture");
            if (importedFacebookIdPicture != null) {
                part.FacebookIdPicture = importedFacebookIdPicture;
            }

            var importedFacebookLink = context.Attribute(part.PartDefinition.Name, "FacebookLink");
            if (importedFacebookLink != null) {
                part.FacebookLink = importedFacebookLink;
            }

            var importedSendOnNextPublish = context.Attribute(part.PartDefinition.Name, "SendOnNextPublish");
            if (importedSendOnNextPublish != null) {
                part.SendOnNextPublish = bool.Parse(importedSendOnNextPublish);
            }

            var importedAccountList = context.Attribute(part.PartDefinition.Name, "AccountList");
           
            if (importedAccountList != null) 
            {
                for (int x = 0; x <= importedAccountList.Count(); x++) {
                    part.AccountList[x] = importedAccountList[x];
                }
            }

            var importedFacebookMessageToPost = context.Attribute(part.PartDefinition.Name, "FacebookMessageToPost");
            if (importedFacebookMessageToPost != null) {
                part.FacebookMessageToPost = importedFacebookMessageToPost;
            }

            var importedHasImage = context.Attribute(part.PartDefinition.Name, "HasImage");
            if (importedHasImage != null) {
                part.HasImage = bool.Parse(importedHasImage);
            }

        }



        protected override void Exporting(FacebookPostPart part, ExportContentContext context) {
            
            context.Element(part.PartDefinition.Name).SetAttributeValue("FacebookMessage", part.FacebookMessage);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FacebookMessageSent", part.FacebookMessageSent);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FacebookCaption", part.FacebookCaption);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FacebookDescription", part.FacebookDescription);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FacebookName", part.FacebookName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FacebookPicture", part.FacebookPicture);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FacebookLink", part.FacebookLink);
            context.Element(part.PartDefinition.Name).SetAttributeValue("SendOnNextPublish", part.SendOnNextPublish);

            if (part.AccountList.Count() > 0) 
            {
                //context.Element(part.PartDefinition.Name).SetAttributeValue("AccountList", part.AccountList);
                //var accountList = context.Element(part.PartDefinition.Name).Element("accountList");
                //for(int x=0; x==part.AccountList.Count(); x++) 
                //{
                //    accountList.Element("accountList").SetAttributeValue("accountList", part.AccountList[x]);
                //}
            }

            context.Element(part.PartDefinition.Name).SetAttributeValue("FacebookMessageToPost", part.FacebookMessageToPost);
            context.Element(part.PartDefinition.Name).SetAttributeValue("HasImage", part.HasImage);

        }   





    }
}