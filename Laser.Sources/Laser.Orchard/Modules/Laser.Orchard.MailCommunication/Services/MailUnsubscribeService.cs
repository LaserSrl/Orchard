using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MailCommunication.Services {

    public interface IMailUnsubscribeService : IDependency {
        bool SendMailConfirmUnsubscribe(string mail);
    }

    public class MailUnsubscribeService : IMailUnsubscribeService {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<CommunicationEmailRecord> _Emailrepository;
        private readonly INotifier _notifier;
        private readonly ICommonsServices _commonServices;

        public Localizer T { get; set; }

        public MailUnsubscribeService(IOrchardServices orchardServices, IRepository<CommunicationEmailRecord> Emailrepository, INotifier notifier,
                                      ICommonsServices commonServices) {
            _orchardServices = orchardServices;
            _Emailrepository = Emailrepository;
            _notifier = notifier;
            _commonServices = commonServices;

            T = NullLocalizer.Instance;
        }


        public bool SendMailConfirmUnsubscribe(string Email) {
            // Controllo che la mail inserita sia presente tra i contatti
            List<CommunicationEmailRecord> lista = (from m in _Emailrepository.Table
                                                    where m.Email.Equals(Email)
                                                    select m).ToList();

            if (lista == null) {
                _orchardServices.Notifier.Information(T("Email not found!"));
                return false;
            }

            // Create Nonce
            string parametri = "Email=" + Email + "Guid=" + new Guid().ToString();
            TimeSpan delay = new TimeSpan(1, 0, 0);
            string Nonce = _commonServices.CreateNonce(parametri, delay);

            // Memorizzo Nonce su CommunicationEmailRecord
            foreach (CommunicationEmailRecord recordMail in lista) {
                recordMail.KeyUnsubscribe = Nonce;
                _Emailrepository.Update(recordMail);
            }







            // 2 - Inviare mail

            //_templateService.SendTemplatedEmail(

            //                     dynamic contentModel,  -> mail, link, parametri
            //                     int templateId,  -> recuperare dai settings
            //                     IEnumerable<string> sendTo, -> null
            //                     IEnumerable<string> bcc, -> mail
            //                     object viewBag = null, -> null
            //                     bool queued = true, -> false
            //                     List<TemplatePlaceHolderViewModel> listaPH = null

            return true;
        }




    }
}