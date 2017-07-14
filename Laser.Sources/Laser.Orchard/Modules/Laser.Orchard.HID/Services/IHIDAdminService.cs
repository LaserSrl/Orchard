using Laser.Orchard.HID.Models;
using Orchard;

namespace Laser.Orchard.HID.Services {
    public interface IHIDAdminService : IDependency {
        HIDSiteSettingsPart GetSiteSettings();
    }
}
