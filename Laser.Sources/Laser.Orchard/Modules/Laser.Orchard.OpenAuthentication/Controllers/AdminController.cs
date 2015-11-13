using System.Web.Mvc;
using Laser.Orchard.OpenAuthentication.Models;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.OpenAuthentication.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System;
using System.Linq;
using Laser.Orchard.OpenAuthentication.Services.Clients;
using System.Reflection;
using System.Collections.Generic;

namespace Laser.Orchard.OpenAuthentication.Controllers {
    [Admin]
    public class AdminController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IProviderConfigurationService _providerConfigurationService;
        private readonly IExternalAuthenticationClient _externalAuthenticationClient;

        public AdminController(IOrchardServices orchardServices,
            IProviderConfigurationService providerConfigurationService,
             IExternalAuthenticationClient externalAuthenticationClient) {
            _orchardServices = orchardServices;
            _providerConfigurationService = providerConfigurationService;
            _externalAuthenticationClient = externalAuthenticationClient;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage open authentication settings")))
                return new HttpUnauthorizedResult();

            var settings = _orchardServices.WorkContext.CurrentSite.As<OpenAuthenticationSettingsPart>();

            var currentProviders = _providerConfigurationService.GetAll();

            var viewModel = new IndexViewModel {
                AutoRegistrationEnabled = settings.AutoRegistrationEnabled,
                CurrentProviders = currentProviders
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost(IndexViewModel viewModel) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage open authentication settings")))
                return new HttpUnauthorizedResult();

            var settings = _orchardServices.WorkContext.CurrentSite.As<OpenAuthenticationSettingsPart>();
            settings.AutoRegistrationEnabled = viewModel.AutoRegistrationEnabled;

            return RedirectToAction("Index");
        }

        public ActionResult Remove(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage open authentication settings")))
                return new HttpUnauthorizedResult();

            _providerConfigurationService.Delete(id);

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage open authentication settings")))
                return new HttpUnauthorizedResult();
            CreateProviderViewModel pv = _providerConfigurationService.Get(id);
            pv.ProviderNameList = getAllProviderName(pv.ProviderName);
            return View((Object)pv);
        }


        private SelectList getAllProviderName(string selectedvalue="") {
            var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.GetInterfaces().Contains(typeof(IExternalAuthenticationClient))
                                     && t.GetConstructor(Type.EmptyTypes) != null
                            select Activator.CreateInstance(t) as IExternalAuthenticationClient;


            List<SelectListItem> lSelectList = new List<SelectListItem>();
            foreach (var instance in instances.OrderByDescending(p=>p.ProviderName)) {
                lSelectList.Insert(0, new SelectListItem() { Value = instance.ProviderName, Text = instance.ProviderName });
            }
            return new SelectList((IEnumerable<SelectListItem>)lSelectList, "Value", "Text", selectedvalue);

        }

        [HttpPost, ActionName("Edit")]
        public ActionResult Edit(CreateProviderViewModel viewModel) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage open authentication settings")))
                return new HttpUnauthorizedResult();

            if (!_providerConfigurationService.VerifyUnicity(viewModel.ProviderName, viewModel.Id)) {

                ModelState.AddModelError("ProviderName", T("Provider name already exists").ToString());

                viewModel.ProviderNameList = getAllProviderName(viewModel.ProviderName);
                return View((Object)viewModel);
            }
            else {
                _providerConfigurationService.Edit(viewModel);


                return RedirectToAction("Index");
            }
        }

        public ActionResult CreateProvider() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage open authentication settings")))
                return new HttpUnauthorizedResult();
            CreateProviderViewModel pv = new CreateProviderViewModel();
            pv.ProviderNameList = getAllProviderName();
            return View(pv);
        }

        [HttpPost, ActionName("CreateProvider")]
        public ActionResult CreateProviderPost(CreateProviderViewModel viewModel) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage open authentication settings")))
                return new HttpUnauthorizedResult();

            if (!_providerConfigurationService.VerifyUnicity(viewModel.ProviderName)) {
                ModelState.AddModelError("ProviderName", T("Provider name already exists").ToString());
            }

            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();
                viewModel.ProviderNameList = getAllProviderName();
                return View(viewModel);
            }

            _providerConfigurationService.Create(new ProviderConfigurationCreateParams {
                DisplayName = viewModel.DisplayName,
                ProviderName = viewModel.ProviderName,
                ProviderIdentifier = viewModel.ProviderIdentifier,
                ProviderIdKey = viewModel.ProviderIdKey,
                ProviderSecret = viewModel.ProviderSecret,
            });
            _orchardServices.Notifier.Information(T("Your configuration has been saved."));

            return RedirectToAction("Index");
        }
    }
}