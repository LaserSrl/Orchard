using Laser.Orchard.Claims.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Handlers {
    public class IdentityClaimsPartHandler : ContentHandler {
        public IdentityClaimsPartHandler(IRepository<IdentityClaimsPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}