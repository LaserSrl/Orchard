using Orchard.ContentManagement;
using Orchard.Events;
using Orchard.Localization;
using System;
using System.Globalization;
using Laser.Orchard.StartupConfig.Models;
using Orchard.Localization.Services;
using Orchard.Core.Common.Models;

namespace Laser.Orchard.StartupConfig.Projections {
    public class OwnerIdFilter : IFilterProvider {
        private readonly ICultureManager _cultureManager;

        public OwnerIdFilter(ICultureManager cultureManager) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            describe.For("Owner", T("Owner"), T("Owner"))
                .Element("OwnerID", T("Owner ID"), T("Search for Content Items associated with a owner ID."),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "OwnerIdForm"
                );
        }

        public void ApplyFilter(dynamic context) {
            var query = (IHqlQuery)context.Query;
            if (context.State != null)
                if (context.State.OwnerId != null && context.State.OwnerId != "") {
                    var ownerId = 0;
                    if (!int.TryParse(context.State.OwnerId.Value.ToString(), out ownerId)) {
                       //??
                    }
                    context.Query = query.Where(x => x.ContentPartRecord<CommonPartRecord>(), x => x.Eq("OwnerId", ownerId));
                }
            return;
        }

        public LocalizedString DisplayFilter(dynamic context) {
            return T("Content Items associated with {0} as owner id.", context.State.OwnerId);
        }
    }
}