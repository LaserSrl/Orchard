using System;
using System.Collections.Generic;
using System.Web;
using ChartaDb.ChartaTableAdapters;
using log4net;
using System.Data;


namespace ChartaWEB
{
    public class News
    {
        private static readonly ILog logger = LogManager.GetLogger("HotList");

        public static string ListaNews ()
        {
            try
            {
                ChartaDb.Charta.GetNewsDataTable objDtNews = new ChartaDb.Charta.GetNewsDataTable();
                GetNewsTableAdapter objTaNews = new GetNewsTableAdapter();

                try
                {
                    objTaNews.Fill(objDtNews);
                }
                catch (ConstraintException)
                {
                    DataRow[] errs = objDtNews.GetErrors();
                    foreach (DataRow err in errs)
                    {
                        string error = err.RowError;
                    }
                }

                string sReturn = "<reply>";
                sReturn += "<news_list newsListNum=\"" + objDtNews.Rows.Count + "\" >";
                foreach (ChartaDb.Charta.GetNewsRow   dr in objDtNews)
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

                sReturn += "</news_list></reply>";

                objDtNews.Dispose();
                objTaNews.Dispose();

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
