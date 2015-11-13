using AutoMapper;
using Laser.Orchard.Facebook.Models;
using Laser.Orchard.Facebook.Services;
using Laser.Orchard.Facebook.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
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
    }
}