using Contrib.Widgets.Services;
using Laser.Orchard.Events.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Projections.Services;
using Orchard.Security;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;

namespace Laser.Orchard.WebServices.Services {
    public interface IWebApiService : IDependency {
        ActionResult Terms(string alias, int maxLevel = 10);
        ActionResult Display(string alias, int page = 1, int pageSize = 10, int maxLevel = 10);
    }

}