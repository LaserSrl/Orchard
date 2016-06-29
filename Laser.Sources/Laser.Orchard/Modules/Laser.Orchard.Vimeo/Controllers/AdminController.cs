using Laser.Orchard.Vimeo.Models;
using Orchard;
using Orchard.UI.Admin;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.Vimeo.ViewModels;
using Orchard.Security;
using Orchard.Localization;
using Orchard;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using OMvc = Orchard.Mvc;
using Orchard.UI.Notify;
using Laser.Orchard.Vimeo.Services;

namespace Laser.Orchard.Vimeo.Controllers {
    [Admin]
    public class AdminController : Controller {

        private readonly IOrchardServices _orchardServices;
        private readonly IVimeoServices _vimeoServices;

        public Localizer T { get; set; }

        public AdminController(IOrchardServices orchardServices, IVimeoServices vimeoServices) {
            _orchardServices = orchardServices;
            _vimeoServices = vimeoServices;
            T = NullLocalizer.Instance;
        }

        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage Vimeo settings")))
                return new HttpUnauthorizedResult();

            var settings = _orchardServices
                .WorkContext
                .CurrentSite
                .As<VimeoSettingsPart>();

            var vm = new VimeoSettingsPartViewModel(settings);

            return View(vm);
        }

        [HttpPost, ActionName("Index")]
        [OMvc.FormValueRequired("submit.TestSettings")]
        public ActionResult IndexTestSettings(VimeoSettingsPartViewModel vm) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage Vimeo settings")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrWhiteSpace(vm.AccessToken)) {
                if (_vimeoServices.TokenIsValid(vm)) {
                    _orchardServices.Notifier.Information(T("Access Token Valid"));
                    //now test group, channel and album
                    if (!string.IsNullOrWhiteSpace(vm.GroupName)) {
                        if(_vimeoServices.GroupIsValid(vm))
                            _orchardServices.Notifier.Information(T("Group Name Valid"));
                        else
                            _orchardServices.Notifier.Error(T("Group Name not valid"));
                    }
                    if (!string.IsNullOrWhiteSpace(vm.ChannelName)) {

                    }
                    if (!string.IsNullOrWhiteSpace(vm.AlbumName)) {

                    }
                } else
                    _orchardServices.Notifier.Error(T("Access Token not valid"));
            }


            return View(vm);
        }

        [HttpPost, ActionName("Index")]
        [OMvc.FormValueRequired("submit.SaveSettings")]
        public ActionResult IndexSaveSettings(VimeoSettingsPartViewModel vm) {
            _vimeoServices.UpdateSettings(vm);
            return RedirectToAction("Index");
        }
    }
}