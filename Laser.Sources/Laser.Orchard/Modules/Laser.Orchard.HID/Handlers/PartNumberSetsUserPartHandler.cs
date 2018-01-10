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

            // LazyField loader
            //OnLoading<PartNumberSetsUserPart>((ctx, part) => LazyLoadHandlers(part));
            //OnVersioning<PartNumberSetsUserPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));
            OnUpdating<PartNumberSetsUserPart>(MyMethod1);
            OnUpdated<PartNumberSetsUserPart>(MyMethod1);
        }

        public void MyMethod1(UpdateContentContext context, PartNumberSetsUserPart part) {

            var a = 7;
            for (int i = 0; i < a; i++) {

            }
        }

        //void LazyLoadHandlers(PartNumberSetsUserPart part) {

        //    part.PartNumberSetsField.Loader(() => {
        //        if (part.Record.PartNumberSetsJR != null && part.Record.PartNumberSetsJR.Any()) {
        //            return part.Record.PartNumberSetsJR.Select(jr => jr.HIDPartNumberSet);
        //        }
        //        return Enumerable.Empty<HIDPartNumberSet>();
        //    });
        //}

    }
}