using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.Queries.Services;
using Laser.Orchard.StartupConfig.Models;
using NHibernate.Transform;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization.Services;
using System;
using System.Collections;
using System.Linq;

namespace Laser.Orchard.Services.MailCommunication {

    public interface IMailCommunicationService : IDependency {

        // IHqlQuery IntegrateAdditionalConditions(IHqlQuery query = null, IContent content = null);
        IList GetMailQueryResult(Int32[] ids, Int32? idlingua);
    }

    public class DefaultMailCommunicationService : IMailCommunicationService {
        private readonly IOrchardServices _orchardServices;
        private readonly ICultureManager _cultureManager;
        private readonly IQueryPickerService _queryPickerServices;
        private readonly ISessionLocator _session;

        public DefaultMailCommunicationService(ISessionLocator session, IQueryPickerService queryPickerServices, IOrchardServices orchardServices, ICultureManager cultureManager) {
            _orchardServices = orchardServices;
            _cultureManager = cultureManager;
            _queryPickerServices = queryPickerServices;
            _session = session;
        }

        public IList GetMailQueryResult(Int32[] ids, Int32? idlingua) {// idcontent) {
            // dynamic content = _orchardServices.ContentManager.Get(idcontent);
            //  content = _orchardServices.ContentManager.Get(idcontent, VersionOptions.DraftRequired);
            IHqlQuery query;
            if (ids != null && ids.Count() > 0) {
                //if (content.QueryPickerPart != null && content.QueryPickerPart.Ids.Length > 0) {
                query = IntegrateAdditionalConditions(_queryPickerServices.GetCombinedContentQuery(ids, null, new string[] { "CommunicationContact" }), idlingua);
            }
            else {
                query = IntegrateAdditionalConditions(null, idlingua);
            }

            // Trasformo in stringa HQL
            var stringHQL = ((DefaultHqlQuery)query).ToHql(false);

            // Rimuovo la Order by per poter fare la query annidata
            // TODO: trovare un modo migliore per rimuovere la order by
            stringHQL = stringHQL.ToString().Replace("order by civ.Id", "");

            var queryForEmail = "SELECT distinct cir.Id as Id, TitlePart.Title as Title, EmailRecord.Email as EmailAddress FROM " +
                "Orchard.ContentManagement.Records.ContentItemVersionRecord as civr join " +
                "civr.ContentItemRecord as cir join " +
                "civr.TitlePartRecord as TitlePart join " +
                "cir.EmailContactPartRecord as EmailPart join " +
                    "EmailPart.EmailRecord as EmailRecord " +
                "WHERE civr.Published=1 AND EmailRecord.Validated AND civr.Id in (" + stringHQL + ")";

            // Creo query ottimizzata per le performance
            var fullStatement = _session.For(null)
                .CreateQuery(queryForEmail)
                .SetCacheable(false)
                ;
            IList lista = fullStatement
                    .SetResultTransformer(Transformers.AliasToEntityMap)
                    .List();
            return lista;
        }

        private IHqlQuery IntegrateAdditionalConditions(IHqlQuery query, Int32? idlocalization) {
            if (query == null) {
                query = _orchardServices.ContentManager.HqlQuery().ForType(new string[] { "CommunicationContact" });
            }
            // Query in base alla localizzazione del contenuto
            //  var localizedPart = content.As<LocalizationPart>();
            if (idlocalization != null) {
                //var langId = _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id;  //default site lang
                //if (localizedPart.Culture != null) {
                //    langId = localizedPart.Culture.Id;
                //}
                if (idlocalization == _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id) {
                    // la lingua è quella di default del sito, quindi prendo tutti quelli che hanno espresso la preferenza sulla lingua e quelli che non l'hanno espressa
                    query = query
                        .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Disjunction(a => a.Eq("Culture_Id", idlocalization), b => b.Eq("Culture_Id", 0))); // lingua prescelta uguale a lingua contenuto oppure nessuna lingua prescelta e allora
                }
                else {
                    // la lingua NON è quella di default del sito, quindi prendo SOLO quelli che hanno espresso la preferenza sulla lingua
                    query = query
                        .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Eq("Culture_Id", idlocalization));
                }
            }

            query = query
                .Where(x => x.ContentPartRecord<EmailContactPartRecord>(), x => x.IsNotEmpty("EmailRecord"));

            return query;
        }
    }
}