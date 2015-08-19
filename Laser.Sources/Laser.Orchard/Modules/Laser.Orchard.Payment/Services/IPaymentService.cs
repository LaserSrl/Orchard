using Laser.Orchard.Payment.Models;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Payment.Services {
    public interface IPaymentService : IDependency {
        #region GestPay
        string GestPayAvviaTransazione(Transazione paymentToCrypt);
        TransazioneRicevuta GestPayRiceviTranzazioneS2S(string a, string b);
        #endregion
    }
}