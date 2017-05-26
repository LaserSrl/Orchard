//using Autofac;
//using Laser.Orchard.Claims.Services;
//using NHibernate;
//using NHibernate.Criterion;
//using NHibernate.SqlCommand;
//using NHibernate.Transform;
//using Orchard;
//using Orchard.Caching;
//using Orchard.ContentManagement;
//using Orchard.ContentManagement.Handlers;
//using Orchard.ContentManagement.MetaData;
//using Orchard.ContentManagement.Records;
//using Orchard.Data;
//using Orchard.Data.Providers;
//using Orchard.Environment.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Laser.Orchard.Claims.Providers {
//    public class ClaimsContentManager : DefaultContentManager {
//        private readonly IComponentContext _context;
//        private readonly Lazy<ITransactionManager> _transactionManager;
//        private readonly Lazy<IEnumerable<ISqlStatementProvider>> _sqlStatementProviders;
//        private readonly ShellSettings _shellSettings;
//        private readonly Func<IContentManagerSession> _contentManagerSession;
//        private readonly IRepository<ContentItemVersionRecord> _contentItemVersionRepository;
//        private readonly IRepository<ContentItemRecord> _contentItemRepository;
//        private readonly IClaimsCheckerService _claimsCheckerService;
//        public ClaimsContentManager(
//            IComponentContext context,
//            IRepository<ContentTypeRecord> contentTypeRepository,
//            IRepository<ContentItemRecord> contentItemRepository,
//            IRepository<ContentItemVersionRecord> contentItemVersionRepository,
//            IContentDefinitionManager contentDefinitionManager,
//            ICacheManager cacheManager,
//            Func<IContentManagerSession> contentManagerSession,
//            Lazy<IContentDisplay> contentDisplay,
//            Lazy<ITransactionManager> transactionManager,
//            Lazy<IEnumerable<IContentHandler>> handlers,
//            Lazy<IEnumerable<IIdentityResolverSelector>> identityResolverSelectors,
//            Lazy<IEnumerable<ISqlStatementProvider>> sqlStatementProviders,
//            ShellSettings shellSettings,
//            ISignals signals,
//            IClaimsCheckerService claimsCheckerService) : base(context, contentTypeRepository, contentItemRepository, contentItemVersionRepository,
//                contentDefinitionManager, cacheManager, contentManagerSession, contentDisplay, transactionManager,
//                handlers, identityResolverSelectors, sqlStatementProviders, shellSettings, signals) {
//            _context = context;
//            _transactionManager = transactionManager;
//            _sqlStatementProviders = sqlStatementProviders;
//            _shellSettings = shellSettings;
//            _contentManagerSession = contentManagerSession;
//            _contentItemVersionRepository = contentItemVersionRepository;
//            _contentItemRepository = contentItemRepository;
//            _claimsCheckerService = claimsCheckerService;
//        }
//        public override ContentItem Get(int id, VersionOptions options, QueryHints hints) {
//            var session = _contentManagerSession();
//            ContentItem contentItem;

//            ContentItemVersionRecord versionRecord = null;

//            // obtain the root records based on version options
//            if (options.VersionRecordId != 0) {
//                // short-circuit if item held in session
//                if (session.RecallVersionRecordId(options.VersionRecordId, out contentItem)) {
//                    return _claimsCheckerService.CheckClaims(contentItem);
//                }

//                versionRecord = _contentItemVersionRepository.Get(options.VersionRecordId);
//            } else if (options.VersionNumber != 0) {
//                // short-circuit if item held in session
//                if (session.RecallVersionNumber(id, options.VersionNumber, out contentItem)) {
//                    return _claimsCheckerService.CheckClaims(contentItem);
//                }

//                versionRecord = _contentItemVersionRepository.Get(x => x.ContentItemRecord.Id == id && x.Number == options.VersionNumber);
//            } else if (session.RecallContentRecordId(id, out contentItem)) {
//                // try to reload a previously loaded published content item

//                if (options.IsPublished) {
//                    return _claimsCheckerService.CheckClaims(contentItem);
//                }

//                versionRecord = contentItem.VersionRecord;
//            } else {
//                // do a query to load the records in case Get is called directly
//                var contentItemVersionRecords = GetManyImplementation(hints,
//                    (contentItemCriteria, contentItemVersionCriteria) => {
//                        contentItemCriteria.Add(Restrictions.Eq("Id", id));
//                        if (options.IsPublished) {
//                            contentItemVersionCriteria.Add(Restrictions.Eq("Published", true));
//                        } else if (options.IsLatest) {
//                            contentItemVersionCriteria.Add(Restrictions.Eq("Latest", true));
//                        } else if (options.IsDraft && !options.IsDraftRequired) {
//                            contentItemVersionCriteria.Add(
//                                Restrictions.And(Restrictions.Eq("Published", false),
//                                                Restrictions.Eq("Latest", true)));
//                        } else if (options.IsDraft || options.IsDraftRequired) {
//                            contentItemVersionCriteria.Add(Restrictions.Eq("Latest", true));
//                        }

//                        contentItemVersionCriteria.SetFetchMode("ContentItemRecord", FetchMode.Eager);
//                        contentItemVersionCriteria.SetFetchMode("ContentItemRecord.ContentType", FetchMode.Eager);
//                        //contentItemVersionCriteria.SetMaxResults(1);
//                    });


//                if (options.VersionNumber != 0) {
//                    versionRecord = contentItemVersionRecords.FirstOrDefault(
//                        x => x.Number == options.VersionNumber) ??
//                           _contentItemVersionRepository.Get(
//                               x => x.ContentItemRecord.Id == id && x.Number == options.VersionNumber);
//                } else {
//                    versionRecord = contentItemVersionRecords.LastOrDefault();
//                }
//            }

//            // no record means content item is not in db
//            if (versionRecord == null) {
//                // check in memory
//                var record = _contentItemRepository.Get(id);
//                if (record == null) {
//                    return null;
//                }

//                versionRecord = GetVersionRecord(options, record);

//                if (versionRecord == null) {
//                    return null;
//                }
//            }

//            // return item if obtained earlier in session
//            if (session.RecallVersionRecordId(versionRecord.Id, out contentItem)) {
//                if (options.IsDraftRequired && versionRecord.Published) {
//                    return _claimsCheckerService.CheckClaims(BuildNewVersion(contentItem));
//                }
//                return _claimsCheckerService.CheckClaims(contentItem);
//            }

//            // allocate instance and set record property
//            contentItem = New(versionRecord.ContentItemRecord.ContentType.Name);
//            contentItem.VersionRecord = versionRecord;

//            // store in session prior to loading to avoid some problems with simple circular dependencies
//            session.Store(contentItem);

//            // create a context with a new instance to load            
//            var context = new LoadContentContext(contentItem);

//            // invoke handlers to acquire state, or at least establish lazy loading callbacks
//            Handlers.Invoke(handler => handler.Loading(context), Logger);
//            Handlers.Invoke(handler => handler.Loaded(context), Logger);

//            // when draft is required and latest is published a new version is appended 
//            if (options.IsDraftRequired && versionRecord.Published) {
//                contentItem = BuildNewVersion(context.ContentItem);
//            }

//            return _claimsCheckerService.CheckClaims(contentItem);
//        }
//        private IEnumerable<ContentItemVersionRecord> GetManyImplementation(QueryHints hints, Action<ICriteria, ICriteria> predicate) {
//            var session = _transactionManager.Value.GetSession();
//            var contentItemVersionCriteria = session.CreateCriteria(typeof(ContentItemVersionRecord));
//            var contentItemCriteria = contentItemVersionCriteria.CreateCriteria("ContentItemRecord");
//            predicate(contentItemCriteria, contentItemVersionCriteria);

//            var contentItemMetadata = session.SessionFactory.GetClassMetadata(typeof(ContentItemRecord));
//            var contentItemVersionMetadata = session.SessionFactory.GetClassMetadata(typeof(ContentItemVersionRecord));

//            if (hints != QueryHints.Empty) {
//                // break apart and group hints by their first segment
//                var hintDictionary = hints.Records
//                    .Select(hint => new { Hint = hint, Segments = hint.Split('.') })
//                    .GroupBy(item => item.Segments.FirstOrDefault())
//                    .ToDictionary(grouping => grouping.Key, StringComparer.InvariantCultureIgnoreCase);

//                // locate hints that match properties in the ContentItemVersionRecord
//                foreach (var hit in contentItemVersionMetadata.PropertyNames.Where(hintDictionary.ContainsKey).SelectMany(key => hintDictionary[key])) {
//                    contentItemVersionCriteria.SetFetchMode(hit.Hint, FetchMode.Eager);
//                    hit.Segments.Take(hit.Segments.Count() - 1).Aggregate(contentItemVersionCriteria, ExtendCriteria);
//                }

//                // locate hints that match properties in the ContentItemRecord
//                foreach (var hit in contentItemMetadata.PropertyNames.Where(hintDictionary.ContainsKey).SelectMany(key => hintDictionary[key])) {
//                    contentItemVersionCriteria.SetFetchMode("ContentItemRecord." + hit.Hint, FetchMode.Eager);
//                    hit.Segments.Take(hit.Segments.Count() - 1).Aggregate(contentItemCriteria, ExtendCriteria);
//                }

//                if (hintDictionary.SelectMany(x => x.Value).Any(x => x.Segments.Count() > 1))
//                    contentItemVersionCriteria.SetResultTransformer(new DistinctRootEntityResultTransformer());
//            }

//            contentItemCriteria.SetCacheable(true);

//            return contentItemVersionCriteria.List<ContentItemVersionRecord>();
//        }
//        private static ICriteria ExtendCriteria(ICriteria criteria, string segment) {
//            return criteria.GetCriteriaByPath(segment) ?? criteria.CreateCriteria(segment, JoinType.LeftOuterJoin);
//        }
//        private ContentItemVersionRecord GetVersionRecord(VersionOptions options, ContentItemRecord itemRecord) {
//            if (options.IsPublished) {
//                return itemRecord.Versions.FirstOrDefault(
//                    x => x.Published) ??
//                       _contentItemVersionRepository.Get(
//                           x => x.ContentItemRecord == itemRecord && x.Published);
//            }
//            if (options.IsLatest || options.IsDraftRequired) {
//                return itemRecord.Versions.FirstOrDefault(
//                    x => x.Latest) ??
//                       _contentItemVersionRepository.Get(
//                           x => x.ContentItemRecord == itemRecord && x.Latest);
//            }
//            if (options.IsDraft) {
//                return itemRecord.Versions.FirstOrDefault(
//                    x => x.Latest && !x.Published) ??
//                       _contentItemVersionRepository.Get(
//                           x => x.ContentItemRecord == itemRecord && x.Latest && !x.Published);
//            }
//            if (options.VersionNumber != 0) {
//                return itemRecord.Versions.FirstOrDefault(
//                    x => x.Number == options.VersionNumber) ??
//                       _contentItemVersionRepository.Get(
//                           x => x.ContentItemRecord == itemRecord && x.Number == options.VersionNumber);
//            }
//            return null;
//        }
//    }
//}