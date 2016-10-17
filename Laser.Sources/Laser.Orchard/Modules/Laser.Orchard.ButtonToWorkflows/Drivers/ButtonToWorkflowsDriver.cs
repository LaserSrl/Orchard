using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard;
using Orchard.Localization;
using Laser.Orchard.ButtonToWorkflows.Models;
using Orchard.ContentManagement.Handlers;
using Laser.Orchard.ButtonToWorkflows.ViewModels;
using Orchard.Workflows.Services;
using Laser.Orchard.ButtonToWorkflows.Settings;
using Orchard.UI.Notify;

namespace Laser.Orchard.ButtonToWorkflows.Drivers {
    public class ButtonToWorkflowsDriver : ContentPartDriver<ButtonToWorkflowsPart> {
        private readonly IOrchardServices _orchardServices;
        public Localizer T { get; set; }
        private readonly IWorkflowManager _workflowManager;
        private readonly INotifier _notifier;
        protected override string Prefix {
            get { return "Laser.Mobile.ButtonToWorkflows"; }
        }

        public ButtonToWorkflowsDriver(IOrchardServices orchardServices, IWorkflowManager workflowManager, INotifier notifier) {
            _orchardServices = orchardServices;
            _workflowManager = workflowManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Editor(ButtonToWorkflowsPart part, dynamic shapeHelper) {
            var model = new ButtonToWorkflowsVM(part);
            var settings = part.TypePartDefinition.Settings.GetModel<ButtonsSetting>();
            settings.ButtonNumber = settings.ButtonNumber[0].Split(',');
            //    model.ButtonText = settings.ButtonText;
            ButtonToWorkflowsSettingsPart settingmodulo = _orchardServices.WorkContext.CurrentSite.As<ButtonToWorkflowsSettingsPart>();
            try {
                string[] elencoButtonsText = settingmodulo.ButtonsText.Split('£');
                string[] elencoButtonsAction = settingmodulo.ButtonsAction.Split('£');
                foreach (string intbutton in settings.ButtonNumber) {
                    model.ElencoButtons.Where(x => x.ButtonNumber == Convert.ToInt32(intbutton)).FirstOrDefault().ButtonAction = elencoButtonsAction[Convert.ToInt32(intbutton)];
                    model.ElencoButtons.Where(x => x.ButtonNumber == Convert.ToInt32(intbutton)).FirstOrDefault().ButtonText = elencoButtonsText[Convert.ToInt32(intbutton)];
                }
                //for (int i = 0; i < elencoButtonsText.Count(); i++) {
                //    if (elencoButtonsText[i] == model.ButtonText) {
                //        model.ButtonAction = elencoButtonsAction[i];
                //        model.ButtonText = settings.ButtonText;
                //    }
                //}
            } catch { }
            return ContentShape("Parts_ButtonToWorkflows", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ButtonToWorkflows", Model: model, Prefix: Prefix));


        }

        protected override DriverResult Editor(ButtonToWorkflowsPart part, IUpdateModel updater, dynamic shapeHelper) {
            var bigmodel = new ButtonToWorkflowsVM(part);
            if (updater.TryUpdateModel(bigmodel, Prefix, null, null))

                if (part.ContentItem.Id != 0) {
                    foreach (ButtonToWorkflowsVMItem model in bigmodel.ElencoButtons)
                        if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Save"] == "submit.CustomButton" + model.ButtonText) {
                            var content = _orchardServices.ContentManager.Get(part.ContentItem.Id, VersionOptions.Latest);
                            //_workflowManager.TriggerEvent("ButtonToWorkflowsSubmitted", content, () => new Dictionary<string, object> { { "Content", content } });
                            //   var settings = part.TypePartDefinition.Settings.GetModel<ButtonsSetting>();
                            //   settings.ButtonNumber = settings.ButtonNumber[0].Split(',');
                            ButtonToWorkflowsSettingsPart settingmodulo = _orchardServices.WorkContext.CurrentSite.As<ButtonToWorkflowsSettingsPart>();
                            string[] elencoButtonsMessage = settingmodulo.ButtonsMessage.Split('£');
                            part.ActionToExecute = model.ButtonAction + "_btn" + (model.ButtonNumber + 1).ToString();
                            //   _workflowManager.TriggerEvent(model.ButtonAction+"_btn"+(model.ButtonNumber+1).ToString(), content, () => new Dictionary<string, object> { { "Content", content } });
                            part.MessageToWrite = elencoButtonsMessage[model.ButtonNumber].ToString();
                            //try {
                            //      _notifier.Add(NotifyType.Information, T(elencoButtonsMessage[Convert.ToInt16(settings.ButtonNumber)]));
                            //  }
                            //  catch { }

                        }
                } else {
                    updater.AddModelError("Error Saving Content Item", T("Error Saving Content Item"));
                }
            //  var viewModel = new ButtonToWorkflowsVM();
            return ContentShape("Parts_ButtonToWorkflows", () => shapeHelper.EditorTemplate(TemplateName: "Parts/ButtonToWorkflows", Model: bigmodel, Prefix: Prefix));
        }
        protected override void Importing(ButtonToWorkflowsPart part, ImportContentContext context) {
            throw new NotImplementedException();
            //TODO: Effettuare check su consistenza UserId
            //var root = context.Data.Element(part.PartDefinition.Name);
            //part.FromUser = root.Attribute("FromUser").Value;
            //part.ToUser = root.Attribute("ToUser").Value;
            //int nTempId;
            //if (int.TryParse(root.Attribute("FromIdUser").Value, out nTempId)) {
            //    part.FromIdUser = int.Parse(root.Attribute("FromIdUser").Value);
            //}
            //if (int.TryParse(root.Attribute("ToIdUser").Value, out nTempId)) {
            //    part.FromIdUser = int.Parse(root.Attribute("ToIdUser").Value);
            //}
            //part.ActionToExecute = root.Attribute("ActionToExecute").Value;
            //part.MessageToWrite = root.Attribute("MessageToWrite").Value;
        }
        protected override void Exporting(ButtonToWorkflowsPart part, ExportContentContext context) {

            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("FromUser", part.FromUser);
            root.SetAttributeValue("ToUser", part.ToUser);
            root.SetAttributeValue("FromIdUser", part.FromIdUser);
            root.SetAttributeValue("ToIdUser", part.ToIdUser);
            root.SetAttributeValue("ActionToExecute", part.ActionToExecute);
            root.SetAttributeValue("MessageToWrite", part.MessageToWrite);



        }


    }
}