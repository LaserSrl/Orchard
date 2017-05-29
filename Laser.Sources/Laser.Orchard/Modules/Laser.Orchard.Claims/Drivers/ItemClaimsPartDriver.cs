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
    public class ItemClaimsPartDriver : ContentPartDriver<ItemClaimsPart> {
        private readonly ITokenizer _tokenizer;
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContext;
        private readonly IOrchardServices _orchardServices;
        public ItemClaimsPartDriver(ITokenizer tokenizer, IContentManager contentManager, IWorkContextAccessor workContext, IOrchardServices orchardServices) {
            _tokenizer = tokenizer;
            _contentManager = contentManager;
            _workContext = workContext;
            _orchardServices = orchardServices;
        }
        protected override DriverResult Display(ItemClaimsPart part, string displayType, dynamic shapeHelper) {
            return null;
        }

        protected override DriverResult Editor(ItemClaimsPart part, dynamic shapeHelper) {
            if (_orchardServices.Authorizer.Authorize(ClaimsPermissions.EditClaims) == false) {
                return null;
            }
            var forceDefault = false;
            if (part.Settings.ContainsKey("ItemClaimsPartSettings.ForceDefault")) {
                forceDefault = Convert.ToBoolean(part.Settings["ItemClaimsPartSettings.ForceDefault"], CultureInfo.InvariantCulture);
            }
            if (forceDefault) {
                return null;
            }
            return ContentShape("Parts_ItemClaimsPart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/ItemClaimsPart",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(ItemClaimsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (_orchardServices.Authorizer.Authorize(ClaimsPermissions.EditClaims) == false) {
                return null;
            }
            updater.TryUpdateModel(part, Prefix, null, null);
            // applica i settings
            var claimsDefault = "";
            if (part.Settings.ContainsKey("ItemClaimsPartSettings.ClaimsDefault")) {
                claimsDefault = part.Settings["ItemClaimsPartSettings.ClaimsDefault"];
            }
            var forceDefault = false;
            if (part.Settings.ContainsKey("ItemClaimsPartSettings.ForceDefault")) {
                forceDefault = Convert.ToBoolean(part.Settings["ItemClaimsPartSettings.ForceDefault"], CultureInfo.InvariantCulture);
            }
            if (forceDefault) {
                part.Claims = _tokenizer.Replace(claimsDefault, part.ContentItem);
            } else {
                if (string.IsNullOrWhiteSpace(part.Claims)) {
                    part.Claims = _tokenizer.Replace(claimsDefault, part.ContentItem);
                }
            }
            // elimina eventuali spazi iniziali e finali
            part.Claims = part.Claims.Trim();
            // verifica che il valore di Claims sia sempre preceduto e seguuito dalla virgola
            if(string.IsNullOrWhiteSpace(part.Claims) == false) {
                if(part.Claims.StartsWith(",") == false) {
                    part.Claims = "," + part.Claims;
                }
                if (part.Claims.EndsWith(",") == false) {
                    part.Claims += ",";
                }
            }
            return Editor(part, shapeHelper);
        }
    }
}