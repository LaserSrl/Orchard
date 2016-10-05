using AutoMapper;
using Laser.Orchard.Twitter.Models;
using Laser.Orchard.Twitter.Services;
using Laser.Orchard.Twitter.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Logging;

namespace Laser.Orchard.Twitter.Drivers {

    public class TwitterAccountDriver : ContentPartDriver<TwitterAccountPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly ITwitterService _TwitterService;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.Twitter"; }
        }

        public TwitterAccountDriver(IOrchardServices orchardServices, ITwitterService TwitterService) {
            _orchardServices = orchardServices;
            _TwitterService = TwitterService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(TwitterAccountPart part, dynamic shapeHelper) {
            TwitterAccountVM vm = new TwitterAccountVM();
            Mapper.CreateMap<TwitterAccountPart, TwitterAccountVM>();
            Mapper.Map(part, vm);

            return ContentShape("Parts_TwitterAccount",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/TwitterAccount",
                                    Model: vm,
                                    Prefix: Prefix));
        }

        protected override DriverResult Editor(TwitterAccountPart part, IUpdateModel updater, dynamic shapeHelper) {
            TwitterAccountVM vm = new TwitterAccountVM();
            updater.TryUpdateModel(vm, Prefix, null, null);
            Mapper.CreateMap<TwitterAccountVM, TwitterAccountPart>();
            Mapper.Map(vm, part);
            return Editor(part, shapeHelper);
        }


        protected override void Importing(TwitterAccountPart part, ImportContentContext context) {
            
            var importedSocialName = context.Attribute(part.PartDefinition.Name, "SocialName");
            if (importedSocialName != null) {
                part.SocialName = importedSocialName;
            }

            var importedAccountType = context.Attribute(part.PartDefinition.Name, "AccountType");
            if (importedAccountType != null) {
                part.AccountType = importedAccountType;
            }

            var importedUserTokenSecret = context.Attribute(part.PartDefinition.Name, "UserTokenSecret");
            if (importedUserTokenSecret != null) {
                part.UserTokenSecret = importedUserTokenSecret;
            }

            var importedIdUser = context.Attribute(part.PartDefinition.Name, "IdUser");
            if (importedIdUser != null) {
                part.IdUser = int.Parse(importedIdUser);
            }

            var importedShared = context.Attribute(part.PartDefinition.Name, "Shared");
            if (importedShared != null) {
                part.Shared = bool.Parse(importedShared);
            }

            var importedValid = context.Attribute(part.PartDefinition.Name, "Valid");
            if (importedValid != null) {
                part.Valid = bool.Parse(importedValid);
            }

            var importedDisplayAs = context.Attribute(part.PartDefinition.Name, "DisplayAs");
            if (importedDisplayAs != null) {
                part.DisplayAs = importedDisplayAs;
            }
        }

        protected override void Exporting(TwitterAccountPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("SocialName", part.SocialName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("AccountType", part.AccountType);
            context.Element(part.PartDefinition.Name).SetAttributeValue("UserTokenSecret", part.UserTokenSecret);
            context.Element(part.PartDefinition.Name).SetAttributeValue("IdUser", part.IdUser);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Shared", part.Shared);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Valid", part.Valid);
            context.Element(part.PartDefinition.Name).SetAttributeValue("DisplayAs", part.DisplayAs);
        }


    }
}