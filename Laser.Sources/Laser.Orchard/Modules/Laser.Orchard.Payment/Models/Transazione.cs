using Laser.Orchard.Payment.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Payment.Models {
    public enum ListaCodiciDivisa { USD = 1, GBP = 2, CHF = 3, DKK = 7, NOK = 8, SEK = 9, CAD = 12, JPY = 71, HKD = 103, BRL = 234, EUR = 242 };
    public enum ListaCodiciLingua { Italiano = 1, Inglese = 2, Spagnolo = 3, Francese = 4, Tedesco = 5 };
    public class Transazione {
        public ListaCodiciDivisa CodiceDivisa { get; set; }

        public int CodiceDivisaCorrect { get { return (int)CodiceDivisa; } }
        [StringLength(30)]
        public string ShopLogin { get; set; }

        public double Amount { get; set; }
        [StringLength(50)]
        public string ShopTransactionID { get; set; }
        [StringLength(20)]
        public string CardNumber { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        [StringLength(50)]
        public string BuyerName { get; set; }
        [StringLength(50)]
        public string BuyerEmail { get; set; }
        public ListaCodiciLingua Language { get; set; }
        public string LanguageCorrect {
            get {
                if ((int)Language == 0)
                    return "";
                else
                    return ((int)Language).ToString();
            }
        }

        [StringLength(1000)]
        public string CustomInfo {
            get {
                if (CustomInfoGenerate == null)
                    return null;
                else
                    return string.Join("*P1*", CustomInfoGenerate.Select(x => x.Key + "=" + x.Value).ToArray());
            }
        }
        public Dictionary<string, string> CustomInfoGenerate { get; set; }
        public string AmountCorrect {
            get {
                return Amount.ToString(CultureInfo.CreateSpecificCulture("it-IT")).Replace(".", string.Empty).Replace(',', '.');
                // return string.Format("{0:N2}", Amount);//.ToString("#.##");
            }
        }
        public string ExpMonthCorrect {
            get {

                if (ExpMonth >= 1 && ExpMonth <= 12)
                    return ExpMonth.ToString().PadLeft(2, '0');
                return "";
            }
        }
        public string ExpYearCorrect {
            get {
                if (ExpYear >= 0 && ExpYear <= 99)
                    return ExpYear.ToString().PadLeft(2, '0');
                return "";
            }
        }
        public bool valid {
            get {


                #region [campi minimi richiesti]
                if (CodiceDivisaCorrect == 0 || Amount <= 0 || string.IsNullOrEmpty(ShopTransactionID))
                    return false;
                #endregion

                #region [caratteri/stringhe non permessi in CustomInfo]
                var StringheNonValide = new List<string> { "&", " ", "§", "(", ")", "*", "<", ">", ",", ";", ":", "*P1*", "/", "[", "]", "?", "=", "__", "/*", "%", "//" };
                if (!string.IsNullOrEmpty(CustomInfo)) {
                    var custominfoparameter = CustomInfo.Split(new string[] { "*P1*", "=" }, StringSplitOptions.None);
                    foreach (var singleparameter in custominfoparameter)
                        foreach (var carattere in StringheNonValide) {
                            if (singleparameter.IndexOf(carattere) > 0)
                                return false;
                        }
                }
                #endregion

                #region [Controllo sui limiti dei campi]

                if (Amount > 999999999 / 100) //9 cifre
                    return false;
                if (ExpMonth > 13 || ExpMonth < 0)
                    return false;
                if (ExpYear > 99 || ExpYear < 0)
                    return false;

                #endregion

                return true;
            }
        }
        public string StringToClean { get; set; }
        public string StringCleaned {
            get {
                var tmp = StringToClean;
                var StringheNonValide = new List<string> { "&", " ", "§", "(", ")", "*", "<", ">", ",", ";", ":", "*P1*", "/", "[", "]", "?", "=", "__", "/*", "%", "//" };
                foreach (var singleparameter in StringheNonValide)
                    tmp = tmp.Replace(singleparameter, "!");
                return tmp;
            }
        }

    }


}