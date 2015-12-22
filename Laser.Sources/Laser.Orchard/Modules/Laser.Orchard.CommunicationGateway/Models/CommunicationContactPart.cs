using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;

namespace Laser.Orchard.CommunicationGateway.Models {

    public class CommunicationContactPart : ContentPart<CommunicationContactPartRecord> {

        public Int32 User_Id {
            get { return this.Retrieve(r => r.User_Id); }
            set { this.Store(r => r.User_Id, value); }
        }
    }

    public class CommunicationContactPartRecord : ContentPartRecord {
        public virtual Int32 User_Id { get; set; }
    }
}