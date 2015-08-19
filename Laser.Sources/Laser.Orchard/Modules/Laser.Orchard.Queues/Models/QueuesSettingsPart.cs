using Orchard.ContentManagement;

namespace Laser.Orchard.Queues.Models
{
    public class QueuesSettingsPart : ContentPart
    {
        public string EndpointUrl
        {
            get { return this.Retrieve(x => x.EndpointUrl); }
            set { this.Store(x => x.EndpointUrl, value); }
        }

        public int PollingInterval
        {
            get { return this.Retrieve(x => x.PollingInterval); }
            set { this.Store(x => x.PollingInterval, value); }
        }

        public int MaxPushToSend
        {
            get { return this.Retrieve(x => x.MaxPushToSend); }
            set { this.Store(x => x.MaxPushToSend, value); }
        }
    }
}