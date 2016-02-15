using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.StartupConfig.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Services {

    public interface ISmsCommunicationService : IDependency {
        IHqlQuery IntegrateAdditionalConditions(IHqlQuery query = null, IContent content = null);
    }

    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SmsCommunicationService : ISmsCommunicationService {
        private readonly IOrchardServices _orchardServices;
        private readonly ICultureManager _cultureManager;

        public SmsCommunicationService(IOrchardServices orchardServices, ICultureManager cultureManager) {
            _orchardServices = orchardServices;
            _cultureManager = cultureManager;
        }

        public IHqlQuery IntegrateAdditionalConditions(IHqlQuery query = null, IContent content = null) {
            if (query == null) {
                query = _orchardServices.ContentManager.HqlQuery().ForType(new string[] { "CommunicationContact" });
            }

            // Query in base alla localizzazione del contenuto
            var localizedPart = content.As<LocalizationPart>();
            if (localizedPart != null) {
                var langId = _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id;  //default site lang
                if (localizedPart.Culture != null) {
                    langId = localizedPart.Culture.Id;
                }
                if (langId == _cultureManager.GetCultureByName(_orchardServices.WorkContext.CurrentSite.SiteCulture).Id) {
                    // la lingua è quella di default del sito, quindi prendo tutti quelli che hanno espresso la preferenza sulla lingua e quelli che non l'hanno espressa
                    query = query
                        .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Disjunction(a => a.Eq("Culture_Id", langId), b => b.Eq("Culture_Id", 0))); // lingua prescelta uguale a lingua contenuto oppure nessuna lingua prescelta e allora
                } else {
                    // la lingua NON è quella di default del sito, quindi prendo SOLO quelli che hanno espresso la preferenza sulla lingua 
                    query = query
                        .Where(x => x.ContentPartRecord<FavoriteCulturePartRecord>(), x => x.Eq("Culture_Id", langId));
                }
            }

            query = query
                .Where(x => x.ContentPartRecord<SmsContactPartRecord>(), x => x.IsNotEmpty("SmsRecord"));

            return query;
        }
    }
}