using Laser.Orchard.UserReactions.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.UserReactions.Projections {
    public class UserReactionsQueryFilter : IFilterProvider {
        private readonly IRepository<UserReactionsSummaryRecord> _repoSummary;
        public Localizer T { get; set; }

        public UserReactionsQueryFilter(IRepository<UserReactionsSummaryRecord> repoSummary) {
            _repoSummary = repoSummary;
            T = NullLocalizer.Instance;
        }

        public void Describe(global::Orchard.Projections.Descriptors.Filter.DescribeFilterContext describe) {
            describe.For("Search", T("Search reactions"), T("Search reactions"))
                .Element("ReactionsFilter", T("Reactions filter"), T("Filter for user reactions."),
                    ApplyFilter,
                    DisplayFilter,
                    "ReactionsFilterForm"
                );
        }

        public void ApplyFilter(FilterContext context) {
            string query = context.State.SearchQuery;
            List<int> ids = null;
            int reaction = context.State.Reaction;
            var op = (UserReactionsFieldOperator)Enum.Parse(typeof(UserReactionsFieldOperator), Convert.ToString(context.State.Operator));
            int value = Convert.ToInt32(context.State.Value);
            int min = Convert.ToInt32(context.State.Min);
            int max = Convert.ToInt32(context.State.Max);

            context.Query.Join(a => a.ContentPartRecord<UserReactionsPartRecord>()
                .Property("Reactions", "reactionsSummary")
                .Property("UserReactionsTypesRecord", "reactionType"));
            context.Query.Where(a => a.Named("reactionType"), x => x.Eq("Id", reaction));

            switch (op) {
                case UserReactionsFieldOperator.LessThan:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Lt("Quantity", value));
                    break;
                case UserReactionsFieldOperator.LessThanEquals:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Le("Quantity", value));
                    break;
                case UserReactionsFieldOperator.Equals:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Eq("Quantity", value));
                    break;
                case UserReactionsFieldOperator.NotEquals:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Not(z => z.Eq("Quantity", value)));
                    break;
                case UserReactionsFieldOperator.GreaterThan:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Gt("Quantity", value));
                    break;
                case UserReactionsFieldOperator.GreaterThanEquals:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Ge("Quantity", value));
                    break;
                case UserReactionsFieldOperator.Between:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Between("Quantity", min, max));
                    break;
                case UserReactionsFieldOperator.NotBetween:
                    context.Query.Where(a => a.Named("reactionsSummary"), x => x.Not(z => z.Between("Quantity", min, max)));
                    break;
            }
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Content items having the specified reactions.");
        }
    }
}