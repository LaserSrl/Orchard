﻿using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Taxonomies.Models;
using Orchard.Tokens;
using System;
using System.Linq;
using System.Collections.Generic;
using Orchard.Taxonomies.Services;

namespace Laser.Orchard.StartupConfig.Projections
{
    public interface IFilterProvider : IEventHandler
    {
        void Describe(dynamic describe);
    }

    [OrchardFeature("Laser.Orchard.StartupConfig.TaxonomiesExtensions")]
    public class TaxonomyTokenFilter : IFilterProvider
    {
        private readonly ITaxonomyService _taxonomyService;
        private int _termsFilterId;
        public Localizer T { get; set; }

        public TaxonomyTokenFilter(ITaxonomyService taxonomyService)
        {
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
        }

        public void Describe(dynamic describe)
        {
            describe.For("Taxonomy", T("Taxonomy"), T("Taxonomy"))
                .Element("HasTermsTokenized", T("Has terms (tokenized)"), T("Categorized content items (search by token)"),
                    (Action<dynamic>)ApplyFilter,
                    (Func<dynamic, LocalizedString>)DisplayFilter,
                    "SelectTermsByTokenForm"
                );
        }

        public void ApplyFilter(dynamic context)
		{
            string ids = context.State.TermToken;

            if (!String.IsNullOrWhiteSpace(ids))
            {
                var idList = ids.Split(new[] { ',' }).Select(Int32.Parse).ToArray();

                var terms = idList.Select(_taxonomyService.GetTerm).ToList();
                var allTerms = new List<TermPart>();
                foreach (var term in terms)
                {
                    if (context.State.IncludeChildren != null)
                        allTerms.AddRange(_taxonomyService.GetChildren(term));

                    allTerms.Add(term);
                }

                allTerms = allTerms.Distinct().ToList();

                var allIds = allTerms.Select(x => x.Id).ToList();

                int op = Convert.ToInt32(context.State.Operator);

                switch (op)
                {
                    case 0: // is one of
                        // Unique alias so we always get a unique join everytime so can have > 1 HasTerms filter on a query.
                        Action<IAliasFactory> s = alias => alias.ContentPartRecord<TermsPartRecord>().Property("Terms", "terms" + _termsFilterId++);
                        Action<IHqlExpressionFactory> f = x => x.InG("TermRecord.Id", allIds);
                        context.Query.Where(s, f);
                        break;
                    case 1: // is all of
                        foreach (var id in allIds)
                        {
                            var termId = id;
                            Action<IAliasFactory> selector =
                                alias => alias.ContentPartRecord<TermsPartRecord>().Property("Terms", "terms" + termId);
                            Action<IHqlExpressionFactory> filter = x => x.Eq("TermRecord.Id", termId);
                            context.Query.Where(selector, filter);
                        }
                    break;
                }
            }

            return;
		}


        public LocalizedString DisplayFilter(dynamic context)
        {
            return T("Categorized with terms specified by token");
        }
    }
}