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
        private readonly ISiteThemeService _siteThemeService;
        private readonly IShapeTableLocator _shapeTableLocator;
        private readonly RequestContext _requestContext;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IFrontEndProfileService _frontEndProfileService;

        public ProfileFrontEndPlacementStrategy(
            Work<IContentDefinitionManager> contentDefinitionManager,
            IContentManager contentManager,
            IShapeFactory shapeFactory,
            IEnumerable<IContentPartDriver> contentPartDrivers,
            IEnumerable<IContentFieldDriver> contentFieldDrivers,
            ISiteThemeService siteThemeService,
            IShapeTableLocator shapeTableLocator,
            RequestContext requestContext,
            IVirtualPathProvider virtualPathProvider,
            IFrontEndProfileService frontEndProfileService) {

            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _shapeFactory = shapeFactory;
            _contentPartDrivers = contentPartDrivers;
            _contentFieldDrivers = contentFieldDrivers;
            _siteThemeService = siteThemeService;
            _shapeTableLocator = shapeTableLocator;
            _requestContext = requestContext;
            _virtualPathProvider = virtualPathProvider;
            _frontEndProfileService = frontEndProfileService;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void ShapeTableCreated(ShapeTable shapeTable) {

            if (!AdminFilter.IsApplied(_requestContext)) {
                var typeDefinitions = _contentDefinitionManager.Value
                .ListTypeDefinitions().
                Where(ctd => ctd.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart"));

                var allPlacements = typeDefinitions
                    .SelectMany(td => GetEditorPlacement(td.Name)
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
                            if (ctx.DisplayType == null) {
                                foreach (var customPlacement in customPlacements) {

                                    var type = customPlacement.ContentType;
                                    var differentiator = customPlacement.Placement.Differentiator;

                                    if (((ctx.Differentiator ?? String.Empty) == (differentiator ?? String.Empty)) && ctx.ContentType == type) {

                                        var location = customPlacement.Placement.Zone;
                                        if (!String.IsNullOrEmpty(customPlacement.Placement.Position)) {
                                            location = String.Concat(location, ":", customPlacement.Placement.Position);
                                        }

                                        return new PlacementInfo { Location = location };
                                    }
                                }
                            }

                            return placement(ctx);
                        };
                    }
                }
            }
        }

        private IEnumerable<PlacementSettings> GetEditorPlacement(string contentType) {
            var content = _contentManager.New(contentType);

            dynamic itemShape = CreateItemShape("Content_Edit");
            itemShape.ContentItem = content;

            var context = new BuildEditorContext(itemShape, content, String.Empty, _shapeFactory);
            BindPlacement(context, null, "Content");

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
                        //don't show
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
            string position = String.Empty;

            // if no placement is found, it's hidden, e.g., no placement was found for the specific ContentType/DisplayType
            if (!hidden && placement.Location != null) {
                var delimiterIndex = placement.Location.IndexOf(':');
                if (delimiterIndex >= 0) {
                    zone = placement.Location.Substring(0, delimiterIndex);
                    position = placement.Location.Substring(delimiterIndex + 1);
                }
            }

            //var content = _contentManager.New(typeName);

            //dynamic itemShape = CreateItemShape("Content_Edit");
            //itemShape.ContentItem = content;

            //if (context is BuildDisplayContext) {
            //    var newContext = new BuildDisplayContext(itemShape, content, "Detail", "", context.New);
            //    BindPlacement(newContext, "Detail", "Content");
            //    result.Apply(newContext);
            //} else {
            //    var newContext = new BuildEditorContext(itemShape, content, "", context.New);
            //    BindPlacement(newContext, null, "Content");
            //    result.Apply(newContext);
            //}

            return new PlacementSettings {
                ShapeType = result.GetShapeType(),
                Zone = zone,
                Position = position,
                Differentiator = result.GetDifferentiator() ?? String.Empty
            };
        }

        #region private methods from the base PlacementService
        private dynamic CreateItemShape(string actualShapeType) {
            var zoneHolding = new ZoneHolding(() => _shapeFactory.Create("ContentZone", Arguments.Empty()));
            zoneHolding.Metadata.Type = actualShapeType;
            return zoneHolding;
        }

        private void BindPlacement(BuildShapeContext context, string displayType, string stereotype) {
            context.FindPlacement = (partShapeType, differentiator, defaultLocation) => {

                var theme = _siteThemeService.GetSiteTheme();
                var shapeTable = _shapeTableLocator.Lookup(theme.Id);

                var request = _requestContext.HttpContext.Request;

                ShapeDescriptor descriptor;
                if (shapeTable.Descriptors.TryGetValue(partShapeType, out descriptor)) {
                    var placementContext = new ShapePlacementContext {
                        Content = context.ContentItem,
                        ContentType = context.ContentItem.ContentType,
                        Stereotype = stereotype,
                        DisplayType = displayType,
                        Differentiator = differentiator,
                        Path = VirtualPathUtility.AppendTrailingSlash(_virtualPathProvider.ToAppRelative(request.Path)) // get the current app-relative path, i.e. ~/my-blog/foo
                    };

                    // define which location should be used if none placement is hit
                    descriptor.DefaultPlacement = defaultLocation;

                    var placement = descriptor.Placement(placementContext);
                    if (placement != null) {
                        placement.Source = placementContext.Source;
                        return placement;
                    }
                }

                return new PlacementInfo {
                    Location = defaultLocation,
                    Source = String.Empty
                };
            };
        }
        #endregion
    }
}