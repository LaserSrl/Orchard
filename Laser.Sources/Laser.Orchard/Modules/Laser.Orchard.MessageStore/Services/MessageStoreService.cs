using Laser.Orchard.MessageStore.Models;
using Laser.Orchard.MessageStore.ViewModels;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using AutoMapper;
using Orchard.Messaging.Services;
using Orchard;

namespace Laser.Orchard.MessageStore.Services {
    public class MessageStoreService : IMessageStoreService {

        private readonly IContentManager _contentManager;
        private readonly IMessageManager _messageManager;
        private readonly IOrchardServices _orchardServices;

        public MessageStoreService(
            IContentManager contentManager,
            IMessageManager messageManager,
             IOrchardServices orchardServices) {
            _contentManager = contentManager;
            _messageManager = messageManager;
            _orchardServices = orchardServices;
        }


        public List<MessageStoreEditModel> GetMessagesToRead() {
            var messages = (_contentManager.Query<MessageStorePart, MessageStorePartRecord>().Where(x => x.MarkRead==true).List());
            Mapper.CreateMap<MessageStorePartRecord, MessageStoreEditModel>();
            List<MessageStoreEditModel> listMessages = new List<MessageStoreEditModel>();
            foreach (var message in messages) {
                var med = new MessageStoreEditModel();
                Mapper.Map(message.Record, med);
                listMessages.Add(med);
            }
            return (listMessages);
        }

        public List<MessageStoreEditModel> GetMessages(int Groupid, string filterstring = "") {
            filterstring = string.IsNullOrEmpty(filterstring) ? "" : filterstring;
            var messages = (_contentManager.Query<MessageStorePart, MessageStorePartRecord>().Where(x => x.Gruppoid == Groupid && x.FilterTest == filterstring).OrderByDescending(y => y.MessageDate).List());
            Mapper.CreateMap<MessageStorePartRecord, MessageStoreEditModel>();
            List<MessageStoreEditModel> listMessages = new List<MessageStoreEditModel>();
            foreach (var message in messages) {
                var med = new MessageStoreEditModel();
                Mapper.Map(message.Record, med);
                listMessages.Add(med);
            }

            return (listMessages);
        }

        public ContentItem Create(MessageStoreEditModel messagetostore) {
            if (messagetostore == null) {
                throw new ArgumentNullException("MessageStoreEditModel");
            } else {
                var newItem = _contentManager.New("MessageStore");
                var OwnPart = newItem.As<MessageStorePart>();
                OwnPart.FilterTest = messagetostore.FilterTest;
                OwnPart.Gruppoid = messagetostore.Gruppoid;
                OwnPart.MarkMessage = messagetostore.MarkMessage;
                OwnPart.MarkRead = messagetostore.MarkRead;
                OwnPart.MessageDate = messagetostore.MessageDate;
                OwnPart.MessageFrom = messagetostore.MessageFrom;
                OwnPart.MessageTo = messagetostore.MessageTo;
                OwnPart.MessageText = messagetostore.MessageText;
                OwnPart.MessageTextHtml = messagetostore.MessageTextHtml;
                OwnPart.MessageObject = messagetostore.MessageObject;
                _contentManager.Create(newItem);
                if (messagetostore.Send) {
                    Send(messagetostore);
                    //  var data = new Dictionary<string, string>();
                    //  data.Add("Subject", messagetostore.MessageObject);
                    //  data.Add("Body", messagetostore.MessageTextHtml);
                    //  data.Add("From", messagetostore.MessageFrom);
                    ////  data.Add("To","lorenzo.frediani@gmail.com");
                    //  var recipient = new List<string> { 
                    //    //  "marco.viglione.laser@gmail.com",
                    //     // "marco.viglione@laser-group.com",
                    //      "lorenzo.frediani@gmail.com",
                    //      "lorenzo.frediani@Laser-group.com"
                    //  };




                    //  _messageManager.Send( recipient, "ActionEmail", "email", data);
                }
                return newItem;
            }
        }

        public bool Send(MessageStoreEditModel messagetostore) {
            if (messagetostore == null) {
                throw new ArgumentNullException("MessageStoreEditModel");
            } else {
                var data = new Dictionary<string, string>();
                data.Add("Subject", messagetostore.MessageObject);
                data.Add("Body", messagetostore.MessageTextHtml);
         //     //  data.Add("Body", "<html><body><img src='http://www.tickartitaly.com/LOGO ticket art italy sfondo chiaro.png' title='Ticket Art Italy' alt='Ticket Art Italy'>Salve, i miei piu cordiali saluti</body></html>");
         ////       data.Add("From", messagetostore.MessageFrom);
                string elencoAttachment = "";
                if (messagetostore.Attachment != null)
                    foreach (string path in messagetostore.Attachment) {
                        elencoAttachment += path + "||";
                    }
                if (elencoAttachment != "")
                    data.Add("Attachment", elencoAttachment);
                var recipient = new List<string> { 
                    messagetostore.MessageTo
                    };




                _messageManager.Send(recipient, "ActionEmail", "email", data);
                //    }
                return true;
            }
        }

    }
}