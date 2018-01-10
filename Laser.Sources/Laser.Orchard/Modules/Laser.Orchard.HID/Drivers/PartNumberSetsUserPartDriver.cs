using Laser.Orchard.HID.Models;
using Orchard.ContentManagement.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.CompilerServices;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Laser.Orchard.HID.Services;
using Laser.Orchard.HID.ViewModels;

namespace Laser.Orchard.HID.Drivers {
    public class PartNumberSetsUserPartDriver : ContentPartDriver<PartNumberSetsUserPart> {

        private readonly IHIDPartNumbersService _HIDPartNumbersService;

        public PartNumberSetsUserPartDriver(
            IHIDPartNumbersService HIDPartNumbersService) {

            _HIDPartNumbersService = HIDPartNumbersService;
        }

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
                _HIDPartNumbersService.UpdatePart(part, vm);
            }
            // TODO
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