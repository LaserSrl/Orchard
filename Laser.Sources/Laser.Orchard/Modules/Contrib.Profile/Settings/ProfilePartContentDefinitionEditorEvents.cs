//using Orchard.ContentManagement.MetaData;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using Orchard.ContentManagement.MetaData.Builders;
//using Orchard.ContentManagement.MetaData.Models;
//using Contrib.Profile.Models;
//using Contrib.Profile.Services;
//using Orchard.ContentTypes.Services;
//using Orchard.ContentTypes.Settings;
//using Orchard.ContentTypes.Extensions;
//using Orchard.Environment.Configuration;
//using Orchard;
//using Orchard.Logging;

//namespace Contrib.Profile.Settings {
//    public class ProfilePartContentDefinitionEditorEvents : ContentDefinitionEditorEventsBase {

//        private readonly IPlacementService _placementService;
//        private readonly IContentDefinitionManager _contentDefinitionManager;
//        private readonly Lazy<IEnumerable<IShellSettingsManagerEventHandler>> _settingsManagerEventHandlers;
//        private readonly ShellSettings _settings;

//        public ProfilePartContentDefinitionEditorEvents(
//            IPlacementService placementService,
//            IContentDefinitionManager contentDefinitionManager,
//            Lazy<IEnumerable<IShellSettingsManagerEventHandler>> settingsManagerEventHandlers,
//            ShellSettings settings) {

//            _placementService = placementService;
//            _contentDefinitionManager = contentDefinitionManager;
//            _settingsManagerEventHandlers = settingsManagerEventHandlers;
//            _settings = settings;

//            Logger = NullLogger.Instance;
//        }

//        public ILogger Logger { get; set; }

//        public override void TypeEditorUpdated(ContentTypeDefinitionBuilder builder) {
//            if (TypeHasProfilePart(builder.Current)) {
//                var typeDefinition = builder.Current;
//                var typeName = typeDefinition.Name;

//                var allPlacements = _placementService.GetEditorPlacement(typeName).ToList();
//                var result = new List<PlacementSettings>(typeDefinition.GetPlacement(PlacementType.Editor));

//                typeDefinition.ResetPlacement(PlacementType.Editor);

//                foreach (var driverPlacement in allPlacements) {
//                    result = result.Where(x => !x.IsSameAs(driverPlacement.PlacementSettings)).ToList();
//                    result.Add(driverPlacement.PlacementSettings);
//                }

//                foreach (var placementSetting in result) {
//                    typeDefinition.Placement(PlacementType.Editor,
//                        placementSetting.ShapeType,
//                        placementSetting.Differentiator,
//                        placementSetting.Zone,
//                        placementSetting.Position);
//                }

//                // persist changes
//                _contentDefinitionManager.StoreTypeDefinition(typeDefinition);

//                _settingsManagerEventHandlers.Value.Invoke(x => x.Saved(_settings), Logger);
//            }
//        }

//        private bool TypeHasProfilePart(ContentTypeDefinition definition) {
//            return definition
//                .Parts.Any(pa => pa.PartDefinition.Name == "ProfilePart");
//        }
//    }
//}