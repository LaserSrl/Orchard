using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.Mobile.Handlers;
using Laser.Orchard.Queries.Services;
using Laser.Orchard.StartupConfig.Models;
using NHibernate.Transform;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Services {

    public interface ISmsCommunicationService : IDependency {
        //IHqlQuery IntegrateAdditionalConditions(IHqlQuery query = null, IContent content = null);
        //IHqlQuery IntegrateAdditionalConditions(IHqlQuery query, Int32? idlocalization);
        IList<SmsHQL> GetSmsQueryResult(Int32[] ids, Int32? idlingua);
        //List<string> GetSmsNumbersQueryResult(Int32[] ids, Int32? idlingua);
    }

    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SmsCommunicationService : ISmsCommunicationService {
        private readonly IOrchardServices _orchardServices;
        private readonly ICultureManager _cultureManager;
        private readonly IQueryPickerService _queryPickerServices;
        private readonly ISessionLocator _session;

        public SmsCommunicationService(IOrchardServices orchardServices, IQueryPickerService queryPickerServices, ICultureManager cultureManager, ISessionLocator session)
        {
            _orchardServices = orchardServices;
            _cultureManager = cultureManager;
            _queryPickerServices = queryPickerServices;
            _session = session;
        }

        //public List<string> GetSmsNumbersQueryResult(Int32[] ids, Int32? idlingua)
        //{
        //    List<string> listaNumeri = new List<string>();
        //    var lista = GetSmsQueryResult(ids, idlingua);
        //    if (lista.Count > 0)
        //    {
        //        // Recupero elenco dei numeri di telefono

        //        foreach (var item in lista)
        //        {
        //            string numeroTelefono = ((SmsHQL)item).SmsPrefix + ((SmsHQL)item).SmsNumber;
        //            listaNumeri.Add(numeroTelefono);
        //        }
        //    }
        //    return listaNumeri;
        //}

        public IList<SmsHQL> GetSmsQueryResult(Int32[] ids, Int32? idlingua)
        {
            IHqlQuery query;
            if (ids != null && ids.Length > 0)
            {
                query = IntegrateAdditionalConditions(_queryPickerServices.GetCombinedContentQuery(ids, null, new string[] { "CommunicationContact" }), idlingua);
            }
            else
            {
                query = IntegrateAdditionalConditions(null, idlingua);
            }

            // Trasformo in stringa HQL
            var stringHQL = ((DefaultHqlQuery)query).ToHql(false);

            // Rimuovo la Order by per poter fare la query annidata
            // TODO: trovare un modo migliore per rimuovere la order by
            stringHQL = stringHQL.ToString().Replace("order by civ.Id", "");

            var queryForSms = "SELECT distinct cir.Id as Id, TitlePart.Title as Title, SmsRecord.Prefix as SmsPrefix, SmsRecord.Sms as SmsNumber FROM " +
                "Orchard.ContentManagement.Records.ContentItemVersionRecord as civr join " +
                "civr.ContentItemRecord as cir join " +
                "civr.TitlePartRecord as TitlePart join " +
                "cir.SmsContactPartRecord as SmsPart join " +
                    "SmsPart.SmsRecord as SmsRecord " +
                "WHERE civr.Published=1 AND civr.Id in (" + stringHQL + ")";

            // Creo query ottimizzata per le performance
            var fullStatement = _session.For(null)
                .CreateQuery(queryForSms)
                .SetCacheable(false);

            var lista = fullStatement
                .SetResultTransformer(Transformers.AliasToBean<SmsHQL>())  //(Transformers.AliasToEntityMap)
                .List<SmsHQL>();
            return lista;
        }

        private IHqlQuery IntegrateAdditionalConditions(IHqlQuery query, Int32? idlocalization)
        {
            if (query == null)
            {
                query = _orchardServices.ContentManager.HqlQuery().ForType(new string[] { "CommunicationContact" });
            }

            // Query in base alla localizzazione del contenuto
            if (idlocalization != null)
            {
                if (idlocalization == _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id)
                {
                    // la lingua è quella di default del sito, quindi prendo tutti quelli che hanno espresso la preferenza sulla lingua e quelli che non l'hanno espressa
                    query = query
                        .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Disjunction(a => a.Eq("Culture_Id", idlocalization), b => b.Eq("Culture_Id", 0))); // lingua prescelta uguale a lingua contenuto oppure nessuna lingua prescelta e allora
                }
                else
                {
                    // la lingua NON è quella di default del sito, quindi prendo SOLO quelli che hanno espresso la preferenza sulla lingua
                    query = query
                        .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Eq("Culture_Id", idlocalization));
                }
            }

            query = query
                .Where(x => x.ContentPartRecord<SmsContactPartRecord>(), x => x.IsNotEmpty("SmsRecord"));

            return query;
        }

        //public IHqlQuery IntegrateAdditionalConditions(IHqlQuery query = null, IContent content = null) {
        //    if (query == null) {
        //        query = _orchardServices.ContentManager.HqlQuery().ForType(new string[] { "CommunicationContact" });
        //    }

        //    // Query in base alla localizzazione del contenuto
        //    var localizedPart = content.As<LocalizationPart>();
        //    if (localizedPart != null) {
        //        var langId = _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id;  //default site lang
        //        if (localizedPart.Culture != null) {
        //            langId = localizedPart.Culture.Id;
        //        }
        //        if (langId == _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id) {
        //            // la lingua è quella di default del sito, quindi prendo tutti quelli che hanno espresso la preferenza sulla lingua e quelli che non l'hanno espressa
        //            query = query
        //                .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Disjunction(a => a.Eq("Culture_Id", langId), b => b.Eq("Culture_Id", 0))); // lingua prescelta uguale a lingua contenuto oppure nessuna lingua prescelta e allora
        //        } else {
        //            // la lingua NON è quella di default del sito, quindi prendo SOLO quelli che hanno espresso la preferenza sulla lingua 
        //            query = query
        //                .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Eq("Culture_Id", langId));
        //        }
        //    }

        //    query = query
        //        .Where(x => x.ContentPartRecord<SmsContactPartRecord>(), x => x.IsNotEmpty("SmsRecord"));

        //    return query;
        //}
    }
}