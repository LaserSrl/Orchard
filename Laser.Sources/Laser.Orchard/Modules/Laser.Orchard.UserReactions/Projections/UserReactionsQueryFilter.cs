using Laser.Orchard.UserReactions.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Razor.Tokenizer;

namespace Laser.Orchard.UserReactions.Projections {
    public class UserReactionsQueryFilter : IFilterProvider {
        private readonly IRepository<UserReactionsSummaryRecord> _repoSummary;
        public Localizer T { get; set; }
        private readonly ITokenizer _tokenizer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repoSummary"></param>
        public UserReactionsQueryFilter(IRepository<UserReactionsSummaryRecord> repoSummary){//, ITokenizer tokenizer 
            _repoSummary = repoSummary;
            T = NullLocalizer.Instance;
            //_tokenizer = tokenizer;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="describe"></param>
        public void Describe(global::Orchard.Projections.Descriptors.Filter.DescribeFilterContext describe) {
            describe.For("Search", T("Search reactions"), T("Search reactions"))
                .Element("ReactionsFilter", T("Reactions filter"), T("Filter for user reactions."),
                    ApplyFilter,
                    DisplayFilter,
                    "ReactionsFilterForm"
                );
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void ApplyFilter(FilterContext context) {
            string query =context.State.SearchQuery;
           

            List<int> ids = null;

            string reaction = context.State.Reaction;
            var op = (UserReactionsFieldOperator)Enum.Parse(typeof(UserReactionsFieldOperator), Convert.ToString(context.State.Operator));
            int value = ((context.State.Value!=string.Empty) ? Convert.ToInt32(context.State.Value):0);
            int min = ((context.State.Min!="")?Convert.ToInt32(context.State.Min):0);
            int max = ((context.State.Max != "") ? Convert.ToInt32(context.State.Max) : 0);

            
            switch (op) {
                case UserReactionsFieldOperator.LessThan:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.TypeName == reaction && x.Quantity < value).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.LessThanEquals:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.TypeName == reaction && x.Quantity <= value).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.Equals:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.TypeName == reaction && x.Quantity == value).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.NotEquals:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.TypeName == reaction && x.Quantity != value).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.GreaterThan:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.TypeName == reaction && x.Quantity > value).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.GreaterThanEquals:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.TypeName == reaction && x.Quantity >= value).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;


                case UserReactionsFieldOperator.Between:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.TypeName == reaction && x.Quantity >= min && x.Quantity <= max).Select(x => x.UserReactionsPartRecord.Id).ToList();
                    break;
                case UserReactionsFieldOperator.NotBetween:
                    ids = _repoSummary.Fetch(x => x.UserReactionsTypesRecord.TypeName == reaction && x.Quantity < min && x.Quantity > max).Select(x => x.UserReactionsPartRecord.Id).ToList();
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



        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public LocalizedString DisplayFilter(FilterContext context) {
            return T("Content items having the specified reactions.");
        }
    }
}