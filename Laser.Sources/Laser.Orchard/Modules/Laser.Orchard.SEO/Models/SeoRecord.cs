using Orchard.ContentManagement.Records;

namespace Laser.Orchard.SEO.Models {

  public class SeoRecord : ContentPartRecord {
    public virtual string TitleOverride { get; set; }
    public virtual string Keywords { get; set; }
    public virtual string Description { get; set; }
  }
}