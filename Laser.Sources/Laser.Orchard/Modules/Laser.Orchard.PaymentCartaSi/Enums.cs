using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.PaymentCartaSi {
    //TODO: implement CodeTables class with lazy dictionaries like it was done for GestPay
    public enum LanguageIdCode { ITA, ENG, SPA, FRA, GER, JPG, CHI, ARA, RUS}
    public enum CardType { VISA, MasterCard, Amex, Diners, Jcb, Maestro, MYBANK, SCT, SDD, MYSI }
}