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
            get {
                if (this.As<WidgetPart>() != null) {
                    return this.As<WidgetPart>().Zone;
                } else { 
                    return ""; 
                }
            }
        }

        public string Position {
            get {
                if (this.As<WidgetPart>() != null) {
                    return this.As<WidgetPart>().Position;
                } else {
                    return "";
                }
            }
        }
    }

    public class WidgetExPartRecord : ContentPartRecord {
        public virtual int? HostId { get; set; }
    }
}