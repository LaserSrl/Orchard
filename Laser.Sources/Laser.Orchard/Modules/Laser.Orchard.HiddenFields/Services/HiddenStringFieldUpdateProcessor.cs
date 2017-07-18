using Laser.Orchard.HiddenFields.Fields;
using Orchard.ContentManagement;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HiddenFields.Services {
    public class HiddenStringFieldUpdateProcessor : IHiddenStringFieldUpdateProcessor {

        private readonly IContentManager _contentManager;
        public Localizer T;

        private IDictionary<HiddenStringFieldUpdateProcessVariant, string> _variantDescriptions;
        public HiddenStringFieldUpdateProcessor(IContentManager contentManager) {
            _contentManager = contentManager;

            T = NullLocalizer.Instance;

            _variantDescriptions = new Dictionary<HiddenStringFieldUpdateProcessVariant, string>() {
                { HiddenStringFieldUpdateProcessVariant.None, T("None").Text },
                { HiddenStringFieldUpdateProcessVariant.All, T("All fields").Text },
                { HiddenStringFieldUpdateProcessVariant.Empty, T("Empy fields").Text }
            };
        }

        public void Process(IEnumerable<HiddenStringField> fields) {

        }

        private void Process(HiddenStringField field) {

        }

        public string GetDescription(HiddenStringFieldUpdateProcessVariant variant) {
            if (_variantDescriptions.ContainsKey(variant)) {
                return _variantDescriptions[variant];
            }
            return variant.ToString();
        }
    }
}