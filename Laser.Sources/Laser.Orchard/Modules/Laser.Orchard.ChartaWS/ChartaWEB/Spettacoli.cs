using System;
using System.Collections.Generic;
using System.Web;
using log4net;
using ChartaDb.ChartaTableAdapters;
using System.Globalization;

namespace ChartaWEB
{
    public static class Spettacoli
    {
        private static readonly ILog logger = LogManager.GetLogger("Spettacoli");

        public static string ListaSpettacoli  (string  pIdTitolo)
        {
            try
            {
                SpettacoliTableAdapter  objTASpett = new SpettacoliTableAdapter ();
                ChartaDb.Charta.SpettacoliDataTable  objDtSpett;

                if (string.IsNullOrEmpty(pIdTitolo))
                {
                    objDtSpett = objTASpett.GetData(null);
                   
                }
                else
                {
                    objDtSpett = objTASpett.GetData(int.Parse(pIdTitolo));
                }

                string sReturn = "<reply>";
                sReturn += "<Spettacoli>";
                IFormatProvider culture = System.Globalization.CultureInfo.CurrentCulture;
                

                foreach (ChartaDb.Charta.SpettacoliRow  dr in objDtSpett)
                {
                     
                    DateTime dateSpettacolo = DateTime.ParseExact(dr.date, "yyyyMMdd", culture);
                    DateTime timeSpettacolo = DateTime.ParseExact(dr.time, "HH.mm", culture);
                    dateSpettacolo = dateSpettacolo.AddHours(timeSpettacolo.Hour).AddMinutes(timeSpettacolo.Minute);
                  

                    if (dateSpettacolo >= DateTime.Now)
                    {
                        sReturn += "<Spteccolo pcode=\"" + dr.pcode + "\" >";
                        sReturn += "<vcode>" + dr.vcode + "</vcode>";
                        sReturn += "<date>" + Util.ConvertWithANDReplace(dr.date.ToString()) + "</date>";
                        sReturn += "<time>" + Util.ConvertWithANDReplace(dr.time.ToString()) + "</time>";
                        sReturn += "<title>" + Util.ConvertWithANDReplace(dr.title.ToString()) + "</title>";
                        sReturn += "<starttime>" + Util.ConvertWithANDReplace(dr.start_time.ToString()) + "</starttime>";
                        sReturn += "<retired>" + Util.ConvertWithANDReplace(dr.retired.ToString()) + "</retired>";
                        sReturn += "<stato>" + Util.ConvertWithANDReplace(dr.stato.ToString()) + "</stato>";
                        sReturn += "</Spteccolo>";
                    }

                }
                objDtSpett.Dispose();
                objTASpett.Dispose();

                sReturn += "</Spettacoli></reply>";

                return sReturn;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("Spettacoli", "999", ex.Message); 
            }
        }
    }
    

}
