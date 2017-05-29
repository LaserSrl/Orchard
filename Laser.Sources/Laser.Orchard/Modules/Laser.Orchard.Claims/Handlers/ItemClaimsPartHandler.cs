using Laser.Orchard.Claims.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Handlers {
    public class ItemClaimsPartHandler : ContentHandler {
        public ItemClaimsPartHandler(IRepository<ItemClaimsPartRecord> repository, ITokenizer tokenizer) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}