using AutoMapper;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;


namespace Laser.Orchard.Mobile.Drivers {

    [OrchardFeature("Laser.Orchard.Sms")]
    public class UserPwdRecoveryPartDriver : ContentPartCloningDriver<UserPwdRecoveryPart> {

        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Mobile.UserPwdRecovery"; }
        }

        public UserPwdRecoveryPartDriver(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        protected override DriverResult Editor(UserPwdRecoveryPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);

        }

        protected override DriverResult Editor(UserPwdRecoveryPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater != null) {
                updater.TryUpdateModel(part, Prefix, null, null);
            }
            return ContentShape("Parts_UserPwdRecoveryPart_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/UserPwdRecoveryPart_Edit", Model: part, Prefix: Prefix));
        }

        //TODO: UserPwdRecoveryPart Import/Export

        protected override void Cloning(UserPwdRecoveryPart originalPart, UserPwdRecoveryPart clonePart, CloneContentContext context) {
            clonePart.InternationalPrefix = originalPart.InternationalPrefix;
            clonePart.PhoneNumber = originalPart.PhoneNumber;
        }
    }
}