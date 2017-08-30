using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.AppDirect.Models;
using Laser.Orchard.AppDirect.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Environment.Configuration;

namespace Laser.Orchard.AppDirect.Driver {
    public class AppDirectSettingsDriver:ContentPartDriver<AppDirectSettingsPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private readonly IRepository<AppDirectSettingsRecord> _repoSetting;
        //private int numeropulsanti = 3;
        public AppDirectSettingsDriver(IOrchardServices orchardServices, ShellSettings shellSettings, IRepository<AppDirectSettingsRecord> repoSetting) {
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;
            _repoSetting = repoSetting;
        }
        protected override string Prefix {
            get { return "Laser.Orchard.AppDirect.Settings"; }
        }


        protected override DriverResult Editor(AppDirectSettingsPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);

        }
        //private string[] sistemaarray(string test) {
        //    if (string.IsNullOrEmpty(test))
        //        test = "£££";
        //    else
        //        test += "£££";
        //    return test.Split('£');
        //}
        //private ButtonToWorkflowsSettingsVM buildmodel(string listaText, string listaAction, string listaDescription, string listaMessage, string listaAsync) {
        //    string[] listText = sistemaarray(listaText);
        //    string[] listAction = sistemaarray(listaAction);
        //    string[] listDescription = sistemaarray(listaDescription);
        //    string[] listMessage = sistemaarray(listaMessage);
        //    string[] listAsync = sistemaarray(listaAsync);
        //    var viewModel = new ButtonToWorkflowsSettingsVM();
        //    viewModel.ButtonsAction1 = listAction[0];
        //    viewModel.ButtonsAction2 = listAction[1];
        //    viewModel.ButtonsAction3 = listAction[2];
        //    viewModel.ButtonsAsync1 = string.IsNullOrEmpty(listAsync[0]) ? false : listAsync[0].ToLower().Equals("true");
        //    viewModel.ButtonsAsync2 = string.IsNullOrEmpty(listAsync[1]) ? false : listAsync[1].ToLower().Equals("true");
        //    viewModel.ButtonsAsync3 = string.IsNullOrEmpty(listAsync[2]) ? false : listAsync[2].ToLower().Equals("true");
        //    viewModel.ButtonsDescription1 = listDescription[0];
        //    viewModel.ButtonsDescription2 = listDescription[1];
        //    viewModel.ButtonsDescription3 = listDescription[2];
        //    viewModel.ButtonsText1 = listText[0];
        //    viewModel.ButtonsText2 = listText[1];
        //    viewModel.ButtonsText3 = listText[2];
        //    viewModel.ButtonsMessage1 = listMessage[0];
        //    viewModel.ButtonsMessage2 = listMessage[1];
        //    viewModel.ButtonsMessage3 = listMessage[2];
        //    return viewModel;
        //}

        protected override DriverResult Editor(AppDirectSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            return ContentShape("Parts_AppDirectSettings_Edit", () => {
                ListAppDirectSettingVM vm = new ListAppDirectSettingVM();
                 //= null;
                vm.ListAppDirectSetting = _repoSetting.Table.ToList().Select(s => new AppDirectSettingVM {
                    Id = s.Id,
                    ConsumerKey = s.ConsumerKey,
                    ConsumerSecret = s.ConsumerSecret,
                    Key = s.TheKey,
                    Delete = false
                });
                //var viewModel = new ButtonToWorkflowsSettingsVM();
                //var getpart = _orchardServices.WorkContext.CurrentSite.As<ButtonToWorkflowsSettingsPart>();
                //viewModel = buildmodel(getpart.ButtonsText, getpart.ButtonsAction, getpart.ButtonsDescription, getpart.ButtonsMessage, getpart.ButtonsAsync);
                ////try {

                ////    string[] listaButton = getpart.ButtonsText.Split('£');
                ////    string[] listaAction = getpart.ButtonsAction.Split('£');
                ////    string[] listaDescription = getpart.ButtonsDescription.Split('£');
                ////    for (int i = 0; i < numeropulsanti; i++) {
                ////        viewModel.
                ////        ButtonVM bvm = new ButtonVM();
                ////        bvm.ButtonsText = listaButton[i];
                ////        bvm.ButtonsAction = listaAction[i];
                ////        viewModel.ListButtons.Add(bvm);
                ////    }
                ////}
                ////catch { }

                if (updater != null) {
                    vm = new ListAppDirectSettingVM();
                    if (updater.TryUpdateModel(vm, Prefix, null, null)) {

                        UpdateOAuth(vm);
                        //string _ButtonsText = "";
                        //string _ButtonsAction = "";
                        //foreach (ButtonVM bvm in viewModel.ListButtons) {
                        //    _ButtonsText += bvm.ButtonsText + "£";
                        //    _ButtonsAction += bvm.ButtonsAction + "£";
                        //}
                        //if (_ButtonsText.Length > 0) {
                        //    _ButtonsText = _ButtonsText.Substring(0, _ButtonsText.Length - 1);
                        //    _ButtonsAction = _ButtonsAction.Substring(0, _ButtonsAction.Length - 1);
                        //}
                        //part.ButtonsAction = _ButtonsAction;
                        //part.ButtonsText = _ButtonsText;
                        //part.ButtonsDescription=                 
                        //part.ButtonsAction = viewModel.ButtonsAction1 + '£' + viewModel.ButtonsAction2 + "£" + viewModel.ButtonsAction3;
                        //part.ButtonsText = viewModel.ButtonsText1 + '£' + viewModel.ButtonsText2 + "£" + viewModel.ButtonsText3;
                        //part.ButtonsDescription = viewModel.ButtonsDescription1 + '£' + viewModel.ButtonsDescription2 + "£" + viewModel.ButtonsDescription3;
                        //part.ButtonsMessage = viewModel.ButtonsMessage1 + '£' + viewModel.ButtonsMessage2 + '£' + viewModel.ButtonsMessage3;
                        //part.ButtonsAsync = viewModel.ButtonsAsync1.ToString() + '£' + viewModel.ButtonsAsync2.ToString() + '£' + viewModel.ButtonsAsync3.ToString();
                    }
                }
                else {
                    part=_orchardServices.WorkContext.CurrentSite.As<AppDirectSettingsPart>();
                    //viewModel = new ButtonToWorkflowsSettingsVM();
                    //viewModel = buildmodel(part.ButtonsText, part.ButtonsAction, part.ButtonsDescription, part.ButtonsMessage, part.ButtonsAsync);
                    ////try {
                    //    string[] listaButton = part.ButtonsText.Split('£');
                    //    string[] listaAction = part.ButtonsAction.Split('£');
                    //    for (int i = 0; i < listaButton.Length; i++) {
                    //        ButtonVM bvm = new ButtonVM();
                    //        if (!(string.IsNullOrEmpty(listaButton[i]) && string.IsNullOrEmpty(listaAction[i]))) {
                    //            bvm.ButtonsText = listaButton[i];
                    //            bvm.ButtonsAction = listaAction[i];
                    //            viewModel.ListButtons.Add(bvm);
                    //        }
                    //    }
                    //}
                    //catch { }
                }
                //ButtonVM emptybvm = new ButtonVM();
                //emptybvm.ButtonsAction = "";
                //emptybvm.ButtonsText = "";
                //viewModel.ListButtons.Add(emptybvm);
                return shapeHelper.EditorTemplate(TemplateName: "Parts/AppDirectSettings_Edit", Model: vm, Prefix: Prefix);
            })
          .OnGroup("AppDirect");
        }
        private void UpdateOAuth(ListAppDirectSettingVM ListOAuth) {
            foreach (var appDirectSetting in ListOAuth.ListAppDirectSetting) {
                AppDirectSettingsRecord appSettingRecord = _repoSetting.Get(appDirectSetting.Id);

                if (appDirectSetting.Delete) {
                    if (appSettingRecord != null)
                        _repoSetting.Delete(appSettingRecord);
                }
                else {
                    if (appSettingRecord == null)
                        _repoSetting.Create(new AppDirectSettingsRecord {
                            TheKey = appDirectSetting.Key,
                            ConsumerKey = appDirectSetting.ConsumerKey,
                            ConsumerSecret = appDirectSetting.ConsumerSecret,
                        });
                    else {
                        appSettingRecord.TheKey = appDirectSetting.Key;
                        appSettingRecord.ConsumerKey = appDirectSetting.ConsumerKey;
                        appSettingRecord.ConsumerSecret = appDirectSetting.ConsumerSecret;
                        _repoSetting.Update(appSettingRecord);
                    }
                }
            }

            _repoSetting.Flush();
        }
    }
}