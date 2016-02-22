using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.Queries.Services;
using NHibernate.Transform;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Handlers {

    #region Support Classes

    // Classe queryForSms
    public class SmsHQL {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SmsPrefix { get; set; }
        public string SmsNumber { get; set; }
    }

    #endregion

    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SmsGatewayPartHandler : ContentHandler {

        private readonly IOrchardServices _orchardServices;
        private readonly INotifier _notifier;
        private readonly ISmsServices _smsServices;
        private readonly ISmsCommunicationService _smsCommunicationService;
        private readonly IQueryPickerService _queryPickerServices;
        private readonly ISessionLocator _session;

        public Localizer T { get; set; }


        public SmsGatewayPartHandler(IRepository<SmsGatewayPartRecord> repository, IOrchardServices orchardServices, INotifier notifier, ISmsServices smsServices,
                                     ISmsCommunicationService smsCommunicationService, IQueryPickerService queryPickerServices, ISessionLocator session) {
            _notifier = notifier;
            _orchardServices = orchardServices;
            _smsServices = smsServices;
            _smsCommunicationService = smsCommunicationService;
            _queryPickerServices = queryPickerServices;
            _session = session;

            Filters.Add(StorageFilter.For(repository));

            OnUpdated<SmsGatewayPart>((context, part) => {
                if (_orchardServices.WorkContext.HttpContext.Request.Form["submit.Test"] == "submit.SmsTest") {
                    if (part.SendToTestNumber && part.NumberForTest != string.Empty) {
                        if (part.ContentItem.ContentType == "CommunicationAdvertising") {

                            string linktosend = "";
                            ICommunicationService _communicationService;

                            bool tryed = _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
                            if (tryed) {
                                if (_communicationService.CampaignLinkExist(part)) {
                                    linktosend = _communicationService.GetCampaignLink("Sms", part);
                                }
                            }
                            string messageToSms = part.Message + " " + linktosend;

                            // Invio SMS a NumberForTest
                            _smsServices.SendSms(
                                part.NumberForTest.Split(';').Select(x => Convert.ToInt64(x)).ToArray(),
                                messageToSms, part.Alias, part.Id.ToString(), part.HaveAlias);
                        }
                    }
                }
            });

            OnPublished<SmsGatewayPart>((context, part) => {
                if (part.SendOnNextPublish && !part.SmsMessageSent) {
                    dynamic content = context.ContentItem;

                    IHqlQuery query;
                    if (content.QueryPickerPart != null && content.QueryPickerPart.Ids.Length > 0) {
                        query = _smsCommunicationService.IntegrateAdditionalConditions(_queryPickerServices.GetCombinedContentQuery(content.QueryPickerPart.Ids, null, new string[] { "CommunicationContact" }), content);
                    } else {
                        query = _smsCommunicationService.IntegrateAdditionalConditions(null, content);
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

                    if (lista.Count > 0) {
                        // Recupero elenco dei numeri di telefono
                        List<string> listaNumeri = new List<string>();

                        foreach (var item in lista) {
                            string numeroTelefono = ((SmsHQL)item).SmsPrefix + ((SmsHQL)item).SmsNumber;
                            listaNumeri.Add(numeroTelefono);
                        }

                        string linktosend = "";
                        ICommunicationService _communicationService;

                        bool tryed = _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
                        if (tryed) {
                            if (_communicationService.CampaignLinkExist(part)) {
                                linktosend = _communicationService.GetCampaignLink("Sms", part);
                            }
                        }
                        string messageToSms = part.Message + " " + linktosend;

                        // Invio SMS
                        _smsServices.SendSms(listaNumeri.Select(x => Convert.ToInt64(x)).ToArray(),
                                             messageToSms, part.Alias, part.Id.ToString(), part.HaveAlias);

                        part.SmsMessageSent = true;
                    }
                }
            });



        }

    }
}