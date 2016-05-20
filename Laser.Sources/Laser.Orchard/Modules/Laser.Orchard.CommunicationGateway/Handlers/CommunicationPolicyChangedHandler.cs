using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.Policy;
using Laser.Orchard.Policy.Events;
using Laser.Orchard.Policy.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using System.Linq;

namespace Laser.Orchard.CommunicationGateway.Handlers {
    public class CommunicationPolicyChangedHandler : IPolicyEventHandler {

        private readonly IContentManager _contentManager;
        private readonly IRepository<CommunicationEmailRecord> _emailRepository;
        private readonly IRepository<CommunicationSmsRecord> _smsRepository;
        private readonly IWorkContextAccessor _workContext;

        public CommunicationPolicyChangedHandler(IContentManager contentManager,
                                                 IRepository<CommunicationEmailRecord> emailRepository,
                                                 IRepository<CommunicationSmsRecord> smsRepository,
                                                 IWorkContextAccessor workContext) {
            _contentManager = contentManager;
            _emailRepository = emailRepository;
            _smsRepository = smsRepository;
            _workContext = workContext;
        }

        public void PolicyChanged(PolicyEventViewModel policyData) {
            try {
                var loggedUser = _workContext.GetContext().CurrentUser;
                if (loggedUser != null) {
                    var contactPart = _contentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(w => w.UserPartRecord_Id == loggedUser.Id).List().FirstOrDefault();

                    if (policyData.policyType == PolicyTypeOptions.CommercialUse) {
                        EmailContactPart emailPart = contactPart.As<EmailContactPart>();
                        if (emailPart != null) {
                            foreach (CommunicationEmailRecord emailContact in emailPart.EmailRecord) {
                                emailContact.AccettatoUsoCommerciale = policyData.accepted;
                                _emailRepository.Update(emailContact);
                            }
                        }

                        SmsContactPart smsPart = contactPart.As<SmsContactPart>();
                        if (smsPart != null) {
                            foreach (CommunicationSmsRecord smsContact in smsPart.SmsRecord) {
                                smsContact.AccettatoUsoCommerciale = policyData.accepted;
                                _smsRepository.Update(smsContact);
                            }
                        }
                    }

                    if (policyData.policyType == PolicyTypeOptions.ThirdParty) {
                        EmailContactPart emailPart = contactPart.As<EmailContactPart>();
                        if (emailPart != null) {
                            foreach (CommunicationEmailRecord emailContact in emailPart.EmailRecord) {
                                emailContact.AutorizzatoTerzeParti = policyData.accepted;
                                _emailRepository.Update(emailContact);
                            }
                        }

                        SmsContactPart smsPart = contactPart.As<SmsContactPart>();
                        if (smsPart != null) {
                            foreach (CommunicationSmsRecord smsContact in smsPart.SmsRecord) {
                                smsContact.AutorizzatoTerzeParti = policyData.accepted;
                                _smsRepository.Update(smsContact);
                            }
                        }
                    }
                }
            }
            catch { }
        }
    }
}