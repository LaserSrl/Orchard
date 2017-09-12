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
using Orchard.ContentManagement.Handlers;
using System.Xml.Linq;

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
        protected override void Exporting(DynamicButtonToWorkflowsSettingsPart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            foreach (var button in _dynamicButtonToWorkflowsService.GetButtons().OrderBy(x => x.Id)) { 
                XElement buttonSettings = new XElement("ButtonSettings");
                buttonSettings.SetAttributeValue("ButtonName", button.ButtonName);
                buttonSettings.SetAttributeValue("ButtonText", button.ButtonText);
                buttonSettings.SetAttributeValue("ButtonDescription", button.ButtonDescription);
                buttonSettings.SetAttributeValue("ButtonMessage", button.ButtonMessage);
                buttonSettings.SetAttributeValue("ButtonAsync", button.ButtonAsync);
                root.Add(buttonSettings);
            }
        }
        protected override void Importing(DynamicButtonToWorkflowsSettingsPart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            var newDefinitions = new List<DynamicButtonToWorkflowsEdit>();
            foreach(var def in _dynamicButtonToWorkflowsService.GetButtons().OrderBy(x => x.Id)) {
                newDefinitions.Add(new DynamicButtonToWorkflowsEdit {
                    Id = def.Id,
                    ButtonName = def.ButtonName,
                    ButtonText = def.ButtonText,
                    ButtonDescription = def.ButtonDescription,
                    ButtonMessage = def.ButtonMessage,
                    ButtonAsync = def.ButtonAsync,
                    Delete = false
                });
            }
            var buttonSettings = root.Elements("ButtonSettings");
            int idx = 0;
            foreach(var button in buttonSettings) {
                if (newDefinitions.Count > idx) {
                    newDefinitions[idx].ButtonName = button.Attribute("ButtonName") != null ? button.Attribute("ButtonName").Value : "";
                    newDefinitions[idx].ButtonText = button.Attribute("ButtonText") != null ? button.Attribute("ButtonText").Value : "";
                    newDefinitions[idx].ButtonDescription = button.Attribute("ButtonDescription") != null ? button.Attribute("ButtonDescription").Value : "";
                    newDefinitions[idx].ButtonMessage = button.Attribute("ButtonMessage") != null ? button.Attribute("ButtonMessage").Value : "";
                    newDefinitions[idx].ButtonAsync = button.Attribute("ButtonAsync") != null ? bool.Parse(button.Attribute("ButtonAsync").Value) : false;
                } else {
                    newDefinitions.Add(new DynamicButtonToWorkflowsEdit {
                        ButtonName = button.Attribute("ButtonName") != null ? button.Attribute("ButtonName").Value : "",
                        ButtonText = button.Attribute("ButtonText") != null ? button.Attribute("ButtonText").Value : "",
                        ButtonDescription = button.Attribute("ButtonDescription") != null ? button.Attribute("ButtonDescription").Value : "",
                        ButtonMessage = button.Attribute("ButtonMessage") != null ? button.Attribute("ButtonMessage").Value : "",
                        ButtonAsync = button.Attribute("ButtonAsync") != null ? bool.Parse(button.Attribute("ButtonAsync").Value) : false,
                        Delete = false
                    });
                }
                idx++;
            }
            _dynamicButtonToWorkflowsService.UpdateButtons(newDefinitions);
        }
    }
}