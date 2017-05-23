using Laser.Orchard.Claims.Models;
using Laser.Orchard.Claims.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Drivers {
    public class IdentityClaimsPartDriver : ContentPartDriver<IdentityClaimsPart> {
        private readonly ITransactionManager _transactionManager;
        public IdentityClaimsPartDriver(ITransactionManager transactionManager) {
            _transactionManager = transactionManager;
        }
        protected override DriverResult Display(IdentityClaimsPart part, string displayType, dynamic shapeHelper) {
            return null;
        }
        protected override DriverResult Editor(IdentityClaimsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_IdentityClaimsPart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/IdentityClaimsPart",
                    Model: new IdentityClaimsPartVM(part),
                    Prefix: Prefix));
        }
        protected override DriverResult Editor(IdentityClaimsPart part, IUpdateModel updater, dynamic shapeHelper) {
            //updater.TryUpdateModel(part, Prefix, null, null);
            IdentityClaimsPartVM vm = new IdentityClaimsPartVM(part);
            updater.TryUpdateModel(vm, Prefix, null, null);

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