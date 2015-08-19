using Laser.Orchard.MessageStore.Models;
using Laser.Orchard.MessageStore.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.MessageStore.Services {
    public interface IMessageStoreService : IDependency {
        /// <summary>
        /// Elenco dei Messaggi
        /// </summary>
        /// <param name="Groupid">Required</param>
        /// <param name="filterstring">Se non esplicitato verrà ricercato per Groupid e filterstring=""</param>
        /// <returns></returns>
        List<MessageStoreEditModel> GetMessages(int Groupid, string filterstring = "");
        List<MessageStoreEditModel> GetMessagesToRead();
        ContentItem Create(MessageStoreEditModel messagetostore);
        bool Send(MessageStoreEditModel messagetostore);
    }
}