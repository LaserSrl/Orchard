using Laser.Orchard.MultiStepAuthentication.Permissions;
using Laser.Orchard.MultiStepAuthentication.ViewModels;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.MultiStepAuthentication.Controllers {
    [OrchardFeature("Laser.Orchard.NonceLogin")]
    [ValidateInput(false), Admin]
    public class NonceLoginAdminController : Controller, IUpdateModel {

        private readonly IAuthorizer _authorizer;
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        private readonly INotifier _notifier;
        
        private const string groupInfoId = "Laser.Orchard.NonceLogin.Settings";

        public NonceLoginAdminController(
            IAuthorizer authorizer,
            ISiteService siteService,
            IContentManager contentManager,
            ITransactionManager transactionManager,
            INotifier notifier) {

            _authorizer = authorizer;
            _siteService = siteService;
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!_authorizer.Authorize(MultiStepAuthenticationPermissions.ConfigureAuthentication, 
                    null, T("Not authorized to manage settings for multi-step authentication")))
                return new HttpUnauthorizedResult();

            var site = _siteService.GetSiteSettings();
            dynamic model = _contentManager.BuildEditor(site, groupInfoId);
            
            if (model == null)
                return HttpNotFound();

            return View(new NonceLoginAdminViewModel() {
                Model = model
            });
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (!_authorizer.Authorize(MultiStepAuthenticationPermissions.ConfigureAuthentication,
                    null, T("Not authorized to manage settings for multi-step authentication")))
                return new HttpUnauthorizedResult();

            var site = _siteService.GetSiteSettings();
            dynamic model = _contentManager.UpdateEditor(site, this, groupInfoId);

            if (model == null) {
                _transactionManager.Cancel();
                return HttpNotFound();
            }

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();

                return View(model);
            }
            _notifier.Information(T("Nonce login settings updated"));

            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}