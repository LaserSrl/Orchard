using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard;
using Laser.Orchard.UserReactions.Services;
using Laser.Orchard.UserReactions.ViewModels;
using Orchard.Data;
using Laser.Orchard.UserReactions.Models;


namespace Laser.Orchard.UserReactions.Controllers {
    public class AdminController : Controller {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IOrchardServices _orchardServices;
        private readonly IUserReactionsService _reactionsService;
        private readonly IRepository<Models.UserReactionsTypesRecord> _repoTypes;
        private readonly INotifier _notifier;

        public Localizer T { get; set; }
        // GET: /Admin/
        public AdminController(
            IAuthenticationService authenticationService,
            IMembershipService membershipService, IOrchardServices orcharcServices,
             IUserReactionsService reactionsService,
            IRepository<UserReactions.Models.UserReactionsTypesRecord> repoTypes,
            INotifier notifier
            ) {
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _orchardServices = orcharcServices;
            _reactionsService = reactionsService;
            T = NullLocalizer.Instance;
            _repoTypes = repoTypes;
            _notifier = notifier;
        }

        [HttpGet]
        public ActionResult Settings() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Yout have to be an Administrator to edit Culture Picker settings!")))
                return new HttpUnauthorizedResult();

            var model = _reactionsService.GetTypesTableWithStyles();
            return View(model);
        }


        [HttpPost]
        public ActionResult Settings(UserReactionsTypes model) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Yout have to be an Administrator to edit Culture Picker settings!")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid) {
                _orchardServices.Notifier.Error(T("Settings update failed: {0}", T("check your input!")));
                return View(model);
            }

            var reactionSettings = _orchardServices.WorkContext.CurrentSite.As<UserReactionsSettingsPart>();

            reactionSettings.StyleFileNameProvider = model.CssName;

            foreach (var item in model.UserReactionsType) {
                
                if (item.Delete && item.Id > 0) 
                {
                    _repoTypes.Delete(_repoTypes.Get(item.Id));
                } 
                else 
                {
                    if (item.Id > 0) 
                    {
                        var record = _repoTypes.Get(item.Id);
                        record.Priority = item.Priority;
                        record.TypeCssClass = item.TypeCssClass;
                        record.TypeName = item.TypeName;
                        record.Activating = item.Activating;
                        _repoTypes.Update(record);
                    } 
                    else 
                    {
                            _repoTypes.Create(new Models.UserReactionsTypesRecord {
                            Priority = item.Priority,
                            TypeCssClass = item.TypeCssClass,
                            TypeName = item.TypeName,
                            Activating=item.Activating
                            
                        });


                        _repoTypes.Flush();
                    }

                }

            }

            _notifier.Add(NotifyType.Information, T("UserReaction settings updating"));
            return RedirectToActionPermanent("Settings");
        }
    }
}
