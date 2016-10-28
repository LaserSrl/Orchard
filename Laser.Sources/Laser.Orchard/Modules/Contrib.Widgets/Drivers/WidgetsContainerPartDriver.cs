using System;
using System.Linq;
using System.Xml;
using Contrib.Widgets.Models;
using Contrib.Widgets.Services;
using Contrib.Widgets.ViewModels;
using Newtonsoft.Json;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.VirtualPath;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Themes.Services;
using Orchard.Widgets.Services;


namespace Contrib.Widgets.Drivers {
    [OrchardFeature("Contrib.Widgets")]
    public class WidgetsContainerPartDriver : ContentPartDriver<WidgetsContainerPart> {
        private readonly ISiteThemeService _siteThemeService;
        private readonly IWidgetsService _widgetsService;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IWidgetManager _widgetManager;
        private readonly IWorkContextAccessor _wca;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _services;
        private readonly ILocalizationService _localizationService;
        private readonly ICultureManager _cultureManager;


        public WidgetsContainerPartDriver(ISiteThemeService siteThemeService, IWidgetsService widgetsService, IVirtualPathProvider virtualPathProvider, IShapeFactory shapeFactory, IWidgetManager widgetManager, IWorkContextAccessor wca, IContentManager contentManager, IOrchardServices services, ILocalizationService localizationService, ICultureManager cultureManager) {
            _siteThemeService = siteThemeService;
            _widgetsService = widgetsService;
            _virtualPathProvider = virtualPathProvider;
            New = shapeFactory;
            _widgetManager = widgetManager;
            _wca = wca;
            _contentManager = contentManager;
            _services = services;
            _localizationService = localizationService;
            _cultureManager = cultureManager;
        }

        private dynamic New { get; set; }

        protected override string Prefix {
            get { return "WidgetsContainer"; }
        }

        protected override DriverResult Display(WidgetsContainerPart part, string displayType, dynamic shapeHelper) {
            // TODO: make DisplayType configurable
            if (displayType != "Detail")
                return null;

            var widgetParts = _widgetManager.GetWidgets(part.Id);

            // Build and add shape to zone.
            var workContext = _wca.GetContext();
            var zones = workContext.Layout.Zones;
            foreach (var widgetPart in widgetParts) {
                var widgetShape = _contentManager.BuildDisplay(widgetPart);
                zones[widgetPart.Zone].Add(widgetShape, widgetPart.Position);
            }

            return null;
        }

        protected override DriverResult Editor(WidgetsContainerPart part, dynamic shapeHelper) {
            return ContentShape("Parts_WidgetsContainer", () => {
                var currentTheme = _siteThemeService.GetSiteTheme();
                var currentThemesZones = _widgetsService.GetZones(currentTheme).ToList();
                var widgetTypes = _widgetsService.GetWidgetTypeNames().ToList();
                var widgets = _widgetManager.GetWidgets(part.Id);
                var zonePreviewImagePath = string.Format("{0}/{1}/ThemeZonePreview.png", currentTheme.Location, currentTheme.Id);
                var zonePreviewImage = _virtualPathProvider.FileExists(zonePreviewImagePath) ? zonePreviewImagePath : null;
                var layer = _widgetsService.GetLayers().First();

                // recupero i contenuti localizzati una try è necessaria in quanto non è detto che un contenuto sia localizzato
                dynamic contentLocalizations;
                try {
                    contentLocalizations = _localizationService.GetLocalizations(part.ContentItem, VersionOptions.Latest)
                        .Select(c => {
                            var localized = c.ContentItem.As<LocalizationPart>();
                            if (localized.Culture == null)
                                localized.Culture = _cultureManager.GetCultureByName(_cultureManager.GetSiteCulture());
                            return c;
                        })
                        .OrderBy(o => o.Culture.Culture)
                        .ToList();
                } catch {
                    contentLocalizations = null;
                }

                var viewModel = New.ViewModel()
                    .CurrentTheme(currentTheme)
                    .Zones(currentThemesZones)
                    .ContentLocalizations(contentLocalizations)
                    .ZonePreviewImage(zonePreviewImage)
                    .WidgetTypes(widgetTypes)
                    .Widgets(widgets)
                    .ContentItem(part.ContentItem)
                    .LayerId(layer.Id)
                    .CloneFrom(0);

                return shapeHelper.EditorTemplate(TemplateName: "Parts.WidgetsContainer", Model: viewModel, Prefix: Prefix);
            });
        }

        protected override DriverResult Editor(WidgetsContainerPart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new WidgetsContainerViewModel();
            if (updater.TryUpdateModel(viewModel, null, null, null)) {
                UpdatePositions(viewModel);
                RemoveWidgets(viewModel);
                CloneWidgets(viewModel, part.ContentItem);
            }

            return Editor(part, shapeHelper);
        }

        private void RemoveWidgets(WidgetsContainerViewModel viewModel) {
            if (string.IsNullOrEmpty(viewModel.RemovedWidgets))
                return;

            var widgetIds = JsonConvert.DeserializeObject<int[]>(viewModel.RemovedWidgets);

            foreach (var widgetId in widgetIds) {
                _widgetsService.DeleteWidget(widgetId);
            }
        }

        private void UpdatePositions(WidgetsContainerViewModel viewModel) {
            if (string.IsNullOrEmpty(viewModel.WidgetPlacement))
                return;

            var data = JsonConvert.DeserializeXNode(viewModel.WidgetPlacement);
            var zonesNode = data.Root;

            foreach (var zoneNode in zonesNode.Elements()) {
                var zoneName = zoneNode.Name.ToString();
                var widgetElements = zoneNode.Elements("widgets");
                var position = 0;

                foreach (var widget in widgetElements
                    .Select(widgetNode => XmlConvert.ToInt32(widgetNode.Value))
                    .Select(widgetId => _widgetsService.GetWidget(widgetId))
                    .Where(widget => widget != null)) {

                    widget.Position = (position++).ToString();
                    widget.Zone = zoneName;
                }
            }
        }



        protected override void Imported(WidgetsContainerPart part, ImportContentContext context) {
            var hostId = context.Attribute(part.PartDefinition.Name, "HostId");
            if (hostId != null) {
                CloneWidgets(Convert.ToInt32(hostId), part.ContentItem);
            }
        }

        protected override void Exporting(WidgetsContainerPart part, ExportContentContext context) {
            // memorizzo l'id della pagina sorgente
            context.Element(part.PartDefinition.Name).SetAttributeValue("HostId", part.Id.ToString());
        }

        #region [ Clone Functionality ]

        private void CloneWidgets(WidgetsContainerViewModel viewModel, ContentItem hostContentItem) {
            CloneWidgets(viewModel.CloneFrom, hostContentItem);
        }

        private void CloneWidgets(int originalContentId, ContentItem destinationContentItem) {
            if (originalContentId <= 0) return;

            // recupero i WidgetExPart del Content selezionato come Master (CloneFrom)
            var widgets = _widgetManager.GetWidgets(originalContentId);
            foreach (var widget in widgets) {

                // recupero il content widget nel ContentItem Master
                //var widgetPart = _widgetsService.GetWidget(widget.Id);

                // Clono il ContentMaster e recupero la parte WidgetExPart
                var clonedContentitem = _services.ContentManager.Clone(widget.ContentItem);

                var widgetExPart = clonedContentitem.As<WidgetExPart>();

                // assegno il nuovo contenitore se non nullo ( nel caso di HtmlWidget per esempio la GetWidget ritorna nullo...)
                if (widgetExPart != null) {
                    widgetExPart.Host = destinationContentItem;
                    _services.ContentManager.Publish(widgetExPart.ContentItem);
                }

            }

        }
        #endregion

    }
}