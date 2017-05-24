using Laser.Orchard.Claims.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Claims.Handlers {
    public class RequiredClaimsPartHandler : ContentHandler {
        public RequiredClaimsPartHandler(IRepository<RequiredClaimsPartRecord> repository, ITokenizer tokenizer) {
            Filters.Add(StorageFilter.For(repository));
            OnUpdated<RequiredClaimsPart>((ctx, part) => {
                part.Claims = tokenizer.Replace(part.Claims, part.ContentItem);
            });
        }
    }
}