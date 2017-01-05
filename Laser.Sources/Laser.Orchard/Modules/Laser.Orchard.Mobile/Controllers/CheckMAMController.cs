using Laser.Orchard.Mobile.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.Mobile.Controllers {
    public class CheckMAMController : ApiController {

        private readonly ISmsServices _smsServices;

        public CheckMAMController(ISmsServices smsServices) {
            _smsServices = smsServices;
        }

        /// <summary>
        /// Metodo per ritornare lo stato della MAM - 0 NOT OK / 1 OK
        /// http://localhost/Laser.Orchard/Sviluppo/Api/Laser.Orchard.Mobile/CheckMAM
        /// </summary>
        /// <param name="Language"></param>
        /// <returns></returns>
        public int Get() {
            return _smsServices.GetStatus();
        }

        public void Post() { }

        public void Put() { }

        public void Delete() { }
    }
}