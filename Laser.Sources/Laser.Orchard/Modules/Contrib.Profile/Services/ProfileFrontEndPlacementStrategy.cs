using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Extensions;
using Orchard.ContentTypes.Services;
using Orchard.ContentTypes.Settings;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;
using Orchard.Themes.Services;
using Orchard.UI.Admin;
using Orchard.UI.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Contrib.Profile.Services {
    public class ProfileFrontEndPlacementStrategy : IShapeTableEventHandler {

        private readonly Work<IContentDefinitionManager> _contentDefinitionManager;
        private readonly IContentManager _contentManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IFrontEndProfileService _frontEndProfileService;
        private readonly IWorkContextAccessor _workContextAccessor;

        public ProfileFrontEndPlacementStrategy(
            Work<IContentDefinitionManager> contentDefinitionManager,
            IContentManager contentManager,
            IShapeFactory shapeFactory,
            IEnumerable<IContentPartDriver> contentPartDrivers,
            IEnumerable<IContentFieldDriver> contentFieldDrivers,
            IFrontEndProfileService frontEndProfileService,
            IWorkContextAccessor workContextAccessor) {

            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _shapeFactory = shapeFactory;
            _contentPartDrivers = contentPartDrivers;
            _contentFieldDrivers = contentFieldDrivers;
            _frontEndProfileService = frontEndProfileService;
            _workContextAccessor = workContextAccessor;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void ShapeTableCreated(ShapeTable shapeTable) {

            var typeDefinitions = _contentDefinitionManager.Value
                .ListTypeDefinitions().
                Where(ctd => ctd.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart"));

            var allPlacements = typeDefinitions
                .SelectMany(td => GetEditorPlacement(td)
                    .Select(p => new TypePlacement { Placement = p, ContentType = td.Name }));

            // group all placement settings by shape type
            var shapePlacements = allPlacements
                .GroupBy(x => x.Placement.ShapeType)
                .ToDictionary(x => x.Key, y => y.ToList());

            foreach (var shapeType in shapeTable.Descriptors.Keys) {
                List<TypePlacement> customPlacements;
                if (shapePlacements.TryGetValue(shapeType, out customPlacements)) {
                    if (!customPlacements.Any()) {
                        continue;
                    }
                    // there are some custom placements, build a predicate
                    var descriptor = shapeTable.Descriptors[shapeType];
                    var placement = descriptor.Placement;
                    descriptor.Placement = ctx => {
                        var WorkContext = _workContextAccessor.GetContext(); //I need the context for the call using the predicates
                        if (ctx.DisplayType == null &&
                            !AdminFilter.IsApplied(WorkContext.HttpContext.Request.RequestContext)) {

                            foreach (var customPlacement in customPlacements) {
                                var type = customPlacement.ContentType;
                                var differentiator = customPlacement.Placement.Differentiator;

                                if (((ctx.Differentiator ?? string.Empty) == (differentiator ?? string.Empty)) && ctx.ContentType == type) {

                                    var location = customPlacement.Placement.Zone;
                                    if (!string.IsNullOrEmpty(customPlacement.Placement.Position)) {
                                        location = string.Concat(location, ":", customPlacement.Placement.Position);
                                    }

                                    return new PlacementInfo { Location = location };
                                }
                            }
                        }
                        //fallback
                        return placement(ctx);
                    };
                }
            }

        }

        /// <summary>
        /// We build a dummy content item in order to execute the drivers for all ContentParts and ContentFields.
        /// This way we can process the resulting shapes one by one and set the placement to "-" (don't place) for
        /// those we don't want to show on front end editors. We need this to prevent their UpdateEditor methods to
        /// be executed.
        /// </summary>
        /// <param name="definition">The definition of the ContentType we are working on.</param>
        /// <returns>The PlacementSetting objects for all ContentParts and ContentFields in the type.</returns>
        private IEnumerable<PlacementSettings> GetEditorPlacement(ContentTypeDefinition definition) {
            var contentType = definition.Name;
            var content = _contentManager.New(contentType); //our dummy content

            dynamic itemShape = CreateItemShape("Content_Edit");
            itemShape.ContentItem = content;

            var context = new BuildEditorContext(itemShape, content, string.Empty, _shapeFactory);
            //get the default placements: if we don't provide these ourselves, placement for shapes will default
            //to null, preventing them to be displayed at all times.
            var defaultPlacements = definition.GetPlacement(PlacementType.Editor);
            BindPlacement(context, null, "Content", defaultPlacements);

            var placementSettings = new List<PlacementSettings>();

            _contentPartDrivers.Invoke(driver => {
                var result = driver.BuildEditor(context);
                if (result != null) {
                    placementSettings.AddRange(ProcessEditDriverResult(result, context, contentType));
                }
            }, Logger);

            _contentFieldDrivers.Invoke(driver => {
                var result = driver.BuildEditorShape(context);
                if (result != null) {
                    placementSettings.AddRange(ProcessEditDriverResult(result, context, contentType));
                }
            }, Logger);

            foreach (var placementSetting in placementSettings) {
                yield return placementSetting;
            }
        }

        private IEnumerable<PlacementSettings> ProcessEditDriverResult(DriverResult result, BuildShapeContext context, string typeName) {
            if (result is CombinedResult) {
                foreach (var subResult in ((CombinedResult)result).GetResults()) {
                    foreach (var placement in ProcessEditDriverResult(subResult, context, typeName)) {
                        yield return placement;
                    }
                }
            } else if (result is ContentShapeResult) {
                var part = result.ContentPart;
                if (part != null) { //sanity check: should always be true
                    var typePartDefinition = part.TypePartDefinition;
                    bool hidePlacement = false;
                    if (_frontEndProfileService.MayAllowPartEdit(typePartDefinition, typeName)) {
                        var field = result.ContentField;
                        if (field != null) { //we run a driver for a ContentField rather than a ContentPart
                            hidePlacement = !_frontEndProfileService.MayAllowFieldEdit(field.PartFieldDefinition);
                        }
                    } else {
                        //don't show anything of this part
                        hidePlacement = true;
                    }
                    yield return GetPlacement((ContentShapeResult)result, context, typeName, hidePlacement);
                }
            }
        }

        private PlacementSettings GetPlacement(ContentShapeResult result, BuildShapeContext context, string typeName, bool hidden = false) {
            var placement = context.FindPlacement(
                result.GetShapeType(),
                result.GetDifferentiator(),
                result.GetLocation()
                );

            string zone = hidden ? "-" : placement.Location;
            string position = string.Empty;

            // if no placement is found, it's hidden, e.g., no placement was found for the specific ContentType/DisplayType
            if (!hidden && placement.Location != null) {
                var delimiterIndex = placement.Location.IndexOf(':');
                if (delimiterIndex >= 0) {
                    zone = placement.Location.Substring(0, delimiterIndex);
                    position = placement.Location.Substring(delimiterIndex + 1);
                }
            }

            return new PlacementSettings {
                ShapeType = result.GetShapeType(),
                Zone = zone,
                Position = position,
                Differentiator = result.GetDifferentiator() ?? string.Empty
            };
        }

        #region private methods from the base PlacementService
        private dynamic CreateItemShape(string actualShapeType) {
            var zoneHolding = new ZoneHolding(() => _shapeFactory.Create("ContentZone", Arguments.Empty()));
            zoneHolding.Metadata.Type = actualShapeType;
            return zoneHolding;
        }

        private void BindPlacement(
            BuildShapeContext context, string displayType,
            string stereotype, IEnumerable<PlacementSettings> defaultSettings) {

            context.FindPlacement = (partShapeType, differentiator, defaultLocation) => {
                var mockSetting = new PlacementSettings {
                    ShapeType = partShapeType,
                    Differentiator = differentiator
                };
                var defaultSetting = defaultSettings.FirstOrDefault(ps => ps.IsSameAs(mockSetting));
                defaultLocation = defaultSetting == null ? defaultLocation : //may still end up with a null defaultLocation
                    defaultSetting.Zone + (string.IsNullOrEmpty(defaultSetting.Position) ? "" : ":" + defaultSetting.Position);
                defaultLocation = string.IsNullOrWhiteSpace(defaultLocation) ? "Content:1" : defaultLocation; //avoid null fallbacks
                return new PlacementInfo {
                    Location = defaultLocation,
                    Source = string.Empty
                };
            };
        }
        #endregion
    }
}