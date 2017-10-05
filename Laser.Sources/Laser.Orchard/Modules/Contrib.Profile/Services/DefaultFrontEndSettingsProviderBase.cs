using Contrib.Profile.Settings;
using Orchard.ContentManagement.MetaData.Models;
using System.Collections.Generic;
using System.Linq;

namespace Contrib.Profile.Services {
    /// <summary>
    /// Base implementation for providers telling the default front-end display/edit settings
    /// for a part. To use this, inherit from this abstract class and provde a constructor 
    /// setting PartName, AllowDisplay and AllowEdit. E.g.:
    /// 
    /// public class MyImplementation : DefaultFrontEndSettingsProviderBase {
    ///     public MyImplementation()
    ///         : base("MyPartName", true, false) {}
    /// }
    /// 
    /// Those four lines are all that is strictly required. Usually, there will be no need to 
    /// override the implemented virtual methods.
    /// </summary>
    public abstract class DefaultFrontEndSettingsProviderBase : IDefaultFrontEndSettingsProvider {
        protected string PartName { get; }
        protected bool AllowDisplay { get; }
        protected bool AllowEdit { get; }

        public DefaultFrontEndSettingsProviderBase(string partName, bool allowDisplay, bool allowEdit) {
            PartName = partName;
            AllowDisplay = allowDisplay;
            AllowEdit = allowEdit;
        }

        public virtual void ConfigureDefaultValues(ContentTypeDefinition definition) {
            if (TypeHasProfilePart(definition)) { //sanity check
                foreach (var typePartDefinition in definition.Parts
                    .Where(ctpd => ctpd.PartDefinition.Name == PartName)) {

                    ProfileFrontEndSettings.SetValues(typePartDefinition.Settings, AllowDisplay, AllowEdit);
                }
            }
        }

        public virtual IEnumerable<string> ForParts() {
            return new string[] { PartName };
        }

        protected bool TypeHasProfilePart(ContentTypeDefinition definition) {
            return definition.Parts
                .Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart");
        }
    }
}