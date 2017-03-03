using Contrib.Widgets.Models;
using Contrib.Widgets.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Contrib.Widgets.Handlers {
    [OrchardFeature("Contrib.Widgets")]
    public class WidgetExPartHandler : ContentHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IWidgetManager _widgetManager;

        public WidgetExPartHandler(IRepository<WidgetExPartRecord> repository, IContentDefinitionManager contentDefinitionManager, IContentManager contentManager, IWidgetManager widgetManager) {
            Filters.Add(StorageFilter.For(repository));
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _widgetManager = widgetManager;
            OnActivated<WidgetExPart>(SetupFields);

            OnPublished<WidgetsContainerPart>((context, part) => {
                PublishWidgets(part);
            });
        }
        private void PublishWidgets(WidgetsContainerPart part) {
            var contentItem = part.ContentItem;

            var widgets = _widgetManager.GetWidgets(contentItem.Id, false);
            foreach (var w in widgets) {
                _contentManager.Publish(w.ContentItem);
            }
        }

        private void SetupFields(ActivatedContentContext context, WidgetExPart part) {
            part.HostField.Loader(() => part.Record.HostId != null ? _contentManager.Get(part.Record.HostId.Value) : null);
            part.HostField.Setter(x => {
                part.Record.HostId = x != null ? x.Id : default(int?);
                return x;
            });
        }

        protected override void Activated(ActivatedContentContext context) {
            if (!context.ContentItem.TypeDefinition.Settings.ContainsKey("Stereotype") || context.ContentItem.TypeDefinition.Settings["Stereotype"] != "Widget")
                return;

            if (!context.ContentItem.Is<WidgetExPart>()) {
                _contentDefinitionManager.AlterTypeDefinition(context.ContentType, type => type.WithPart("WidgetExPart"));
            }
        }
    }
}