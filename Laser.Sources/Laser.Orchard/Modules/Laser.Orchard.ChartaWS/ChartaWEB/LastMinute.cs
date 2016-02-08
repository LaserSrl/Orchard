using System;
using System.Collections.Generic;
using System.Web;
using ChartaDb.ChartaTableAdapters;
using log4net;
using System.Data;

namespace ChartaWEB
{
    public class LastMinute
    {
        private static readonly ILog logger = LogManager.GetLogger("HotList");

        public static string ListaLastMinute ()
        {
            try
            {
                GetLastMinuteTableAdapter objTaLast = new GetLastMinuteTableAdapter();
                ChartaDb.Charta.GetLastMinuteDataTable objDtLast = objTaLast.GetData();

                string sReturn = "<reply>";
                sReturn += "<last_minute_list  numLastMinute=\"" + objDtLast.Rows.Count + "\" >";
                foreach (ChartaDb.Charta.GetLastMinuteRow dr in objDtLast)
                {
                    sReturn += "<perform vcode=\"" + Util.ConvertWithANDReplace(dr.vode) + "\" code=\"" + Util.ConvertWithANDReplace(dr.pcode) + "\"";
                    sReturn += " titolo=\"" + Util.ConvertWithANDReplace(dr.Titolo) + "\" nome_teatro=\"" + Util.ConvertWithANDReplace(dr.NomeTeatro) + "\" ";
                    sReturn += " categoria=\"" + Util.ConvertWithANDReplace(dr.Categoria) + "\" id_evento=\"" + Util.ConvertWithANDReplace(dr.id_evento.ToString()) + "\" ";
                    sReturn += " comune=\"" + Util.ConvertWithANDReplace(dr.comune) + "\" provincia=\"" + Util.ConvertWithANDReplace(dr.provincia) + "\" ";
                    sReturn += " data=\"" + Util.ConvertWithANDReplace(dr.data.ToString()) + "\" ora=\"" + Util.ConvertWithANDReplace(dr.ora.ToString()) + "\" ";
                    sReturn += " img=\"" + Util.ConvertWithANDReplace(dr.immagine_mini) + "\"";
                    sReturn += " lat=\"" + Util.ConvertWithANDReplace(dr.LAT) + "\"";
                    sReturn += " lon=\"" + Util.ConvertWithANDReplace(dr.LON) + "\"";
                    sReturn += " exact=\"" + Util.ConvertWithANDReplace(dr.EXACT.ToString()) + "\" />";

                }

                SupportTableAdapter tableAdapterSupport = new SupportTableAdapter();
                DataRow[] dr1 = tableAdapterSupport.GetData().Select("key = 'baseImagePath'  ");
                string _BaseImagePath = dr1[0]["value"].ToString();
                sReturn += "<baseimagepath>" + _BaseImagePath + "</baseimagepath>";

                sReturn += "</last_minute_list></reply>";

                objDtLast.Dispose(); 
                objTaLast.Dispose();

                return sReturn;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("ListaLastMinute", "999", ex.Message);
            }
        }


    }
}
