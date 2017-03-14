using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Orchard.Workflows.Models;
using RazorEngine;

namespace Laser.Orchard.StartupConfig.RazorCodeExecution.Services {

    public class RazorModelContext {
        public IOrchardServices OrchardServices { get; set; }
        public IContent ContentItem { get; set; }
        public IDictionary<string, object> Tokens { get; set; }
        public Localizer T { get; set; }
    }

    public interface IRazorExecuteService : IDependency {

        string Execute(string codeFileWithExtension, IContent contentItem, IDictionary<string, object> tokens = null);

        string ExecuteString(string code, IContent contentItem, IDictionary<string, object> tokens = null);
    }

    public class RazorExecuteService : IRazorExecuteService {
        private readonly ShellSettings _shellSettings;
        private readonly IOrchardServices _orchardServices;
        private readonly IRazorTemplateManager _razorTemplateManager;

        public Localizer T { get; set; }

        public RazorExecuteService(ShellSettings shellSettings, IOrchardServices orchardServices, IRazorTemplateManager razorTemplateManager) {
            _shellSettings = shellSettings;
            _orchardServices = orchardServices;
            _razorTemplateManager = razorTemplateManager;
            T = NullLocalizer.Instance;
        }

        public string Execute(string codeFileWithExtension, IContent content, IDictionary<string, object> tokens = null) {
            var uriDir = String.Format("~/App_Data/Sites/{0}/Code/", _shellSettings.Name);
            var uriFile = String.Format("{0}/{1}", uriDir, codeFileWithExtension);
            var localDir = HostingEnvironment.MapPath(uriDir);
            if (!System.IO.Directory.Exists(localDir))
                System.IO.Directory.CreateDirectory(localDir);
            var localFile = HostingEnvironment.MapPath(uriFile);
            var model = new RazorModelContext {
                OrchardServices = _orchardServices,
                ContentItem = content,
                Tokens = tokens ?? new Dictionary<string, object>(),
                T = T
            };
            //           RazorTemplateManager rt = new RazorTemplateManager();
            return _razorTemplateManager.RunFile(localFile, model);
        }

        public string ExecuteString(string codeTemplate, IContent content, IDictionary<string, object> tokens = null) {
            if (!string.IsNullOrEmpty(codeTemplate)) {
                var config = new TemplateServiceConfiguration();
#if DEBUG
                config.Debug = true;
#endif
                string result = "";
                using (var service = RazorEngineService.Create(config)) {
                    var model = new RazorModelContext {
                        OrchardServices = _orchardServices,
                        ContentItem = content,
                        Tokens = tokens ?? new Dictionary<string, object>(),
                        T = T
                    };
                    result = service.RunCompile(new LoadedTemplateSource(codeTemplate, null), "htmlRawTemplatea", null, (Object)model);
                }
                string resultnobr = result.Replace("\r\n", "").Replace(" ", "");
                if (!string.IsNullOrEmpty(resultnobr)) {
                    return result;
                }
            }
            return null;
        }
    }
}