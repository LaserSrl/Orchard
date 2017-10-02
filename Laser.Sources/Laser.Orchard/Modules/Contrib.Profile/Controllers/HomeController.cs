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

            dynamic shape = Services.ContentManager.BuildDisplay(BuildFrontEndShape(user, MayAllowPartDisplay, MayAllowFieldDisplay));

            return View((object)shape);
        }

        public ActionResult Edit() {
            IUser user = Services.WorkContext.CurrentUser;

            if (user == null ||
                UserHasNoProfilePart(user)) {
                return HttpNotFound();
            }
            
            dynamic shape = Services.ContentManager.BuildEditor(BuildFrontEndShape(user, MayAllowPartEdit, MayAllowFieldEdit));

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

            var uItem = BuildFrontEndShape(user, MayAllowPartEdit, MayAllowFieldEdit);
            dynamic shape = Services.ContentManager.UpdateEditor(uItem, this);
            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View("Edit", (object)shape);
            }
            user = _contentManager.Get<IUser>(userId, VersionOptions.DraftRequired);
            uItem.VersionRecord = user.ContentItem.VersionRecord;
            _contentManager.Publish(uItem);
            Services.Notifier.Information(T("Your profile has been saved."));

            return RedirectToAction("Edit");
        }
        
        private ContentItem BuildFrontEndShape(
            IUser user, 
            Func<ContentTypePartDefinition, string, bool> partTest, 
            Func<ContentPartFieldDefinition, bool> fieldTest) {

            //based on the Type definition we find, we will build a new type definition
            //that allows us to only show what we may.
            var guid = Guid.NewGuid().ToString(); //used to generate type name
            var editTypeName = "UserEdit" + guid;
            var userTypeDefinition = user.ContentItem.TypeDefinition;
            var contentTypeDefinition =
                new ContentTypeDefinition(editTypeName, editTypeName,
                Enumerable.Empty<ContentTypePartDefinition>(), new SettingsDictionary());

            var builder = new ContentItemBuilder(contentTypeDefinition);

            foreach (var typePartDefinition in userTypeDefinition.Parts
                .Where(ctpd => partTest(ctpd, userTypeDefinition.Name))) {

                var part = user
                    .ContentItem
                    .Parts
                    .Where(pa => pa.PartDefinition.Name == typePartDefinition.PartDefinition.Name)
                    .FirstOrDefault();

                if (part.Fields.Any(fi => !fieldTest(fi.PartFieldDefinition))) {
                    var myPart = (ContentPart)(Activator.CreateInstance(part.GetType()));
                    var fieldDefinitions = new List<ContentPartFieldDefinition>();
                    fieldDefinitions.AddRange(part
                        .PartDefinition.Fields.Where(cpfd =>
                        fieldTest(cpfd)));
                    var contentPartDefinition = new ContentPartDefinition(part.PartDefinition.Name, fieldDefinitions, part.PartDefinition.Settings);
                    var partDefinition = new ContentTypePartDefinition(contentPartDefinition, part.TypePartDefinition.Settings);
                    myPart.TypePartDefinition = partDefinition;
                    foreach (var field in part.Fields.Where(fi => fieldTest(fi.PartFieldDefinition))) {
                        ((ContentPart)myPart).Weld(field);
                    }
                    builder.Weld((ContentPart)myPart);
                } else {
                    builder.Weld(part);
                }
            }

            var item = builder.Build();
            item.ContentManager = _contentManager;

            //add all the parts that were welded dynamically to user. We must do this, because the UserPart
            //is added like this.
            foreach (var part in user.ContentItem.Parts
                .Where(pa => !userTypeDefinition
                    .Parts
                    .Select(ctpd => ctpd.PartDefinition.Name)
                    .Contains(pa.PartDefinition.Name))) {
                //builder.Weld(part);
                ((IList<ContentPart>)(item.Parts)).Add(part);
            }

            return item;
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