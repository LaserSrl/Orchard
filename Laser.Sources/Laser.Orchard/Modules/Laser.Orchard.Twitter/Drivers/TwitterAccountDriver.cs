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
using Orchard.Users.Models;

namespace Laser.Orchard.Twitter.Drivers {

    public class TwitterAccountDriver : ContentPartDriver<TwitterAccountPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly ITwitterService _TwitterService;
        IContentManager _contentManager;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Orchard.Twitter"; }
        }

        public TwitterAccountDriver(IOrchardServices orchardServices, ITwitterService TwitterService,
                                    IContentManager contentManager) {
            _orchardServices = orchardServices;
            _TwitterService = TwitterService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
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

            var importedUserTokenSecret = context.Attribute(part.PartDefinition.Name, "UserTokenSecret");
            if (importedUserTokenSecret != null) {
                part.UserTokenSecret = importedUserTokenSecret;
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

            //06-12-2016
            var root = context.Element(part.PartDefinition.Name);

            if (part.IdUser > 0) {
                //cerco il corrispondente valore dell' identity dalla partse lo associo al campo iduser 
                var contItemUser = _contentManager.Get(part.IdUser);
                if (contItemUser != null) {
                    root.SetAttributeValue("IdUser", _contentManager.GetItemMetadata(contItemUser).Identity.ToString());
                }
            }


            root.SetAttributeValue("SocialName", part.SocialName);
            root.SetAttributeValue("AccountType", part.AccountType);
            root.SetAttributeValue("UserTokenSecret", part.UserTokenSecret);
            root.SetAttributeValue("Shared", part.Shared);
            root.SetAttributeValue("Valid", part.Valid);
            root.SetAttributeValue("DisplayAs", part.DisplayAs);
        }


    }
}