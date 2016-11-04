using AutoMapper;
using Laser.Orchard.Twitter.Models;
using Laser.Orchard.Twitter.Services;
using Laser.Orchard.Twitter.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
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
            Mapper.Initialize(cfg => {
                cfg.CreateMap<TwitterAccountPart, TwitterAccountVM>();
            });
            Mapper.Map<TwitterAccountPart, TwitterAccountVM>(part, vm);

            return ContentShape("Parts_TwitterAccount",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/TwitterAccount",
                                    Model: vm,
                                    Prefix: Prefix));
        }

        protected override DriverResult Editor(TwitterAccountPart part, IUpdateModel updater, dynamic shapeHelper) {
            TwitterAccountVM vm = new TwitterAccountVM();
            updater.TryUpdateModel(vm, Prefix, null, null);
            Mapper.Initialize(cfg => {
                cfg.CreateMap<TwitterAccountVM, TwitterAccountPart>();
            });
            Mapper.Map<TwitterAccountVM, TwitterAccountPart>(vm, part);
            return Editor(part, shapeHelper);
        }
    }
}