﻿using Laser.Orchard.CommunicationGateway.Events;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Laser.Orchard.Mobile.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Mobile.Handlers {
    [OrchardFeature("Laser.Orchard.SmsGateway")]
    public class SmsGatewayEventHandler : ICommunicationEventHandler {

        private readonly IOrchardServices _orchardServices;

        public SmsGatewayEventHandler(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public void PopulateChannel(ContentItem ci, AdvertisingVM.Advertising adv) {

            if (adv.SmsGateway != null) {

                var smsSettingsPart = _orchardServices.WorkContext.CurrentSite.As<SmsSettingsPart>();

                ((dynamic)ci).SmsGatewayPart.ExternalId = adv.SmsGateway.Id;
                ((dynamic)ci).SmsGatewayPart.Message = adv.SmsGateway.Text;

                // Aggiungo +39
                string listaDestinatari = "";
                foreach (string tel in adv.SmsGateway.Contacts.PhoneNumbers) {
                    string number = "+39" + tel;
                    listaDestinatari += number + Environment.NewLine;
                }

                ((dynamic)ci).SmsGatewayPart.SendToRecipientList = true;
                ((dynamic)ci).SmsGatewayPart.RecipientList = listaDestinatari;
                ((dynamic)ci).SmsGatewayPart.HaveAlias = smsSettingsPart.MamHaveAlias;

                if (smsSettingsPart.MamHaveAlias) {
                    // La MAM utilizza uno o più Alias
                    List<string> listaAlias = null;
                    if (smsSettingsPart.SmsFrom != "") {
                        listaAlias = new List<string>(smsSettingsPart.SmsFrom.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    // Utilizzo il primo della lista
                    ((dynamic)ci).SmsGatewayPart.Alias = listaAlias[0];
                } 
                else {
                    ((dynamic)ci).SmsGatewayPart.Alias = null;
                }

                // Approved
                ((dynamic)ci).SmsGatewayPart.SendOnNextPublish = true;
            }
        }

    }
}