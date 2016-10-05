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

namespace Laser.Orchard.Facebook.Drivers {

    public class FacebookAccountDriver : ContentPartDriver<FacebookAccountPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IFacebookService _facebookService;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.Facebook"; }
        }

        public FacebookAccountDriver(IOrchardServices orchardServices, IFacebookService facebookService) {
            _orchardServices = orchardServices;
            _facebookService = facebookService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
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

            var importedIdPage = context.Attribute(part.PartDefinition.Name, "IdPage");
            if (importedIdPage != null) {
                part.IdPage = importedIdPage;
            }

            var importedIdUser = context.Attribute(part.PartDefinition.Name, "IdUser");
            if (importedIdUser != null) {
                part.IdUser = int.Parse(importedIdUser);
            }

            var importedShared = context.Attribute(part.PartDefinition.Name, "Shared");
            if (importedShared != null) {
                part.Shared = bool.Parse(importedShared);
            }

            var importedPageName = context.Attribute(part.PartDefinition.Name, "PageName");
            if (importedPageName != null) {
                part.Shared = bool.Parse(importedPageName);
            }

        }


        protected override void Exporting(FacebookAccountPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("SocialName", part.SocialName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("AccountType", part.AccountType);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UserToken", part.UserToken);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PageToken", part.PageToken);
            context.Element(part.PartDefinition.Name).SetAttributeValue("IdPage", part.IdPage);
            context.Element(part.PartDefinition.Name).SetAttributeValue("IdUser", part.IdUser);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Shared", part.Shared);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PageName", part.PageName);
        }


    }
}