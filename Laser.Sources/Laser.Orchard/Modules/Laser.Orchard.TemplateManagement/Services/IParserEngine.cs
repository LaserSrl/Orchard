using Laser.Orchard.TemplateManagement.Models;
using Orchard;

namespace Laser.Orchard.TemplateManagement.Services {
	public interface IParserEngine : IDependency {
		string Id { get; }
		string DisplayText { get; }
		string LayoutBeacon { get; }
		string ParseTemplate(TemplatePart template, ParseTemplateContext context);
	}

	public class ParseTemplateContext {
		public object Model { get; set; }
        public object ViewBag { get; set; }
	}
}