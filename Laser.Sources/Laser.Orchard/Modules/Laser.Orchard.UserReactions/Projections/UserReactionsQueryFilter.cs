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

            switch (op) {
                case UserReactionsFieldOperator.LessThan:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.Id == reaction && x.Quantity < value).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.LessThanEquals:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.Id == reaction && x.Quantity <= value).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.Equals:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.Id == reaction && x.Quantity == value).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.NotEquals:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.Id == reaction && x.Quantity != value).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.GreaterThan:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.Id == reaction && x.Quantity > value).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.GreaterThanEquals:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.Id == reaction && x.Quantity >= value).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.Between:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.Id == reaction && x.Quantity >= min && x.Quantity <= max).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.NotBetween:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.Id == reaction && x.Quantity < min && x.Quantity > max).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
            }
            if (ids.Count > 0) {
                context.Query.Where(x => x.ContentItem(), x => x.InG<int>("Id", ids));
            }
            else {
                // non deve estrare nulla quindi metto la condizione Id = 0
                context.Query.Where(x => x.ContentItem(), x => x.Eq("Id", 0));
            }
        }

        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Content items having the specified reactions.");
        }
    }
}