using Contrib.Profile.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.ContentTypes.Extensions;
using Orchard.ContentTypes.Settings;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.UI.Zones;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Contrib.Profile.Settings {
    public class ProfileFrontEndEditorEvents : ContentDefinitionEditorEventsBase {
        //The settings are attached to all parts and fields
        //They are attached only if there is a ProfilePart
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly Lazy<IEnumerable<IShellSettingsManagerEventHandler>> _settingsManagerEventHandlers;
        private readonly ShellSettings _shellSettings;
        private readonly IContentManager _contentManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IFrontEndProfileService _frontEndProfileService;

        //This event hendler is instanced once per transaction, so I can use a bool to save checks
        private bool _typeHasProfilePart { get; set; }

        public ProfileFrontEndEditorEvents(
            IContentDefinitionManager contentDefinitionManager,
            Lazy<IEnumerable<IShellSettingsManagerEventHandler>> settingsManagerEventHandlers,
            ShellSettings shellSettings,
            IContentManager contentManager,
            IShapeFactory shapeFactory,
            IEnumerable<IContentPartDriver> contentPartDrivers,
            IEnumerable<IContentFieldDriver> contentFieldDrivers,
            IFrontEndProfileService frontEndProfileService) {

            _contentDefinitionManager = contentDefinitionManager;
            _settingsManagerEventHandlers = settingsManagerEventHandlers;
            _contentManager = contentManager;
            _shapeFactory = shapeFactory;
            _contentPartDrivers = contentPartDrivers;
            _contentFieldDrivers = contentFieldDrivers;
            _frontEndProfileService = frontEndProfileService;

            _shellSettings = shellSettings;
        }

        public ILogger Logger { get; set; }

        #region ProfileFrontEndSettings for Fields
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (_typeHasProfilePart) {
                var settings = definition.Settings.GetModel<ProfileFrontEndSettings>();
                yield return DefinitionTemplate(settings);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            //var typeDefinition = _contentDefinitionManager.GetTypeDefinition(builder.PartName);
            if (_typeHasProfilePart) {
                //_typeHasProfilePart = true;
                var settings = new ProfileFrontEndSettings();
                if (updateModel.TryUpdateModel(settings, "ProfileFrontEndSettings", null, null)) {
                    ProfileFrontEndSettings.SetValues(builder, settings.AllowFrontEndDisplay, settings.AllowFrontEndEdit);
                }
                yield return DefinitionTemplate(settings);
            }
        }
        #endregion

        #region ProfileFrontEndSettings for Parts
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (_typeHasProfilePart ||
                definition.ContentTypeDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart")) {
                _typeHasProfilePart = true;
                var settings = definition.Settings.GetModel<ProfileFrontEndSettings>();
                yield return DefinitionTemplate(settings);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(builder.TypeName);
            if (_typeHasProfilePart ||
                typeDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart")) {
                _typeHasProfilePart = true;
                var settings = new ProfileFrontEndSettings();
                if (updateModel.TryUpdateModel(settings, "ProfileFrontEndSettings", null, null)) {
                    ProfileFrontEndSettings.SetValues(builder, settings.AllowFrontEndDisplay, settings.AllowFrontEndEdit);
                }
                yield return DefinitionTemplate(settings);
            }
        }
        #endregion


        public override void TypeEditorUpdated(ContentTypeDefinitionBuilder builder) {
            // This is called after everything else has been done for a type
            // At this stage, check whether this is a type with a ProfilePart
            var typeDefinition = builder.Current;
            if (typeDefinition.Parts.Any(cptd => cptd.PartDefinition.Name == "ProfilePart")) {
                // In this case we want to save in a setting for the type all the configuration regarding
                // front-end display/edit for the different fields and parts. This is similar to what
                // is done in Orchard.ContentTypes for the "Edit Placement" functionality
                var placements = GetEditorPlacement(typeDefinition).ToList();
                // write the placement settings as a setting for the type
                foreach (var placement in placements) {
                    builder.WithSetting("ContentTypeSettings.Placement.ProfileFrontEndEditor", placement);
                }

                // persist changes: The type definition is persisted already after this events are processed
                //_contentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);
                // schedules a re-evaluation of the shell
                _settingsManagerEventHandlers.Value.Invoke(x => x.Saved(_shellSettings), Logger);
            }
            base.TypeEditorUpdated(builder);
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
    }
}