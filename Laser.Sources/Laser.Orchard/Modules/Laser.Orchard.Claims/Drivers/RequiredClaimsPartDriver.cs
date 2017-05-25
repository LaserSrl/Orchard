using Laser.Orchard.Claims.Models;
using Laser.Orchard.Claims.Security;
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
        private readonly IOrchardServices _orchardServices;
        public RequiredClaimsPartDriver(ITokenizer tokenizer, IContentManager contentManager, IWorkContextAccessor workContext, IOrchardServices orchardServices) {
            _tokenizer = tokenizer;
            _contentManager = contentManager;
            _workContext = workContext;
            _orchardServices = orchardServices;
        }
        protected override DriverResult Display(RequiredClaimsPart part, string displayType, dynamic shapeHelper) {
            return null;
        }

        protected override DriverResult Editor(RequiredClaimsPart part, dynamic shapeHelper) {
            if (_orchardServices.Authorizer.Authorize(ClaimsPermissions.EditClaims) == false) {
                return null;
            }
            var forceDefault = false;
            try {
                forceDefault = Convert.ToBoolean(part.Settings["RequiredClaimsPartSettings.ForceDefault"], CultureInfo.InvariantCulture);
            } catch {
                forceDefault = false;
            }
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
            if (_orchardServices.Authorizer.Authorize(ClaimsPermissions.EditClaims) == false) {
                return null;
            }
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