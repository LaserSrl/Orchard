using System.Collections.Generic;
using Laser.Orchard.ButtonToWorkflows.Models;
using Laser.Orchard.ButtonToWorkflows.Services;
using Laser.Orchard.ButtonToWorkflows.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System.Linq;
using Orchard.Localization;

namespace Laser.Orchard.ButtonToWorkflows.Drivers {

    [OrchardFeature("Laser.Orchard.ButtonToWorkflows")]
    public class DynamicButtonToWorkflowsSettingsPartDriver : ContentPartDriver<DynamicButtonToWorkflowsSettingsPart> {

        private readonly IDynamicButtonToWorkflowsService _dynamicButtonToWorkflowsService;
        private readonly IControllerContextAccessor _controllerContextAccessor;

        private const string TemplateName = "Parts/DynamicButtonToWorkflowsSettings";

        public Localizer T { get; set; }

        public DynamicButtonToWorkflowsSettingsPartDriver(
            IDynamicButtonToWorkflowsService dynamicButtonToWorkflowsService,
            IControllerContextAccessor controllerContextAccessor) {
            _dynamicButtonToWorkflowsService = dynamicButtonToWorkflowsService;
            _controllerContextAccessor = controllerContextAccessor;
            T = NullLocalizer.Instance;
        }

        protected override string Prefix
        {
            get { return "Laser.DynamicButtonToWorkflows.Settings"; }
        }

        protected override DriverResult Editor(DynamicButtonToWorkflowsSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_DynamicButtonToWorkflowsSettings_Edit",
                () => {
                    IEnumerable<DynamicButtonToWorkflowsEdit> buttons = null;
                    var buttonsWithErrors = _controllerContextAccessor.Context.Controller.TempData[Prefix + "ButtonsWithErrors"];
                    if (buttonsWithErrors == null)
                        buttons = _dynamicButtonToWorkflowsService.GetButtons().Select(s => new DynamicButtonToWorkflowsEdit {
                            Id = s.Id,
                            ButtonName = s.ButtonName,
                            ButtonText = s.ButtonText,
                            ButtonDescription = s.ButtonDescription,
                            ButtonMessage = s.ButtonMessage,
                            ButtonAsync = s.ButtonAsync,
                            Delete = false
                        });
                    else
                        buttons = ((IEnumerable<DynamicButtonToWorkflowsEdit>)buttonsWithErrors).Where(x => x.Delete == false);

                    var model = new DynamicButtonToWorkflowsSettingsVM {
                        Buttons = buttons
                    };

                    return shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: model,
                    Prefix: Prefix);
                }).OnGroup("Buttons");
        }

        protected override DriverResult Editor(DynamicButtonToWorkflowsSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            DynamicButtonToWorkflowsSettingsVM dynamicButtonSettingsVM = new DynamicButtonToWorkflowsSettingsVM();

            if (updater.TryUpdateModel(dynamicButtonSettingsVM, Prefix, null, null)) {
                var buttonsByName = dynamicButtonSettingsVM.Buttons.GroupBy(q => new { q.ButtonName }).Select(q => new { q.Key.ButtonName, Occurrences = q.Count() });
                buttonsByName = buttonsByName.Where(q => q.Occurrences > 1);

                if (buttonsByName.ToList().Count() > 0) {
                    _controllerContextAccessor.Context.Controller.TempData[Prefix + "ButtonsWithErrors"] = dynamicButtonSettingsVM.Buttons;
                    updater.AddModelError("ButtonUpdateError", T("Cannot have multiple buttons with the same name ({0})", string.Join(", ", buttonsByName.Select(q => q.ButtonName))));
                }
                else {
                    _dynamicButtonToWorkflowsService.UpdateButtons(dynamicButtonSettingsVM.Buttons);
                }
            }
            else {
                _controllerContextAccessor.Context.Controller.TempData[Prefix + "ButtonsWithErrors"] = dynamicButtonSettingsVM.Buttons;
            }

            return Editor(part, shapeHelper);
        }
    }
}