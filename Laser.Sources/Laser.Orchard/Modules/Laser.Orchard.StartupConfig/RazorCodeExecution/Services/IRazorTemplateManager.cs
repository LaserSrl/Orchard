using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System.IO;
using System.Xml.Linq;
using Orchard;

namespace Laser.Orchard.StartupConfig.RazorCodeExecution.Services {

    public interface IRazorTemplateManager : ISingletonDependency {

        string RunString(string key, string code, Object model, Dictionary<string, object> dicdvb = null);

        string RunFile(string localFilePath, RazorModelContext model);

        void AddLayout(string key, string code);

        void StartNewRazorEngine();
    }

    public class RazorTemplateManager : IRazorTemplateManager {

        public IRazorEngineService RazorEngineServiceStatic {
            get {
                if (_razorEngine == null) {
                    StartNewRazorEngine();
                }
                return _razorEngine;
            }
        }

        public void StartNewRazorEngine() {
            var config = new TemplateServiceConfiguration();
#if DEBUG
            config.Debug = true;
#endif
            config.Namespaces.Add("Orchard");
            config.Namespaces.Add("Orchard.ContentManagement");
            config.Namespaces.Add("Orchard.Caching");
            _razorEngine = RazorEngineService.Create(config);
        }

        private IRazorEngineService _razorEngine;

        public void AddLayout(string key, string code) {
            if (!RazorEngineServiceStatic.IsTemplateCached(key, null) && !string.IsNullOrEmpty(code)) {
                RazorEngineServiceStatic.AddTemplate(key, new LoadedTemplateSource(code, null));
            }
        }

        public string RunString(string key, string code, Object model, Dictionary<string, object> dicdvb = null) {
            DynamicViewBag dvb = new DynamicViewBag();
            if (dicdvb != null)
                foreach (var k in dicdvb.Keys) {
                    dvb.AddValue(k, dicdvb[k]);
                }
            if (!RazorEngineServiceStatic.IsTemplateCached(key, null)) {
                if (!string.IsNullOrEmpty(code)) {
                    RazorEngineServiceStatic.AddTemplate(key, new LoadedTemplateSource(code, null));
                    RazorEngineServiceStatic.Compile(key, null);
                }
                else
                    return null;
            }
            return RazorEngineServiceStatic.RunCompile(key, null, (Object)model, dvb);
        }

        public string RunFile(string localFilePath, RazorModelContext model) {
            if (!RazorEngineServiceStatic.IsTemplateCached(localFilePath, null)) {
                if (System.IO.File.Exists(localFilePath)) {
                    string codeTemplate = File.ReadAllText(localFilePath);
                    if (!string.IsNullOrEmpty(codeTemplate)) {
                        RazorEngineServiceStatic.AddTemplate(localFilePath, new LoadedTemplateSource(codeTemplate, localFilePath));
                        RazorEngineServiceStatic.Compile(localFilePath, null);
                    }
                    else
                        return null;
                }
                else
                    return null;
            }
            string result = RazorEngineServiceStatic.RunCompile(localFilePath, null, (Object)model);
            string resultnobr = (result ?? "").Replace("\r\n", "").Replace(" ", "");
            if (!string.IsNullOrEmpty(resultnobr))
                return result;
            else
                return null;
        }
    }
}