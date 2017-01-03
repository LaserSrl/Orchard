using Laser.Orchard.HID.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Laser.Orchard.HID.Handlers {
    public class HIDSettingsPartHandler : ContentHandler {

        public HIDSettingsPartHandler(){
            Filters.Add(new ActivatingFilter<HIDSiteSettingsPart>("Site"));
        }
    }
}