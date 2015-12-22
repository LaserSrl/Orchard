using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Laser.Orchard.CommunicationGateway.ViewModels {
    public class ContentIndexVM {
        public Int32 Id { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public DateTime? ModifiedUtc { get; set; }
        public dynamic Option { get; set; }
        public ContentItem ContentItem { get; set; }
    }
}