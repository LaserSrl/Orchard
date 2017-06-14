using System;
using Laser.Orchard.ContentExtension.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.ContentExtension.Handlers {
    public class DynamicProjectionPartHandler : ContentHandler {
        public DynamicProjectionPartHandler(IRepository<DynamicProjectionPartRecord> dynamicprojectionRepository) {
            Filters.Add(StorageFilter.For(dynamicprojectionRepository));
            OnInitializing<DynamicProjectionPart>((ctx, x) => {
                x.OnAdminMenu = false;
                x.AdminMenuText = String.Empty;
            });
        }
    }
}

