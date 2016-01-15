using System;
using Orchard;
using Orchard.Environment.Extensions;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace Laser.Orchard.TemplateManagement.Parsers {
    public interface IRazorMachine : ISingletonDependency {
        string ExecuteContent(string templateContent, object model = null, object viewBag = null);
        void RegisterLayout(string virtualPath, string templateContent);
    }

    [OrchardFeature("Laser.Orchard.TemplateManagement.Parsers.Razor")]
    public class RazorMachineWrapper : IRazorMachine {
        private readonly IRazorEngineService _razorEngineService;

        public RazorMachineWrapper() {
            var config = new TemplateServiceConfiguration();
            config.Namespaces.Add("Orchard.ContentManagement");
            _razorEngineService = RazorEngineService.Create(config);
        }

        public string ExecuteContent(string templateContent, object model = null, object viewBag = null) {
            return _razorEngineService.RunCompile(templateContent, Guid.NewGuid().ToString(), null, model, (DynamicViewBag)viewBag);
        }

        public void RegisterLayout(string virtualPath, string templateContent) {
            _razorEngineService.AddTemplate(virtualPath, templateContent);
        }
    }
}