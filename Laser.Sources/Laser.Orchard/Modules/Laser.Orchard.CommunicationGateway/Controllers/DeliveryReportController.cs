using Laser.Orchard.CommunicationGateway.Models;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.CommunicationGateway.Controllers {
    public class DeliveryReportController : ApiController {

        private readonly IRepository<CommunicationDeliveryReportRecord> _deliveryReportRepository;

        public DeliveryReportController(IRepository<CommunicationDeliveryReportRecord> deliveryReportRepository) {
            _deliveryReportRepository = deliveryReportRepository;
        }

        /// <summary>
        /// Metodo per ritornare le informazioni sul dettaglio
        /// http://localhost/Laser.Orchard/Sviluppo/Api/CommunicationGateway/Get/{id}
        /// </summary>
        /// <param name="Language"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public List<CommunicationDeliveryReportRecord> Get(int Id) {
            return _deliveryReportRepository.Fetch(x => x.CommunicationAdvertisingPartRecord_Id == Id).ToList();
        }

        /// <summary>
        /// Metodo per ritornare le informazioni sul dettaglio tramite Id Esterno
        /// http://localhost/Laser.Orchard/Sviluppo/Api/CommunicationGateway/GetByExternalId/{id}
        /// </summary>
        /// <param name="Language"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public List<CommunicationDeliveryReportRecord> GetByExternalId(string ExternalId) {
            return _deliveryReportRepository.Fetch(x => x.ExternalId == ExternalId).ToList();
        }

        public void Post() { }

        public void Put() { }

        public void Delete() { }

    }
}