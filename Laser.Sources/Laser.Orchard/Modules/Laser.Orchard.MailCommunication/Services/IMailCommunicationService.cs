using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Laser.Orchard.CommunicationGateway.Models;
using Orchard;
using Orchard.ContentManagement;

namespace Laser.Orchard.Services.MailCommunication {
    public interface IMailCommunicationService : IDependency {
        IHqlQuery IntegrateAdditionalConditions(IHqlQuery query = null);
    }

    public class DefaultMailCommunicationService : IMailCommunicationService {
        private readonly IOrchardServices _orchardServices;
        public DefaultMailCommunicationService(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public IHqlQuery IntegrateAdditionalConditions(IHqlQuery query = null) {
            if (query == null) {
                query = _orchardServices.ContentManager.HqlQuery().ForType(new string[] { "CommunicationContact" });
            }
            return query
                .Where(x => x.ContentPartRecord<EmailContactPartRecord>(), x => x.IsNotEmpty("EmailRecord"));
        }
    }

}
