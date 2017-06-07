using Laser.Orchard.DataProtection.Models;
using Laser.Orchard.DataProtection.Security;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Laser.Orchard.DataProtection.Drivers {
    public class DataContextPartDriver : ContentPartDriver<DataContextPart> {
        private readonly ITokenizer _tokenizer;
        private readonly IContentManager _contentManager;
        private readonly IWorkContextAccessor _workContext;
        private readonly IOrchardServices _orchardServices;
        public DataContextPartDriver(ITokenizer tokenizer, IContentManager contentManager, IWorkContextAccessor workContext, IOrchardServices orchardServices) {
            _tokenizer = tokenizer;
            _contentManager = contentManager;
            _workContext = workContext;
            _orchardServices = orchardServices;
        }
        protected override DriverResult Display(DataContextPart part, string displayType, dynamic shapeHelper) {
            return null;
        }

        protected override DriverResult Editor(DataContextPart part, dynamic shapeHelper) {
            if (_orchardServices.Authorizer.Authorize(DataProtectionPermissions.ManageDataProtection) == false) {
                return null;
            }
            var forceDefault = false;
            if (part.Settings.ContainsKey("DataContextPartSettings.ForceDefault")) {
                forceDefault = Convert.ToBoolean(part.Settings["DataContextPartSettings.ForceDefault"], CultureInfo.InvariantCulture);
            }
            if (forceDefault) {
                return null;
            }
            return ContentShape("Parts_DataContextPart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/DataContextPart",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(DataContextPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (_orchardServices.Authorizer.Authorize(DataProtectionPermissions.ManageDataProtection) == false) {
                return null;
            }
            updater.TryUpdateModel(part, Prefix, null, null);
            // applica i settings
            var contextDefault = "";
            if (part.Settings.ContainsKey("DataContextPartSettings.ContextDefault")) {
                contextDefault = part.Settings["DataContextPartSettings.ContextDefault"];
            }
            var forceDefault = false;
            if (part.Settings.ContainsKey("DataContextPartSettings.ForceDefault")) {
                forceDefault = Convert.ToBoolean(part.Settings["DataContextPartSettings.ForceDefault"], CultureInfo.InvariantCulture);
            }
            if (forceDefault) {
                part.Context = _tokenizer.Replace(contextDefault, part.ContentItem);
            } else {
                if (string.IsNullOrWhiteSpace(part.Context)) {
                    part.Context = _tokenizer.Replace(contextDefault, part.ContentItem);
                }
            }
            // elimina eventuali spazi iniziali e finali
            part.Context = part.Context.Trim();
            // verifica che il valore di Context sia sempre preceduto e seguito dalla virgola
            if(string.IsNullOrWhiteSpace(part.Context) == false) {
                if(part.Context.StartsWith(",") == false) {
                    part.Context = "," + part.Context;
                }
                if (part.Context.EndsWith(",") == false) {
                    part.Context += ",";
                }
            }
            return Editor(part, shapeHelper);
        }
    }
}