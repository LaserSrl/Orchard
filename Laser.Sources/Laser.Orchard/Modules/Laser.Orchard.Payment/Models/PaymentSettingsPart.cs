using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Payment.Models {
    public class PaymentSettingsPart : ContentPart<PaymentSettingsPartRecord> {
        public string GestpayShopLogin {
            get { return Record.GestpayShopLogin; }
            set { Record.GestpayShopLogin = value; }
        }
        public bool GestpayTest {
            get { return Record.GestpayTest; }
            set { Record.GestpayTest = value; }
        }
        public string PaymentMethodSelected {
            get { return Record.PaymentMethodSelected; }
            set { Record.PaymentMethodSelected = value; }
        }
    }

    public class PaymentSettingsPartRecord : ContentPartRecord {
        public virtual string GestpayShopLogin { get; set; }
        public virtual bool GestpayTest { get; set; }
        public virtual string PaymentMethodSelected { get; set; }
    }
}