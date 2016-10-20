using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Services;
using Orchard.Users.Models;
using NHibernate.Transform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Laser.Orchard.UserReactions.Projections {
    public class ReactionClickedQueryFilter : IFilterProvider {
        public Localizer T { get; set; }

        public ReactionClickedQueryFilter() {
            T = NullLocalizer.Instance;
        }
        public void Describe(DescribeFilterContext describe) {
            describe.For("Search", T("Search reactions"), T("Search reactions"))
                .Element("Reaction Clicked Filter", T("Reaction clicked filter"), T("Filter for a specific reaction."),
                    ApplyFilter,
                    DisplayFilter,
                    "ReactionClickedFilterForm"
                );
        }
        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Users who clicked on a specific reaction.");
        }
        public void ApplyFilter(FilterContext context) {
            char[] separator = { ',' };
            string reaction = context.State.Reaction;
            // recupera il contentId da un content picker field
            int contentId = 0;
            string aux = (string)(context.State.ContentId);
            if (string.IsNullOrWhiteSpace(aux) == false) {
                var arr = aux.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 0) {
                    contentId = Convert.ToInt32(arr[0]);
                }
            }
            string subquery = string.Format(@"select contact.Id as contactId
                from Laser.Orchard.CommunicationGateway.Models.CommunicationContactPartRecord as contact, 
                Laser.Orchard.UserReactions.Models.UserReactionsClickRecord as click
                where click.UserPartRecord.Id = contact.UserPartRecord_Id
                and click.UserReactionsTypesRecord.TypeName='{0}'
                and click.ContentItemRecordId={1}
                group by contact.Id 
                having mod(count(contact.Id) , 2) = 1", reaction.Replace("'", "''"), contentId);
            context.Query.Where(a => a.Named("ci"), x => x.InSubquery("Id", subquery));
        }
    }
}