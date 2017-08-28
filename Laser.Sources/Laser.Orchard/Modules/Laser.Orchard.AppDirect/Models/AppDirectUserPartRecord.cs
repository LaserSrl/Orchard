using Orchard.ContentManagement.Records;

namespace Laser.Orchard.AppDirect.Models
{
  public class AppDirectUserPartRecord : ContentPartRecord
  {
    public virtual string CompanyCountry { get; set; }

    public virtual string CompanyName { get; set; }

    public virtual string CompanyUuidCreator { get; set; }

    public virtual string CompanyWebSite { get; set; }

    public virtual string Email { get; set; }

    public virtual string FirstName { get; set; }

    public virtual string Language { get; set; }

    public virtual string LastName { get; set; }

    public virtual string Locale { get; set; }

    public virtual string OpenIdCreator { get; set; }

    public virtual string UuidCreator { get; set; }
        public virtual string AccountIdentifier { get; set; }
        

    }
}
