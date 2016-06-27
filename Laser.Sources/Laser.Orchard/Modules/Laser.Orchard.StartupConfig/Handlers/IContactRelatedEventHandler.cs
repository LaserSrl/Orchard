using Orchard.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.StartupConfig.Handlers {
    public interface IContactRelatedEventHandler : IEventHandler {
        void DeviceUpdated();
        void SmsUpdated();
    }
}