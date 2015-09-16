using AutoMapper;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Laser.Orchard.StartupConfig.Drivers {

    [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]
    public class MaintenancePartDriver : ContentPartDriver<MaintenancePart> {
       // public Localizer T { get; set; }

        protected override string Prefix {
            get { return "MaintenancePartDriver"; }
        }

        //GET
        protected override DriverResult Editor(MaintenancePart part, dynamic shapeHelper) {
            MaintenanceVM MaintenanceVM = new MaintenanceVM();
            Mapper.CreateMap<MaintenancePart, MaintenanceVM>();
            Mapper.Map(part, MaintenanceVM);
            return ContentShape("Parts_Maintenance_Edit",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts/Maintenance_Edit",
                        Model: MaintenanceVM,
                        Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(MaintenancePart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(part, Prefix, null, null)) {
            }
            else {
                //foreach (var modelState in ModelState.Values) {
                //    foreach (var error in modelState.Errors) {
                //        Debug.WriteLine(error.ErrorMessage);
                //    }
                //}
                //  _notifier.Add(NotifyType.Error, T("Error on udpate google analytics"));
            }
            return Editor(part, shapeHelper);
        }
    }
}