using AutoMapper;
using Laser.Orchard.Facebook.Models;
using Laser.Orchard.Facebook.Services;
using Laser.Orchard.Facebook.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Users;
using Orchard.Users.Models;


namespace Laser.Orchard.Facebook.Drivers {

    public class FacebookAccountDriver : ContentPartDriver<FacebookAccountPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IFacebookService _facebookService;
        private readonly IContentManager _contentManager;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.Facebook"; }
        }

        public FacebookAccountDriver(IOrchardServices orchardServices, IFacebookService facebookService,
                                     IContentManager contentManager) {
            _orchardServices = orchardServices;
            _facebookService = facebookService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
        }

        protected override DriverResult Editor(FacebookAccountPart part, dynamic shapeHelper) {
            FacebookAccountVM vm = new FacebookAccountVM();
            Mapper.CreateMap<FacebookAccountPart, FacebookAccountVM>();
            Mapper.Map(part, vm);

            return ContentShape("Parts_FacebookAccount",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/FacebookAccount",
                                    Model: vm,
                                    Prefix: Prefix));
        }

        protected override DriverResult Editor(FacebookAccountPart part, IUpdateModel updater, dynamic shapeHelper) {
            FacebookAccountVM vm = new FacebookAccountVM();
            updater.TryUpdateModel(vm, Prefix, null, null);
            Mapper.CreateMap<FacebookAccountVM, FacebookAccountPart>();
            Mapper.Map(vm, part);
            return Editor(part, shapeHelper);
        }


        protected override void Importing(FacebookAccountPart part, ImportContentContext context) {

            // mod. 06-12-2016
            var importedIdPage = context.Attribute(part.PartDefinition.Name, "IdPage");
            if (importedIdPage != null) {
                part.IdPage = importedIdPage;
            }

            //mod 06-12-2016
            context.ImportAttribute(part.PartDefinition.Name, "IdUser", x => {
                var tempPartFromid = context.GetItemFromSession(x);

                if (tempPartFromid != null && tempPartFromid.Is<UserPart>()) {
                    //associa id user
                    part.IdUser = tempPartFromid.As<UserPart>().Id;
                }
            });


            var importedSocialName = context.Attribute(part.PartDefinition.Name, "SocialName");
            if (importedSocialName != null) {
                part.SocialName = importedSocialName;
            }

            var importedAccountType = context.Attribute(part.PartDefinition.Name, "AccountType");
            if (importedAccountType != null) {
                part.AccountType = importedAccountType;
            }

            var importedUserToken = context.Attribute(part.PartDefinition.Name, "UserToken");
            if (importedUserToken != null) {
                part.UserToken = importedUserToken;
            }

            var importedPageToken = context.Attribute(part.PartDefinition.Name, "PageToken");
            if (importedPageToken != null) {
                part.PageToken = importedPageToken;
            }


            var importedShared = context.Attribute(part.PartDefinition.Name, "Shared");
            if (importedShared != null) {
                part.Shared = bool.Parse(importedShared);
            }

            var importedPageName = context.Attribute(part.PartDefinition.Name, "PageName");
            if (importedPageName != null) {
                part.PageName = importedPageName;
            }

            var importedValid = context.Attribute(part.PartDefinition.Name, "Valid");
            if (importedValid != null) {
                part.Valid = bool.Parse(importedValid);
            }

            var importedDisplayAs = context.Attribute(part.PartDefinition.Name, "DisplayAs");
            if (importedDisplayAs != null) {
                part.DisplayAs = importedDisplayAs;
            }

            var importedUserIdFacebook = context.Attribute(part.PartDefinition.Name, "UserName");
            if (importedUserIdFacebook != null) {
                part.UserIdFacebook = importedUserIdFacebook;
            }

            var importedUserName = context.Attribute(part.PartDefinition.Name, "UserIdFacebook");
            if (importedUserName != null) {
                part.UserName = importedUserName;
            }

        }


        protected override void Exporting(FacebookAccountPart part, ExportContentContext context) {

            //mod. 06-12-2016
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("IdPage", part.IdPage);
          
            if (part.IdUser > 0) {
                //cerco il corrispondente valore dell' identity dalla partse lo associo al campo iduser 
                var contItemUser = _contentManager.Get(part.IdUser);
                if (contItemUser != null) {
                    root.SetAttributeValue("IdUser", _contentManager.GetItemMetadata(contItemUser).Identity.ToString());
                }
            }

            root.SetAttributeValue("SocialName", part.SocialName);
            root.SetAttributeValue("AccountType", part.AccountType);
            root.SetAttributeValue("UserToken", part.UserToken);
            root.SetAttributeValue("PageToken", part.PageToken);

            root.SetAttributeValue("Shared", part.Shared);
            root.SetAttributeValue("PageName", part.PageName);

            root.SetAttributeValue("Valid", part.Valid);
            root.SetAttributeValue("DisplayAs", part.DisplayAs);
            root.SetAttributeValue("UserIdFacebook", part.UserIdFacebook);
            root.SetAttributeValue("UserName", part.UserName);
        }


    }
}