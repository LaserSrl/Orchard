//using Orchard;
//using Orchard.ContentManagement;
//using Orchard.ContentManagement.Drivers;
//using Orchard.ContentManagement.Handlers;
//using Orchard.ContentManagement.MetaData;
//using Orchard.ContentManagement.MetaData.Models;
//using Orchard.ContentTypes.Services;
//using Orchard.ContentTypes.Settings;
//using Orchard.DisplayManagement;
//using Orchard.DisplayManagement.Descriptors;
//using Orchard.Environment.Extensions;
//using Orchard.FileSystems.VirtualPath;
//using Orchard.Themes.Services;
//using Orchard.UI.Zones;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Routing;

//namespace Contrib.Profile.Services {
//    public class ProfilePlacementService : PlacementService, IPlacementService {
//        //we extend on PlacementService rather than just implementing the interface so
//        //we can call on base methods in case we don't have to process the stuff that has
//        //to do with a ProfilePart. Alternatively, we could have rewritten the whole thing.
//        //This way, in case the default behaviour changes, we stay aligned to it for any
//        //ContentType that doesn't have a ProfilePart

//        private readonly IContentManager _contentManager;
//        private readonly ISiteThemeService _siteThemeService;
//        private readonly IExtensionManager _extensionManager;
//        private readonly IShapeFactory _shapeFactory;
//        private readonly IShapeTableLocator _shapeTableLocator;
//        private readonly RequestContext _requestContext;
//        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;
//        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
//        private readonly IVirtualPathProvider _virtualPathProvider;
//        private readonly IWorkContextAccessor _workContextAccessor;
//        private readonly IContentDefinitionManager _contentDefinitionManager;
//        private readonly IFrontEndProfileService _frontEndProfileService;

//        public ProfilePlacementService(
//            IContentManager contentManager,
//            ISiteThemeService siteThemeService,
//            IExtensionManager extensionManager,
//            IShapeFactory shapeFactory,
//            IShapeTableLocator shapeTableLocator,
//            RequestContext requestContext,
//            IEnumerable<IContentPartDriver> contentPartDrivers,
//            IEnumerable<IContentFieldDriver> contentFieldDrivers,
//            IVirtualPathProvider virtualPathProvider,
//            IWorkContextAccessor workContextAccessor,
//            IContentDefinitionManager contentDefinitionManager,
//            IFrontEndProfileService frontEndProfileService
//            )
//            : base(contentManager, siteThemeService, extensionManager, shapeFactory,
//                 shapeTableLocator, requestContext, contentPartDrivers, contentFieldDrivers,
//                 virtualPathProvider, workContextAccessor) {
//            _contentManager = contentManager;
//            _siteThemeService = siteThemeService;
//            _extensionManager = extensionManager;
//            _shapeFactory = shapeFactory;
//            _shapeTableLocator = shapeTableLocator;
//            _requestContext = requestContext;
//            _contentPartDrivers = contentPartDrivers;
//            _contentFieldDrivers = contentFieldDrivers;
//            _virtualPathProvider = virtualPathProvider;
//            _workContextAccessor = workContextAccessor;
//            _contentDefinitionManager = contentDefinitionManager;
//            _frontEndProfileService = frontEndProfileService;
//        }

//        public new IEnumerable<DriverResultPlacement> GetDisplayPlacement(string contentType) {
//            return base.GetDisplayPlacement(contentType);
//            //TODO: do the override base on the settings for front end display
//        }

//        public new IEnumerable<DriverResultPlacement> GetEditorPlacement(string contentType) {
//            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
//            if (typeDefinition.Parts.Any(pa => pa.PartDefinition.Name == "ProfilePart")) {
//                return GetEditorPlacement(contentType, typeDefinition);
//            }
//            return base.GetEditorPlacement(contentType);
//        }

//        private IEnumerable<DriverResultPlacement> GetEditorPlacement(string contentType, ContentTypeDefinition typeDefinition) {
//            var content = _contentManager.New(contentType);

//            dynamic itemShape = CreateItemShape("Content_Edit");
//            itemShape.ContentItem = content;

//            var context = new BuildEditorContext(itemShape, content, String.Empty, _shapeFactory);
//            BindPlacement(context, null, "Content");

//            var placementSettings = new List<DriverResultPlacement>();

//            _contentPartDrivers.Invoke(driver => {
//                var result = driver.BuildEditor(context);
//                if (result != null) {
//                    placementSettings.AddRange(ProcessEditDriverResult(result, context, contentType));
//                }
//            }, Logger);

//            _contentFieldDrivers.Invoke(driver => {
//                var result = driver.BuildEditorShape(context);
//                if (result != null) {
//                    placementSettings.AddRange(ProcessEditDriverResult(result, context, contentType));
//                }
//            }, Logger);

//            foreach (var placementSetting in placementSettings) {
//                yield return placementSetting;
//            }
//        }

//        private IEnumerable<DriverResultPlacement> ProcessEditDriverResult(DriverResult result, BuildShapeContext context, string typeName) {
//            if (result is CombinedResult) {
//                foreach (var subResult in ((CombinedResult)result).GetResults()) {
//                    foreach (var placement in ProcessEditDriverResult(subResult, context, typeName)) {
//                        yield return placement;
//                    }
//                }
//            } else if (result is ContentShapeResult) {
//                var part = result.ContentPart;
//                if (part != null) { //sanity check: should always be true
//                    var typePartDefinition = part.TypePartDefinition;
//                    bool hidePlacement = false;
//                    if (_frontEndProfileService.MayAllowPartEdit(typePartDefinition, typeName)) {
//                        var field = result.ContentField;
//                        if (field != null) { //we run a driver for a ContentField rather than a ContentPart
//                            hidePlacement = !_frontEndProfileService.MayAllowFieldEdit(field.PartFieldDefinition);
//                        }
//                    } else {
//                        //don't show
//                        hidePlacement = true;
//                    }
//                    yield return GetPlacement((ContentShapeResult)result, context, typeName, hidePlacement);
//                }
//            }
//        }

//        private DriverResultPlacement GetPlacement(ContentShapeResult result, BuildShapeContext context, string typeName, bool hidden = false) {
//            var placement = context.FindPlacement(
//                result.GetShapeType(),
//                result.GetDifferentiator(),
//                result.GetLocation()
//                );

//            string zone = hidden ? "-" : placement.Location;
//            string position = String.Empty;

//            // if no placement is found, it's hidden, e.g., no placement was found for the specific ContentType/DisplayType
//            if (!hidden && placement.Location != null) {
//                var delimiterIndex = placement.Location.IndexOf(':');
//                if (delimiterIndex >= 0) {
//                    zone = placement.Location.Substring(0, delimiterIndex);
//                    position = placement.Location.Substring(delimiterIndex + 1);
//                }
//            }

//            var content = _contentManager.New(typeName);

//            dynamic itemShape = CreateItemShape("Content_Edit");
//            itemShape.ContentItem = content;

//            if (context is BuildDisplayContext) {
//                var newContext = new BuildDisplayContext(itemShape, content, "Detail", "", context.New);
//                BindPlacement(newContext, "Detail", "Content");
//                result.Apply(newContext);
//            } else {
//                var newContext = new BuildEditorContext(itemShape, content, "", context.New);
//                BindPlacement(newContext, null, "Content");
//                result.Apply(newContext);
//            }

//            return new DriverResultPlacement {
//                Shape = itemShape.Content,
//                ShapeResult = result,
//                PlacementSettings = new PlacementSettings {
//                    ShapeType = result.GetShapeType(),
//                    Zone = zone,
//                    Position = position,
//                    Differentiator = result.GetDifferentiator() ?? String.Empty
//                }
//            };
//        }


//        #region private methods from the base PlacementService
//        private dynamic CreateItemShape(string actualShapeType) {
//            var zoneHolding = new ZoneHolding(() => _shapeFactory.Create("ContentZone", Arguments.Empty()));
//            zoneHolding.Metadata.Type = actualShapeType;
//            return zoneHolding;
//        }

//        private void BindPlacement(BuildShapeContext context, string displayType, string stereotype) {
//            context.FindPlacement = (partShapeType, differentiator, defaultLocation) => {

//                var theme = _siteThemeService.GetSiteTheme();
//                var shapeTable = _shapeTableLocator.Lookup(theme.Id);

//                var request = _requestContext.HttpContext.Request;

//                ShapeDescriptor descriptor;
//                if (shapeTable.Descriptors.TryGetValue(partShapeType, out descriptor)) {
//                    var placementContext = new ShapePlacementContext {
//                        Content = context.ContentItem,
//                        ContentType = context.ContentItem.ContentType,
//                        Stereotype = stereotype,
//                        DisplayType = displayType,
//                        Differentiator = differentiator,
//                        Path = VirtualPathUtility.AppendTrailingSlash(_virtualPathProvider.ToAppRelative(request.Path)) // get the current app-relative path, i.e. ~/my-blog/foo
//                    };

//                    // define which location should be used if none placement is hit
//                    descriptor.DefaultPlacement = defaultLocation;

//                    var placement = descriptor.Placement(placementContext);
//                    if (placement != null) {
//                        placement.Source = placementContext.Source;
//                        return placement;
//                    }
//                }

//                return new PlacementInfo {
//                    Location = defaultLocation,
//                    Source = String.Empty
//                };
//            };
//        }
//        #endregion
//    }
//}