using System;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using Contrib.Profile.Models;
using Orchard.ContentManagement.MetaData.Models;
using System.Linq;
using Contrib.Profile.Settings;
using System.Collections.Generic;
using Orchard.ContentManagement.Handlers;

namespace Contrib.Profile.Controllers {
    [ValidateInput(false), Themed]
    public class HomeController : Controller, IUpdateModel {

        private readonly IMembershipService _membershipService;
        private readonly IContentManager _contentManager;

        public HomeController(IOrchardServices services,
            IMembershipService membershipService,
            IContentManager contentManager) {

            _membershipService = membershipService;
            _contentManager = contentManager;

            Services = services;
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(string username) {
            IUser user = _membershipService.GetUser(username);

            if (user == null ||
                UserHasNoProfilePart(user) ||
                !Services.Authorizer.Authorize(Permissions.ViewProfiles, user, null)) {
                return HttpNotFound();
            }
            
            dynamic shape = BuildFrontEndShape(
                _contentManager.BuildDisplay(user, "", ""), //since the result of BuildDisplay is dynamic I have to do the ugly thing below
                new Func<ContentTypePartDefinition, string, bool>(MayAllowPartDisplay), 
                new Func<ContentPartFieldDefinition, bool>(MayAllowFieldDisplay));

            return View((object)shape);
        }

        public ActionResult Edit() {
            IUser user = Services.WorkContext.CurrentUser;

            if (user == null ||
                UserHasNoProfilePart(user)) {
                return HttpNotFound();
            }
            
            dynamic shape = BuildFrontEndShape(
                _contentManager.BuildEditor(user),
                new Func<ContentTypePartDefinition, string, bool>(MayAllowPartEdit),
                new Func<ContentPartFieldDefinition, bool>(MayAllowFieldEdit));

            return View((object)shape);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost() {
            IUser user = Services.WorkContext.CurrentUser;

            if (user == null ||
                UserHasNoProfilePart(user)) {
                return HttpNotFound();
            }
            var userId = user.Id;

            dynamic shape = BuildFrontEndShape(
                _contentManager.UpdateEditor(user, this),
                new Func<ContentTypePartDefinition, string, bool>(MayAllowPartEdit),
                new Func<ContentPartFieldDefinition, bool>(MayAllowFieldEdit));
            
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View("Edit", (object)shape);
            }

            Services.Notifier.Information(T("Your profile has been saved."));

            return RedirectToAction("Edit");
        }
        

        private dynamic BuildFrontEndShape(
            dynamic shape,
            Func<ContentTypePartDefinition, string, bool> partTest,
            Func<ContentPartFieldDefinition, bool> fieldTest) {
            
            //shape.Content.Items contains the List<object> of the things we will display
            //we can do a ((List<dynamic>)(shape.Content.Items)).RemoveAll(condition) to get rid 
            //of the stuff we do not want to see.

            //remove parts. This also removes all parts that are dynamically attached and hence
            //cannot have the setting to control their visibility
            ((List<dynamic>)(shape.Content.Items))
                .RemoveAll(it =>
                    it.ContentPart != null &&
                    !partTest(it.ContentPart.TypePartDefinition, it.ContentPart.TypeDefinition.Name)
                );
            //remove fields
            ((List<dynamic>)(shape.Content.Items))
                .RemoveAll(it =>
                    it.ContentPart != null &&
                    it.ContentField != null &&
                    !fieldTest(it.ContentField.PartFieldDefinition)
                ); 

            return shape;
        }

        private bool UserHasNoProfilePart(IUser user) {
            return user.As<ProfilePart>() == null && user.ContentItem.As<ProfilePart>() == null;
        }

        private bool MayAllowPartDisplay(ContentTypePartDefinition definition, string typeName) {
            return definition.PartDefinition.Name == typeName || //this is to account for fields added to the type
                definition.Settings.GetModel<ProfileFrontEndSettings>().AllowFrontEndDisplay;
        }

        private bool MayAllowPartEdit(ContentTypePartDefinition definition, string typeName) {
            return definition.PartDefinition.Name == typeName || //this is to account for fields added to the type
                definition.Settings.GetModel<ProfileFrontEndSettings>().AllowFrontEndEdit;
        }

        private bool MayAllowFieldDisplay(ContentPartFieldDefinition definition) {
            return definition.Settings.GetModel<ProfileFrontEndSettings>().AllowFrontEndDisplay;
        }

        private bool MayAllowFieldEdit(ContentPartFieldDefinition definition) {
            return definition.Settings.GetModel<ProfileFrontEndSettings>().AllowFrontEndEdit;
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}