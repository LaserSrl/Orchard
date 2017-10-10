using System.Linq;
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

        protected override DriverResult Editor(AppDirectSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_AppDirectSettings_Edit", () => {
                var baseurl = "";
             //   part = _orchardServices.WorkContext.CurrentSite.As<AppDirectSettingsPart>();
                if (part != null)
                    baseurl = part.BaseUrl;
                var vm = new ListAppDirectSettingVM {
                    ListAppDirectSetting = _repoSetting.Table.ToList().Select(s => new AppDirectSettingVM {
                        Id = s.Id,
                        ConsumerKey = s.ConsumerKey,
                        ConsumerSecret = s.ConsumerSecret,
                        Key = s.TheKey,
                        Delete = false
                    }),
                    BaseUrl = baseurl
                };
                
                if (updater != null) {
                    vm = new ListAppDirectSettingVM();
                    if (updater.TryUpdateModel(vm, Prefix, null, null)) {
                        UpdateOAuth(vm);
                        part.BaseUrl = vm.BaseUrl;
                   }
                }
                else {
                    part = _orchardServices.WorkContext.CurrentSite.As<AppDirectSettingsPart>();
                }
                return shapeHelper.EditorTemplate(TemplateName: "Parts/AppDirectSettings_Edit", Model: vm, Prefix: Prefix);
            })
          .OnGroup("AppDirect");
        }
        private void UpdateOAuth(ListAppDirectSettingVM ListOAuth) {
            foreach (var appDirectSetting in ListOAuth.ListAppDirectSetting) {
                var appSettingRecord = _repoSetting.Get(appDirectSetting.Id);
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