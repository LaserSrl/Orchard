using System.Collections.Generic;
using System.Linq;
using Laser.Orchard.TemplateManagement.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;

namespace Laser.Orchard.TemplateManagement.Services {
    public interface ITemplateService : IDependency {
        IEnumerable<TemplatePart> GetLayouts();
        IEnumerable<TemplatePart> GetTemplates();
        IEnumerable<TemplatePart> GetTemplatesWithLayout(int LayoutIdSelected);
        TemplatePart GetTemplate(int id);
        string ParseTemplate(TemplatePart template, ParseTemplateContext context);
        IEnumerable<IParserEngine> GetParsers();
        IParserEngine GetParser(string id);
        IParserEngine SelectParser(TemplatePart template);
    }

    [OrchardFeature("Laser.Orchard.TemplateManagement")]
    public class TemplateService : Component, ITemplateService {
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<IParserEngine> _parsers;
        private readonly IOrchardServices _services;

        public TemplateService(IEnumerable<IParserEngine> parsers, IOrchardServices services) {
            _contentManager = services.ContentManager;
            _parsers = parsers;
            _services = services;
        }

        public IEnumerable<TemplatePart> GetLayouts() {
            return _contentManager.Query<TemplatePart, TemplatePartRecord>().Where(x => x.IsLayout).List();
        }

        public IEnumerable<TemplatePart> GetTemplates() {
            return _contentManager.Query<TemplatePart, TemplatePartRecord>().Where(x => !x.IsLayout).List();
        }

        public IEnumerable<TemplatePart> GetTemplatesWithLayout(int LayoutIdSelected) {
            return _contentManager.Query<TemplatePart, TemplatePartRecord>().Where(x => x.LayoutIdSelected == LayoutIdSelected).List();
        }

        public TemplatePart GetTemplate(int id) {
            return _contentManager.Get<TemplatePart>(id);
        }

        public string ParseTemplate(TemplatePart template, ParseTemplateContext context) {
            var parser = SelectParser(template);
            return parser.ParseTemplate(template, context);
        }

        public IParserEngine GetParser(string id) {
            return _parsers.SingleOrDefault(x => x.Id == id);
        }

        public IParserEngine SelectParser(TemplatePart template) {
            var parserId = template.DefaultParserIdSelected;
            IParserEngine parser = null;

            if (!string.IsNullOrWhiteSpace(parserId)) {
                parser = GetParser(parserId);
            }

            if (parser == null) {
                parserId = _services.WorkContext.CurrentSite.As<SiteSettingsPart>().DefaultParserIdSelected;
                parser = GetParser(parserId);
            }

            return parser ?? _parsers.First();
        }

        public IEnumerable<IParserEngine> GetParsers() {
            return _parsers;
        }
    }
}