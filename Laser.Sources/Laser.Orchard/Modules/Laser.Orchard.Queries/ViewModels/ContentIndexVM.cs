using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Queries.ViewModels {
    public class ContentIndexVM {
        public Int32 Id { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public DateTime? ModifiedUtc { get; set; }

    }
}