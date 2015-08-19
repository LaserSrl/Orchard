using Laser.Orchard.IXMSD.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.IXMSD.Handlers {
    public class IXMSDSettingsPartHandler : ContentHandler {
        public Localizer T { get; set; }

        public IXMSDSettingsPartHandler() {

            Filters.Add(new ActivatingFilter<IXMSDSettingsPart>("Site"));
            T = NullLocalizer.Instance;
            OnGetContentItemMetadata<IXMSDSettingsPart>((context, part) => context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("IXMSD"))));
        }

    }
}