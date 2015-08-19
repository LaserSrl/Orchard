using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Utilities;
using Orchard.Widgets.Models;

namespace Contrib.Widgets.Models {
    public class WidgetExPart : ContentPart<WidgetExPartRecord> {
        internal LazyField<ContentItem> HostField = new LazyField<ContentItem>();

        public ContentItem Host {
            get { return HostField.Value; }
            set { HostField.Value = value; }
        }

        public string Zone {
            get { return this.As<WidgetPart>().Zone; }
        }

        public string Position {
            get { return this.As<WidgetPart>().Position; }
        }
    }

    public class WidgetExPartRecord : ContentPartRecord {
        public virtual int? HostId { get;set; }
    }
}