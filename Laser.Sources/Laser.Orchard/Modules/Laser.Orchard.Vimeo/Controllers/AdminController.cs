using Laser.Orchard.Vimeo.Models;
using Orchard;
using Orchard.UI.Admin;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using System.IO;
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
        private readonly IVimeoAdminServices _vimeoAdminServices;

        public Localizer T { get; set; }

        public AdminController(IOrchardServices orchardServices, IVimeoAdminServices vimeoAdminServices) {
            _orchardServices = orchardServices;
            _vimeoAdminServices = vimeoAdminServices;
            T = NullLocalizer.Instance;
        }

        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage Vimeo settings")))
                return new HttpUnauthorizedResult();

            if (VimeoSettingsPartViewModel.CCLicenseDictionary == null || VimeoSettingsPartViewModel.CCLicenseDictionary.Count == 0) {
                //load license options from file
                var assembly = Assembly.GetExecutingAssembly();
                var ccResourceName = "Laser.Orchard.Vimeo.CreativeCommonsOptions.txt";
                VimeoSettingsPartViewModel.CCLicenseDictionary = new Dictionary<string, string>(); //value, text
                using (Stream st = assembly.GetManifestResourceStream(ccResourceName)) {
                    using (StreamReader reader = new StreamReader(st)) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            string[] parts = line.Split(new[] { "," }, StringSplitOptions.None); //value, text
                            VimeoSettingsPartViewModel.CCLicenseDictionary.Add(parts[0], parts[1]);
                        }
                    }
                }
            }
            if (VimeoSettingsPartViewModel.LocaleDictionary == null || VimeoSettingsPartViewModel.LocaleDictionary.Count == 0) {
                //load languages from file
                var assembly = Assembly.GetExecutingAssembly();
                var locResourceName = "Laser.Orchard.Vimeo.LanguageCodes.txt";
                VimeoSettingsPartViewModel.LocaleDictionary = new Dictionary<string, string>(); //value, text
                using (Stream st = assembly.GetManifestResourceStream(locResourceName)) {
                    using (StreamReader reader = new StreamReader(st)) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            string[] parts = line.Split(new[] { "," }, StringSplitOptions.None); //value, text
                            VimeoSettingsPartViewModel.LocaleDictionary.Add(parts[0], parts[1]);
                        }
                    }
                }
            }
            if (VimeoSettingsPartViewModel.ContentRatingDictionary == null || VimeoSettingsPartViewModel.ContentRatingDictionary.Count == 0) {
                //load ratings from file
                //NOTE: "safe" and "unrated" settings are not in the file. The file only contains the "unsafe" rating options
                var assembly = Assembly.GetExecutingAssembly();
                var crResourceName = "Laser.Orchard.Vimeo.ContentRating.txt";
                VimeoSettingsPartViewModel.ContentRatingDictionary = new Dictionary<string, string>(); //value, text
                using (Stream st = assembly.GetManifestResourceStream(crResourceName)) {
                    using (StreamReader reader = new StreamReader(st)) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            string[] parts = line.Split(new[] { "," }, StringSplitOptions.None); //value, text
                            VimeoSettingsPartViewModel.ContentRatingDictionary.Add(parts[0], parts[1]);
                        }
                    }
                }
            }

            //var settings = _orchardServices
            //    .WorkContext
            //    .CurrentSite
            //    .As<VimeoSettingsPart>();

            var vm = _vimeoAdminServices.GetSettingsVM(); // new VimeoSettingsPartViewModel(settings);

            return View(vm);
        }

        [HttpPost, ActionName("Index")]
        [OMvc.FormValueRequired("submit.TestSettings")]
        public ActionResult IndexTestSettings(VimeoSettingsPartViewModel vm) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage Vimeo settings")))
                return new HttpUnauthorizedResult();
            try {
                if (!string.IsNullOrWhiteSpace(vm.AccessToken)) {
                    if (_vimeoAdminServices.TokenIsValid(vm)) {
                        _orchardServices.Notifier.Information(T("Access Token Valid"));
                        //now test group, channel and album
                        if (!string.IsNullOrWhiteSpace(vm.GroupName)) {
                            if (_vimeoAdminServices.GroupIsValid(vm))
                                _orchardServices.Notifier.Information(T("Group Name Valid"));
                            else
                                _orchardServices.Notifier.Error(T("Group Name not valid"));
                        }
                        if (!string.IsNullOrWhiteSpace(vm.ChannelName)) {
                            if (_vimeoAdminServices.ChannelIsValid(vm))
                                _orchardServices.Notifier.Information(T("Channel Name Valid"));
                            else
                                _orchardServices.Notifier.Error(T("Channel Name not valid"));
                        }
                        if (!string.IsNullOrWhiteSpace(vm.AlbumName)) {
                            if (_vimeoAdminServices.AlbumIsValid(vm))
                                _orchardServices.Notifier.Information(T("Album Name Valid"));
                            else
                                _orchardServices.Notifier.Error(T("Album Name not valid"));
                        }
                    } else
                        _orchardServices.Notifier.Error(T("Access Token not valid"));
                }
                //update the access tokens by removing those flagged for deletion

                //test the remaining tokens
                string atsValid = _vimeoAdminServices.TokensAreValid(vm);
                if (atsValid == "OK") {
                    _orchardServices.Notifier.Information(T("Access Tokens are valid"));
                    //now test group, channel and album
                    if (!string.IsNullOrWhiteSpace(vm.GroupName)) {
                        if (_vimeoAdminServices.GroupIsValid(vm))
                            _orchardServices.Notifier.Information(T("Group Name Valid"));
                        else
                            _orchardServices.Notifier.Error(T("Group Name not valid"));
                    }
                    if (!string.IsNullOrWhiteSpace(vm.ChannelName)) {
                        if (_vimeoAdminServices.ChannelIsValid(vm))
                            _orchardServices.Notifier.Information(T("Channel Name Valid"));
                        else
                            _orchardServices.Notifier.Error(T("Channel Name not valid"));
                    }
                    if (!string.IsNullOrWhiteSpace(vm.AlbumName)) {
                        if (_vimeoAdminServices.AlbumIsValid(vm))
                            _orchardServices.Notifier.Information(T("Album Name Valid"));
                        else
                            _orchardServices.Notifier.Error(T("Album Name not valid"));
                    }
                } else {
                    _orchardServices.Notifier.Error(T("Access Tokens are not valid: {0}{1}", Environment.NewLine, atsValid));
                }
            } catch (VimeoRateException vre) {
                _orchardServices.Notifier.Error(T("Too many requests to Vimeo. Rate limits will reset on {0} UTC", vre.resetTime.Value.ToString()));
            } catch (Exception ex) {
                _orchardServices.Notifier.Error(T("{0}", ex.Message));
            }



            return View(vm);
        }

        [HttpPost, ActionName("Index")]
        [OMvc.FormValueRequired("submit.SaveSettings")]
        public ActionResult IndexSaveSettings(VimeoSettingsPartViewModel vm) {
            try {
                _vimeoAdminServices.UpdateSettings(vm);
            } catch (VimeoRateException vre) {
                _orchardServices.Notifier.Error(T("Too many requests to Vimeo. Rate limits will reset on {0} UTC", vre.resetTime.Value.ToString()));
            } catch (Exception ex) {
                _orchardServices.Notifier.Error(T("{0}", ex.Message));
            }
            return RedirectToAction("Index");
        }
    }
}