using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.Mobile.Models;
using Laser.Orchard.Mobile.Services;
using Laser.Orchard.Mobile.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;

namespace Laser.Orchard.Mobile.Drivers {

    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SmsGatewayPartDriver : ContentPartDriver<SmsGatewayPart> {

        private readonly ISmsServices _smsServices;
        private readonly IOrchardServices _orchardServices;

        public SmsGatewayPartDriver(ISmsServices smsServices, IOrchardServices orchardServices) {
            _smsServices = smsServices;
            _orchardServices = orchardServices;
        }

        protected override string Prefix {
            get { return "Laser.Orchard.SmsGateway"; }
        }

        // GET
        protected override DriverResult Editor(SmsGatewayPart part, dynamic shapeHelper) {

            const int MSG_MAX_CHAR_NUMBER_SINGOLO = 160;
            const int MSG_MAX_CHAR_NUMBER_CONCATENATI = 1530;

            SmsServiceReference.Config SmsConfig = _smsServices.GetConfig();

            // Controllo se conteggiare lo shortilink all'interno del messaggio
            bool shortlinkExist = false;

            ICommunicationService _communicationService;
            bool tryed = _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
            if (tryed) {
                shortlinkExist = _communicationService.CampaignLinkExist(part);
            }

            // Dimensione massima caratteri
            int MaxLenght = MSG_MAX_CHAR_NUMBER_SINGOLO;
            if (SmsConfig.MaxLenghtSms > 1) {
                MaxLenght = MSG_MAX_CHAR_NUMBER_CONCATENATI;
            }

            // Tolgo 16 caratteri necessari per lo shortlink
            if (shortlinkExist) {
                MaxLenght = MaxLenght - 16;
            }

            var model = new SmsGatewayVM {
                Protocollo = SmsConfig.Protocollo,
                AliasList = SmsConfig.ListaAlias,
                Settings = smsPlaceholdersSettingsPart,
                MaxLenghtSms = MaxLenght,
                SmsGateway = part,
                ShortlinkExist = shortlinkExist
            };

            return ContentShape("Parts_SmsGateway_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/SmsGateway_Edit", Model: model, Prefix: Prefix));
        }

        // POST
        protected override DriverResult Editor(SmsGatewayPart part, IUpdateModel updater, dynamic shapeHelper) {

            var model = new SmsGatewayVM {
                SmsGateway = part
            };

            updater.TryUpdateModel(model, Prefix, null, new string[] { "Settings" });

            // reset Alias
            if (!part.HaveAlias)
                part.Alias = null;

            return Editor(part, shapeHelper);
        }


    }
}