using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Models {
    public class QueryFilterPart:ContentPart {
 
        [DisplayName("QueryId")]
        public Int32 QueryId {
            get { return this.Retrieve(r => r.QueryId); }
            set { this.Store(r => r.QueryId, value); }
        }
    }
}