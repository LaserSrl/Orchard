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

        public HomeController(IOrchardServices services,
            IMembershipService membershipService) {

            _membershipService = membershipService;

            Services = services;
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(string username) {
            IUser user = _membershipService.GetUser(username);

            if(user == null ||
                UserHasNoProfilePart(user) ||
                !Services.Authorizer.Authorize(Permissions.ViewProfiles, user, null)) {
                return HttpNotFound();
            }

            //based on the Type definition we find, we will build a new type definition
            //that allows us to only show what we may.
            var guid = Guid.NewGuid().ToString(); //used to generate type name
            var viewTypeName = "UserView" + guid;
            var userTypeDefinition = user.ContentItem.TypeDefinition;
            var partDefinitions = new List<ContentTypePartDefinition>();
            var parts = new List<ContentPart>();
            foreach (var typePartDefinition in userTypeDefinition.Parts
                .Where(ctpd => MayAllowPartDisplay(ctpd, userTypeDefinition.Name))) {

                partDefinitions.Add(typePartDefinition);
                parts.Add(user
                    .ContentItem
                    .Parts
                    .Where(pa => pa.PartDefinition.Name == typePartDefinition.PartDefinition.Name)
                    .FirstOrDefault());
            }
            parts.AddRange(
                user.ContentItem.Parts
                .Where(pa => !userTypeDefinition
                    .Parts
                    .Select(ctpd => ctpd.PartDefinition.Name)
                    .Contains(pa.PartDefinition.Name))
                );

            var contentTypeDefinition = 
                new ContentTypeDefinition(viewTypeName, viewTypeName,
                partDefinitions, new SettingsDictionary());

            var builder = new ContentItemBuilder(contentTypeDefinition);
            foreach (var part in parts) {
                builder.Weld(part);
            }

            //dynamic shape = Services.ContentManager.BuildDisplay(user.ContentItem);
            dynamic shape = Services.ContentManager.BuildDisplay(builder.Build());

            //foreach (var part in parts) {
            //    user.ContentItem.Weld(part);
            //}

            return View((object)shape);
        }

        public ActionResult Edit() {
            IUser user = Services.WorkContext.CurrentUser;

            if (user == null ||
                UserHasNoProfilePart(user)) {
                return HttpNotFound();
            }


            dynamic shape = Services.ContentManager.BuildEditor(user.ContentItem);

            return View((object)shape);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost() {
            IUser user = Services.WorkContext.CurrentUser;

            if (user == null ||
                UserHasNoProfilePart(user)) {
                return HttpNotFound();
            }

            dynamic shape = Services.ContentManager.UpdateEditor(user.ContentItem, this);
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View("Edit", (object)shape);
            }

            Services.Notifier.Information(T("Your profile has been saved."));

            return RedirectToAction("Edit");
        }

        private bool UserHasNoProfilePart(IUser user) {
            return user.As<ProfilePart>() == null && user.ContentItem.As<ProfilePart>() == null;
        }

        private bool MayAllowPartDisplay(ContentTypePartDefinition definition, string typeName) {
            return definition.PartDefinition.Name == typeName || //this is to account for fields added to the type
                definition.Settings.GetModel<ProfileFrontEndSettings>().AllowFrontEndDisplay;
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}