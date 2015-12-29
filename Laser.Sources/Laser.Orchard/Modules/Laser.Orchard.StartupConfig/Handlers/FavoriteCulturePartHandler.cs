using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.StartupConfig.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class FavoriteCulturePartHandler: ContentHandler {
        public FavoriteCulturePartHandler(IRepository<FavoriteCulturePartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }

    }
}