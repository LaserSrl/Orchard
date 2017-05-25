using Laser.Orchard.Claims.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Drivers {
    public class RequiredClaimsPartDriver : ContentPartDriver<RequiredClaimsPart> {
        private readonly ITokenizer _tokenizer;
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContext;
        public RequiredClaimsPartDriver(ITokenizer tokenizer, IContentManager contentManager, IWorkContextAccessor workContext) {
            _tokenizer = tokenizer;
            _contentManager = contentManager;
            _workContext = workContext;
        }
        protected override DriverResult Display(RequiredClaimsPart part, string displayType, dynamic shapeHelper) {
            return null;
        }

        protected override DriverResult Editor(RequiredClaimsPart part, dynamic shapeHelper) {
            var forceDefault = Convert.ToBoolean(part.Settings["RequiredClaimsPartSettings.ForceDefault"], CultureInfo.InvariantCulture);
            if (forceDefault) {
                return null;
            }
            return ContentShape("Parts_RequiredClaimsPart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/RequiredClaimsPart",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(RequiredClaimsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            // applica i settings
            var claimsDefault = part.Settings["RequiredClaimsPartSettings.ClaimsDefault"];
            var forceDefault = Convert.ToBoolean(part.Settings["RequiredClaimsPartSettings.ForceDefault"], CultureInfo.InvariantCulture);
            if (forceDefault) {
                part.Claims = _tokenizer.Replace(claimsDefault, part.ContentItem);
            } else {
                if (string.IsNullOrWhiteSpace(part.Claims)) {
                    part.Claims = _tokenizer.Replace(claimsDefault, part.ContentItem);
                }
            }
            return Editor(part, shapeHelper);
        }
    }
}