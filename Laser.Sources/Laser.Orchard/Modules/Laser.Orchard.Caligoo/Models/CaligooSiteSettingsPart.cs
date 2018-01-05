using Orchard.ContentManagement;

namespace Laser.Orchard.Caligoo.Models {
    public class CaligooSiteSettingsPart : ContentPart {
        public string BaseUrl {
            get { return this.Retrieve(x => x.BaseUrl); }
            set { this.Store(x => x.BaseUrl, value); }
        }
        public string LoginPath {
            get {
                var aux = this.Retrieve(x => x.LoginPath);
                if (string.IsNullOrWhiteSpace(aux)) {
                    aux = "jwt"; // default value
                }
                return aux;
            }
            set { this.Store(x => x.LoginPath, value); }
        }
        public string RefreshPath {
            get {
                var aux = this.Retrieve(x => x.RefreshPath);
                if (string.IsNullOrWhiteSpace(aux)) {
                    aux = "refresh"; // default value
                }
                return aux;
            }
            set { this.Store(x => x.RefreshPath, value); }
        }
        public string LocationsPath {
            get {
                var aux = this.Retrieve(x => x.LocationsPath);
                if (string.IsNullOrWhiteSpace(aux)) {
                    aux = "locations"; // default value
                }
                return aux;
            }
            set { this.Store(x => x.LocationsPath, value); }
        }
        public string UsersPath {
            get {
                var aux = this.Retrieve(x => x.UsersPath);
                if (string.IsNullOrWhiteSpace(aux)) {
                    aux = "users"; // default value
                }
                return aux;
            }
            set { this.Store(x => x.UsersPath, value); }
        }
        /// <summary>
        /// Timeout in milliseconds during requests to Caligoo Web API.
        /// </summary>
        public int RequestTimeoutMillis {
            get {
                var aux = this.Retrieve(x => x.RequestTimeoutMillis);
                if(aux == 0) {
                    aux = 10000; // default value
                }
                return aux;
            }
            set { this.Store(x => x.RequestTimeoutMillis, value); }
        }
        public string Username {
            get { return this.Retrieve(x => x.Username); }
            set { this.Store(x => x.Username, value); }
        }
        public string Password {
            get { return this.Retrieve(x => x.Password); }
            set { this.Store(x => x.Password, value); }
        }
    }
}