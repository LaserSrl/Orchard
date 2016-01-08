using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Tokens;
using Orchard.Projections.Descriptors;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Forms.Services;

namespace Laser.Orchard.Queries.Services {
    public interface IQueryPickerService : IDependency {
        List<QueryPart> GetUserDefinedQueries();
        IEnumerable<ContentItem> GetContentItemsAndCombined(int[] queryIds, int skip = 0, int count = 0);
        IEnumerable<IHqlQuery> GetContentQueries(QueryPartRecord queryRecord, Dictionary<string, object> tokens = null);
    }


    public class QueryPickerDefault : IQueryPickerService {
        private readonly IOrchardServices _services;
        private readonly IEnumerable<IFilterProvider> _filterProviders;
        private readonly IContentManager _contentManager;
        private readonly ITokenizer _tokenizer;
        private readonly IProjectionManager _projectionManager;

        public QueryPickerDefault(IOrchardServices services, IContentManager contentManager,
                        ITokenizer tokenizer,
                        IProjectionManager projectionManager,
                        IEnumerable<IFilterProvider> filterProviders) {
            _services = services;
            _tokenizer = tokenizer;
            _contentManager = contentManager;
            _projectionManager = projectionManager;

            _filterProviders = filterProviders;

        }

        public List<QueryPart> GetUserDefinedQueries() {
            var availableQueries = _services.ContentManager.Query().ForType("Query").Join<TitlePartRecord>().OrderBy(x => x.Title).List()
                .Where(x => ((dynamic)x).QueryUserFilterExtensionPart.UserQuery.Value == true)
                .Select(x => x.As<QueryPart>());
            return availableQueries.ToList();
        }

        public IEnumerable<ContentItem> GetContentItemsAndCombined(int[] queryIds, int skip = 0, int count = 0) {
            IEnumerable<ContentItem> contentItems = new List<ContentItem>();

            foreach (var queryId in queryIds) {
                if (contentItems.Count() > 0) {
                    contentItems = contentItems.Intersect(_projectionManager.GetContentItems(queryId, 0, 5000000));
                } else {
                    contentItems = _projectionManager.GetContentItems(queryId, 0, 5000000);
                }
            }
            return contentItems.Skip(skip).Take(count);
        }

        public IEnumerable<IHqlQuery> GetContentQueries(QueryPartRecord queryRecord, Dictionary<string, object> tokens = null) {
            var availableFilters = DescribeFilters().ToList();

            // pre-executing all groups 
            foreach (var group in queryRecord.FilterGroups) {

                var contentQuery = _contentManager.HqlQuery().ForVersion(VersionOptions.Published);
                // fatto da HERMES
                if (tokens == null) {
                    tokens = new Dictionary<string, object>();
                }
                //FINE
                // iterate over each filter to apply the alterations to the query object
                foreach (var filter in group.Filters) {
                    var tokenizedState = _tokenizer.Replace(filter.State, tokens /*new Dictionary<string, object>()*/);
                    var filterContext = new FilterContext {
                        Query = contentQuery,
                        State = FormParametersHelper.ToDynamic(tokenizedState)
                    };

                    string category = filter.Category;
                    string type = filter.Type;

                    // look for the specific filter component
                    var descriptor = availableFilters
                        .SelectMany(x => x.Descriptors)
                        .FirstOrDefault(x => x.Category == category && x.Type == type);

                    // ignore unfound descriptors
                    if (descriptor == null) {
                        continue;
                    }

                    // apply alteration
                    descriptor.Filter(filterContext);

                    contentQuery = filterContext.Query;
                }
                // Siccome sono in una Query Definita dallo User, devo anche filtrare per ContentType "CommunicationContact"
                var contentTypesToFilter = "CommunicationContact";
                contentQuery = contentQuery.ForType(contentTypesToFilter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

                // iterate over each sort criteria to apply the alterations to the query object
                yield return contentQuery;
            }
        }
        private IEnumerable<TypeDescriptor<FilterDescriptor>> DescribeFilters() {
            var context = new DescribeFilterContext();

            foreach (var provider in _filterProviders) {
                provider.Describe(context);
            }
            return context.Describe();
        }

    }
}