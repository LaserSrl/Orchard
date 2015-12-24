using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Laser.Orchard.SEO.Models {

    public class SeoPart : ContentPart<SeoVersionRecord> {

        public string TitleOverride {
            get { return this.Retrieve(x => x.TitleOverride); }
            set { this.Store(x => x.TitleOverride, value); }
        }

        public string Keywords {
            get { return this.Retrieve(x => x.Keywords); }
            set { this.Store(x => x.Keywords, value); }
        }

        public string Description {
            get { return this.Retrieve(x => x.Description); }
            set { this.Store(x => x.Description, value); }
        }

    }
}