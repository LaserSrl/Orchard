﻿using Laser.Orchard.CommunicationGateway.Services;
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
using Orchard.UI.Admin;

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

        protected override DriverResult Display(SmsGatewayPart part, string displayType, dynamic shapeHelper) {
            //Determine if we're on an admin page
            bool isAdmin = AdminFilter.IsApplied(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            if (isAdmin) {
                if (displayType == "Summary")
                    return ContentShape("Parts_SmsGateway_SummaryAdmin",
                        () => shapeHelper.Parts_SmsGateway_SummaryAdmin(SmsMessageSent: part.SmsMessageSent, SendOnNextPublish: part.SendOnNextPublish, RecipientsNumber: part.SmsRecipientsNumber, SmsDeliveredOrAcceptedNumber: part.SmsDeliveredOrAcceptedNumber, SmsRejectedOrExpiredNumber: part.SmsRejectedOrExpiredNumber));
                if (displayType == "SummaryAdmin")
                    return ContentShape("Parts_SmsGateway_SummaryAdmin",
                        () => shapeHelper.Parts_SmsGateway_SummaryAdmin(SmsMessageSent: part.SmsMessageSent, SendOnNextPublish: part.SendOnNextPublish, RecipientsNumber: part.SmsRecipientsNumber, SmsDeliveredOrAcceptedNumber: part.SmsDeliveredOrAcceptedNumber, SmsRejectedOrExpiredNumber: part.SmsRejectedOrExpiredNumber));
                return null;
            } else {
                return null;
            }

        }


        // GET
        protected override DriverResult Editor(SmsGatewayPart part, dynamic shapeHelper) {

            var smsPlaceholdersSettingsPart = _orchardServices.WorkContext.CurrentSite.As<SmsPlaceholdersSettingsPart>();
            var smsSettingsPart = _orchardServices.WorkContext.CurrentSite.As<SmsSettingsPart>();

            // Controllo se conteggiare lo shortilink all'interno del messaggio
            bool shortlinkExist = false;

            ICommunicationService _communicationService;
            bool tryed = _orchardServices.WorkContext.TryResolve<ICommunicationService>(out _communicationService);
            if (tryed) {
                shortlinkExist = _communicationService.CampaignLinkExist(part);
            }

            // Tolgo 16 caratteri necessari per lo shortlink
            int MaxLenght = smsSettingsPart.MaxLenghtSms;
            if (shortlinkExist) {
                MaxLenght = MaxLenght - 16;
            }

            if (!smsSettingsPart.MamHaveAlias) {
                part.HaveAlias = false;
            }

            List<string> ListaAlias = null;
            if (smsSettingsPart.SmsFrom != "") {
                ListaAlias = new List<string>(smsSettingsPart.SmsFrom.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            }

            var model = new SmsGatewayVM {
                Protocollo = smsSettingsPart.Protocollo,
                AliasList = ListaAlias,
                MaxLenghtSms = MaxLenght,
                SmsGateway = part,
                Settings = smsPlaceholdersSettingsPart,
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