using Laser.Orchard.HID.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.HID.Handlers {
    public class PartNumberSetsUserPartHandler : ContentHandler {

        public PartNumberSetsUserPartHandler(
            IRepository<PartNumberSetsUserPartRecord> repository) {

            Filters.Add(StorageFilter.For(repository));

            // sneakily attach the part to users
            Filters.Add(new ActivatingFilter<PartNumberSetsUserPart>("User"));
        }

    }
}