using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;

namespace Laser.Orchard.CommunicationGateway.Models {

    public class CommunicationContactPart : ContentPart<CommunicationContactPartRecord> {

        public Int32 UserIdentifier {
            get { return this.Retrieve(r => r.UserPartRecord_Id); }
            set { this.Store(r => r.UserPartRecord_Id, value); }
        }
    }

    public class CommunicationContactPartRecord : ContentPartRecord {
        public virtual Int32 UserPartRecord_Id { get; set; }
    }
}