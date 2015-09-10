using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;

namespace Laser.Orchard.StartupConfig.Services {
    [OrchardFeature("Laser.Orchard.StartupConfig.Maintenance")]
    public class MaintenanceService : IMaintenanceService {
        private readonly IOrchardServices _orchardServices;

        public MaintenanceService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }
        public List<MaintenanceVM> Get(){
            var listofcontentitems = _orchardServices.ContentManager.Query<MaintenancePart>(VersionOptions.Published).List();
            List<MaintenanceVM> ListMaintenanceVM = new List<MaintenanceVM>();
            foreach (var y in listofcontentitems) {
                MaintenanceVM MaintenanceVM = new MaintenanceVM();
                Mapper.CreateMap<MaintenancePart, MaintenanceVM>();
                Mapper.Map(y.As<MaintenancePart>(), MaintenanceVM);
               ListMaintenanceVM.Add( MaintenanceVM);
            }
            return ListMaintenanceVM;
        }
    }
}