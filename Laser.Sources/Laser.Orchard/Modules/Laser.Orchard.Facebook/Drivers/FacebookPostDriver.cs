using AutoMapper;
using Laser.Orchard.Facebook.Models;
using Laser.Orchard.Facebook.Services;
using Laser.Orchard.Facebook.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.Facebook.Drivers {

    public class FacebookPostDriver : ContentPartDriver<FacebookPostPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IFacebookService _facebookService;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.Facebook"; }
        }

        public FacebookPostDriver(IOrchardServices orchardServices, IFacebookService facebookService) {
            _orchardServices = orchardServices;
            _facebookService = facebookService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(FacebookPostPart part, dynamic shapeHelper) {
            FacebookPostVM vm = new FacebookPostVM();
            Mapper.CreateMap<FacebookPostPart, FacebookPostVM>();
            Mapper.Map(part, vm);
            List<FacebookAccountPart> listaccount = _facebookService.GetValidFacebookAccount();
            List<SelectListItem> lSelectList = new List<SelectListItem>();
            foreach (FacebookAccountPart fa in listaccount) {
                lSelectList.Insert(0, new SelectListItem() { Value = fa.Id.ToString(), Text = fa.DisplayAs });
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