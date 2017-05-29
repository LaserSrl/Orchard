using Laser.Orchard.Claims.Models;
using Laser.Orchard.Claims.Security;
using Laser.Orchard.Claims.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Drivers {
    public class IdentityClaimsPartDriver : ContentPartDriver<IdentityClaimsPart> {
        private readonly ITransactionManager _transactionManager;
        private readonly ITokenizer _tokenizer;
        private readonly IOrchardServices _orchardServices;
        public IdentityClaimsPartDriver(ITokenizer tokenizer, ITransactionManager transactionManager, IOrchardServices orchardServices) {
            _tokenizer = tokenizer;
            _transactionManager = transactionManager;
            _orchardServices = orchardServices;
        }
        protected override DriverResult Display(IdentityClaimsPart part, string displayType, dynamic shapeHelper) {
            return null;
        }
        protected override DriverResult Editor(IdentityClaimsPart part, dynamic shapeHelper) {
            if (_orchardServices.Authorizer.Authorize(ClaimsPermissions.EditClaims) == false) {
                return null;
            }
            var forceDefault = false;
            if (part.Settings.ContainsKey("IdentityClaimsPartSettings.ForceDefault")) {
                forceDefault = Convert.ToBoolean(part.Settings["IdentityClaimsPartSettings.ForceDefault"], CultureInfo.InvariantCulture);
            }
            if (forceDefault) {
                return null;
            }
            return ContentShape("Parts_IdentityClaimsPart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/IdentityClaimsPart",
                    Model: new IdentityClaimsPartVM(part),
                    Prefix: Prefix));
        }
        protected override DriverResult Editor(IdentityClaimsPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (_orchardServices.Authorizer.Authorize(ClaimsPermissions.EditClaims) == false) {
                return null;
            }
            IdentityClaimsPartVM vm = new IdentityClaimsPartVM(part);
            updater.TryUpdateModel(vm, Prefix, null, null);
            // applica i settings
            var claimsDefault = "";
            if (part.Settings.ContainsKey("IdentityClaimsPartSettings.ClaimsDefault")) {
                claimsDefault = part.Settings["IdentityClaimsPartSettings.ClaimsDefault"];
            }
            var forceDefault = false;
            if (part.Settings.ContainsKey("IdentityClaimsPartSettings.ForceDefault")) {
                forceDefault = Convert.ToBoolean(part.Settings["IdentityClaimsPartSettings.ForceDefault"], CultureInfo.InvariantCulture);
            }
            if (forceDefault) {
                vm.ClaimsArea = _tokenizer.Replace(claimsDefault, part.ContentItem);
            } else {
                if (string.IsNullOrWhiteSpace(vm.ClaimsArea)) {
                    vm.ClaimsArea = _tokenizer.Replace(claimsDefault, part.ContentItem);
                }
            }

            // deserializza le claims
            var session = _transactionManager.GetSession();
            var sep = new string[] { "\r", "\n", "\r\n" };
            var toDelete = new List<IdentityClaimsRecord>();
            toDelete.AddRange(part.ClaimsSets);
            part.ClaimsSets.Clear();
            foreach(var row in vm.ClaimsArea.Split(sep, StringSplitOptions.RemoveEmptyEntries)) {
                var claims = toDelete.FirstOrDefault(x => x.IdentityClaimsPartRecord_id == part.Id && x.IdentityClaims == row);
                if(claims == null) {
                    // nuova claims
                    var record = new IdentityClaimsRecord() { IdentityClaims = row, IdentityClaimsPartRecord_id = part.Id };
                    session.Save(record);
                    part.ClaimsSets.Add(record);
                } else {
                    // claims invariata
                    part.ClaimsSets.Add(claims);
                }
            }
            return Editor(part, shapeHelper);
        }
    }
}