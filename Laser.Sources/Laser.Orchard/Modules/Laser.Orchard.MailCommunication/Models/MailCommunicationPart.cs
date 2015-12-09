using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Laser.Orchard.MailCommunication.Models {
    public class MailCommunicationPart : ContentPart {
        public bool MailMessageSent {
            get { return this.Retrieve(r => r.MailMessageSent); }
            set { this.Store(r => r.MailMessageSent, value); }
        }
        public bool SendOnNextPublish {
            get { return this.Retrieve(r => r.SendOnNextPublish); }
            set { this.Store(r => r.SendOnNextPublish, value); }
        }

        public bool SendToTestEmail {
            get { return this.Retrieve(r => r.SendToTestEmail); }
            set { this.Store(r => r.SendToTestEmail, value); }
        }

        public string EmailForTest {
            get { return this.Retrieve(r => r.EmailForTest); }
            set { this.Store(r => r.EmailForTest, value); }
        }

    }
}