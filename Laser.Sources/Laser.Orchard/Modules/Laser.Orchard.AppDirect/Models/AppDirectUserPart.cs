
using Orchard.ContentManagement;

namespace Laser.Orchard.AppDirect.Models {
    public class AppDirectUserPart : ContentPart<AppDirectUserPartRecord> {
        public string Email {
            get
            {
                return this.Retrieve(r => r.Email);
            }
            set
            {
                this.Store(r => r.Email, value);
            }
        }


        public string FirstName {
            get
            {
                return this.Retrieve<string>(r => r.FirstName);
            }
            set
            {
                this.Store(r => r.FirstName, value);
            }
        }

        public string Language {
            get
            {
                return this.Retrieve(r => r.Language);
            }
            set
            {
                this.Store(r => r.Language, value);
            }
        }

        public string LastName {
            get
            {
                return this.Retrieve(r => r.LastName);
            }
            set
            {
                this.Store(r => r.LastName, value);
            }
        }

        public string Locale {
            get
            {
                return this.Retrieve(r => r.Locale);
            }
            set
            {
                this.Store(r => r.Locale, value);
            }
        }

        public string OpenIdCreator {
            get
            {
                return this.Retrieve(r => r.OpenIdCreator);
            }
            set
            {
                this.Store(r => r.OpenIdCreator, value);
            }
        }

        public string UuidCreator {
            get
            {
                return this.Retrieve(r => r.UuidCreator);
            }
            set
            {
                this.Store(r => r.UuidCreator, value);
            }
        }

        public string AccountIdentifier {
            get
            {
                return this.Retrieve(r => r.AccountIdentifier);
            }
            set
            {
                this.Store(r => r.AccountIdentifier, value);
            }
        }
        

        public string CompanyWebSite {
            get
            {
                return this.Retrieve(r => r.CompanyWebSite);
            }
            set
            {
                this.Store(r => r.CompanyWebSite, value);
            }
        }

        public string CompanyUuidCreator {
            get
            {
                return this.Retrieve(r => r.CompanyUuidCreator);
            }
            set
            {
                this.Store(r => r.CompanyUuidCreator, value);
            }
        }

        public string CompanyName {
            get
            {
                return this.Retrieve(r => r.CompanyName);
            }
            set
            {
                this.Store(r => r.CompanyName, value);
            }
        }

        public string CompanyCountry {
            get
            {
                return this.Retrieve(r => r.CompanyCountry);
            }
            set
            {
                this.Store(r => r.CompanyCountry, value);
            }
        }

    }
}
