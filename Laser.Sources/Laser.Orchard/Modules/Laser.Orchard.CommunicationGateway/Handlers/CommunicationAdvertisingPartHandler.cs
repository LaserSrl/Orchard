using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.CommunicationGateway.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.CommunicationGateway.Handlers {
    public class CommunicationAdvertisingPartHandler : ContentHandler{
        public CommunicationAdvertisingPartHandler(IRepository<CommunicationAdvertisingPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}