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
            if (!string.IsNullOrEmpty(setting.FacebookCaption))
                vm.ShowFacebookCaption =false;        
            if (!string.IsNullOrEmpty(setting.FacebookDescription))
                vm.ShowFacebookDescription =false;      
            if (!string.IsNullOrEmpty(setting.FacebookLink))
                vm.ShowFacebookLink =false;
            if (!string.IsNullOrEmpty(setting.FacebookMessage))
                vm.ShowFacebookMessage = false;
            if (!string.IsNullOrEmpty(setting.FacebookName))
                vm.ShowFacebookName = false;
            if (!string.IsNullOrEmpty(setting.FacebookPicture))
                vm.ShowFacebookPicture = false;
            //if (!string.IsNullOrEmpty(setting.FacebookCaption))
            //    vm.FacebookCaption = _tokenizer.Replace(setting.FacebookCaption, tokens);
            //else
            //    vm.FacebookCaption = part.FacebookCaption;
            //if (!string.IsNullOrEmpty(setting.FacebookDescription))
            //    vm.FacebookDescription = _tokenizer.Replace(setting.FacebookDescription, tokens);
            //else
            //    vm.FacebookDescription = part.FacebookDescription;
            //if (!string.IsNullOrEmpty(setting.FacebookLink))
            //    vm.FacebookLink = _tokenizer.Replace(setting.FacebookLink, tokens);
            //else
            //    vm.FacebookLink = part.FacebookLink;
            //if (!string.IsNullOrEmpty(setting.FacebookMessage))
            //    vm.FacebookMessage = _tokenizer.Replace(setting.FacebookMessage, tokens);
            //else
            //    vm.FacebookMessage = part.FacebookMessage;
            //if (!string.IsNullOrEmpty(setting.FacebookName))
            //    vm.FacebookName = _tokenizer.Replace(setting.FacebookName, tokens);
            //else
            //    vm.FacebookName = part.FacebookName;
            //if (!string.IsNullOrEmpty(setting.FacebookPicture))
            //    vm.FacebookPicture = _tokenizer.Replace(setting.FacebookPicture, tokens);
            //else
            //    vm.FacebookPicture = part.FacebookPicture;
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