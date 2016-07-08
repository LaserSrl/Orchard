using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.MailCommunication.ViewModels {
    public class UnsubscribeVM {

        public string Guid { get; set; }
        public string Email { get; set; }
        public string LinkUnsubscription { get; set; }
        public DateTime UnsubscriptionDate { get; set; }

    }
}