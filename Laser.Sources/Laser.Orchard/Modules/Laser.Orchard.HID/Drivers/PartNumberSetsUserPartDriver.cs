using Laser.Orchard.HID.Models;
using Laser.Orchard.HID.Services;
using Laser.Orchard.HID.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.UI.Notify;
using System.Linq;

namespace Laser.Orchard.HID.Drivers {
    public class PartNumberSetsUserPartDriver : ContentPartDriver<PartNumberSetsUserPart> {

        private readonly IHIDPartNumbersService _HIDPartNumbersService;
        private readonly INotifier _notifier;

        public PartNumberSetsUserPartDriver(
            IHIDPartNumbersService HIDPartNumbersService,
            INotifier notifier) {

            _HIDPartNumbersService = HIDPartNumbersService;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "PartNumberSetsUserPart"; } }

        protected override DriverResult Editor(PartNumberSetsUserPart part, dynamic shapeHelper) {

            var allSets = _HIDPartNumbersService.GetAllSets();

            var model = new PartNumberSetsUserPartEditViewModel {
                Sets = allSets.Select(pns => new PartNumberSetsUserPartEditEntry {
                        Name = pns.Name,
                        IsSelected = part.PartNumberSets.Any(set => set.Id == pns.Id),
                        Id = pns.Id
                    }).ToList()
            };

            return ContentShape("Parts_PartNumberSetsUserPart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/PartNumberSetsUserPart",
                    Model: model,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(PartNumberSetsUserPart part, IUpdateModel updater, dynamic shapeHelper) {

            var vm = new PartNumberSetsUserPartEditViewModel();
            if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                // Update the part
                var context = _HIDPartNumbersService.UpdatePart(part, vm);
                if (context.UserErrors.Any()) {
                    _notifier.Error(T(context.ErrorSummary()));
                }
            }
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(PartNumberSetsUserPart part, ExportContentContext context) {
            // TODO
            base.Exporting(part, context);
        }

        protected override void Importing(PartNumberSetsUserPart part, ImportContentContext context) {
            // TODO
            base.Importing(part, context);
        }
    }
}