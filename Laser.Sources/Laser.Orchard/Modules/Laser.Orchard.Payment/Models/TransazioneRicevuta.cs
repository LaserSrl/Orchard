using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Payment.Models {
    public class TransazioneRicevuta {
        public ListaCodiciDivisa CodiceDivisa { get; set; }
        public int CodiceDivisaCorrect { get { return (int)CodiceDivisa; } }
        [StringLength(30)]
        public string ShopLogin { get; set; }

        public double Amount { get; set; }
        public string AmountCorrect {
            get {
                return Amount.ToString(CultureInfo.CreateSpecificCulture("it-IT")).Replace(".", string.Empty).Replace(',', '.');
            }
        }
        [StringLength(50)]
        public string ShopTransactionID { get; set; }

        //  [StringLength(20)]
        //public string CardNumber { get; set; }
        //public int ExpMonth { get; set; }
        //public int ExpYear { get; set; }
        [StringLength(50)]
        public string BuyerName { get; set; }
        [StringLength(50)]
        public string BuyerEmail { get; set; }


        //public int Language { get; set; }
       // [StringLength(1000)]
        public Dictionary<string,string> CustomInfo {get;set;}
        //    get {
        //        if (CustomInfoGenerate == null)
        //            return null;
        //        else
        //            return string.Join("*P1*", CustomInfoGenerate.Select(x => x.Key + "=" + x.Value).ToArray());
        //    }
        //}
        //public Dictionary<string, string> CustomInfoGenerate { get; set; }

        #region [added info]
        [StringLength(2)]
        public string TransactionResult { get; set; }
        [StringLength(6)]
        public string AuthorizationCode { get; set; }

        public int BankTransactionID { get; set; }
        [StringLength(30)]
        public string Country { get; set; }
        [StringLength(50)]
        public string VbV { get; set; }

        public int ErrorCode { get; set; }
        [StringLength(255)]
        public string ErrorDescription { get; set; }
        public int AlertCode { get; set; }
        [StringLength(255)]
        public string AlertDescription { get; set; }
        [StringLength(255)]
        public string Level3D { get; set; }


        #endregion

        //public string ExpMonthCorrect {
        //    get {
        //        if (ExpMonth >= 1 && ExpMonth <= 12)
        //            return ExpMonth.ToString().PadLeft(2, '0');
        //        return "";
        //    }
        //}
        //public string ExpYearCorrect {
        //    get {
        //        if (ExpYear >= 0 && ExpYear <= 99)
        //            return ExpYear.ToString().PadLeft(2, '0');
        //        return "";
        //    }
        //}
        public bool valid {
            get {

                #region [campi minimi richiesti]
                if (CodiceDivisaCorrect == 0 || Amount <= 0 || string.IsNullOrEmpty(ShopTransactionID) || string.IsNullOrEmpty(TransactionResult) || string.IsNullOrEmpty(AuthorizationCode) || BankTransactionID == 0)
                    return false;
                #endregion

                #region [caratteri/stringhe non permessi in CustomInfo]
                //var StringheNonValide = new List<string> { "&", " ", "§", "(", ")", "*", "<", ">", ",", ";", ":", "*P1*", "/", "[", "]", "?", "=", "__", "/*", "%", "//" };
                //if (!string.IsNullOrEmpty(CustomInfo)) {
                //    var custominfoparameter = CustomInfo.Split(new string[] { "*P1*","=" }, StringSplitOptions.None);
                //    foreach (var singleparameter in custominfoparameter)
                //        foreach (var carattere in StringheNonValide) {
                //            if (singleparameter.IndexOf(carattere) > 0)
                //                return false;
                //        }
                //}
                #endregion

                #region [Controllo sui limiti dei campi]

                if (Amount > 999999999 / 100) //9 cifre
                    return false;
                //if  (ExpMonth>13 || ExpMonth<0) 
                //    return false;
                //if (ExpYear > 99 || ExpYear < 0)
                //    return false;
                //if (Language > 99) // 2 cifre
                //    return false;
                #endregion

                return true;
            }
        }
    }
}