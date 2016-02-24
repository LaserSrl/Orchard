using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Laser.Orchard.TemplateManagement.Models;
using Laser.Orchard.TemplateManagement.Services;
using Orchard.Environment.Extensions;
using Orchard.Logging;

namespace Laser.Orchard.TemplateManagement.Parsers {
    [OrchardFeature("Laser.Orchard.TemplateManagement.Parsers.Razor")]
    public class RazorParserEngine : ParserEngineBase {
        private readonly IRazorMachine _razorMachine;

        public RazorParserEngine(IRazorMachine razorMachine) {
            _razorMachine = razorMachine;
        }

        public override string DisplayText {
            get { return "Razor Engine"; }
        }

        public override string LayoutBeacon {
            get { return "@RenderBody()"; }
        }

        public override string ParseTemplate(TemplatePart template, ParseTemplateContext context) {
            var layout = template.Layout;
            var templateContent = template.Text;
            //var viewBag = context.ViewBag;

            if (layout != null) {
                var layoutGuid = Guid.NewGuid();
                _razorMachine.RegisterLayout("~/shared/_" + layoutGuid + ".cshtml", layout.Text);
                templateContent = "@{ Layout = \"~/shared/_" + layoutGuid + ".cshtml\"; }\r\n" + templateContent;
            }

            try {
                // Convert viewBag to string/object pairs if required
                //if (viewBag != null) {
                //    if (viewBag is IEnumerable<KeyValuePair<string, string>>)
                //        viewBag = ((IEnumerable<KeyValuePair<string, string>>) viewBag).Select(x => new KeyValuePair<string, object>(x.Key, x.Value)).ToDictionary(x => x.Key, x => x.Value);
                //}
                
                var tmpl = _razorMachine.ExecuteContent(templateContent, context.Model, context.ViewBag);
                return tmpl;
            } catch (Exception ex) {
                Logger.Log(LogLevel.Error, ex, "Failed to parse the {0} Razor template with layout {1}", template.Title, layout != null ? layout.Title : "[none]");
                return BuildErrorContent(ex, template, layout);
            }
        }

        private static string BuildErrorContent(Exception ex, TemplatePart templatePart, TemplatePart layout) {
            var sb = new StringBuilder();
            var currentException = ex;
            sb.AppendLine("Error On Template");
            while (currentException != null) {
                sb.AppendLine(currentException.Message);
                currentException = currentException.InnerException;
            }

            sb.AppendFormat("\r\nTemplate ({0}):\r\n", templatePart.Title);
            sb.AppendLine(templatePart.Text);

            if (layout != null) {
                sb.AppendFormat("\r\nLayout ({0}):\r\n", layout.Title);
                sb.AppendLine(layout.Text);
            }
            return sb.ToString();
        }
    }
}