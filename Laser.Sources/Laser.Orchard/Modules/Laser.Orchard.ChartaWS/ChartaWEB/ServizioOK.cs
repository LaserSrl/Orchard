using System;
using System.Collections.Generic;
using System.Web;
using log4net;
using System.Xml;
using ChartaDb.ChartaTableAdapters;
using System.Text;
using System.Data;
using ChartaDb;


namespace ChartaWEB
{
    public class ServizioOK
    {
        private static readonly ILog logger = LogManager.GetLogger("ServizioOK");


        public static string GetServizioOK ()
        {
            try
            {
                string sReturn = string.Empty;
                
                SupportTableAdapter tableAdapterSupport = new SupportTableAdapter();
                DataRow[] dr1 = tableAdapterSupport.GetData().Select("key = 'ServizioOk' ");
                string svalue = dr1[0]["value"].ToString();
                sReturn += "<reply>";
                sReturn += "<ServizioOk>" + Util.ConvertWithANDReplace(svalue) + "</ServizioOk>";
                sReturn += "</reply>";

                tableAdapterSupport.Dispose();


                return sReturn;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("ServizioOK", "999", ex.Message);
            }
            

        }

        public static string GetNumeroServizio ()
        {
            try
            {
                string sReturn = string.Empty;

                SupportTableAdapter tableAdapterSupport = new SupportTableAdapter();
                DataRow[] dr1 = tableAdapterSupport.GetData().Select("key = 'NumeroServizio' ");
                string svalue = dr1[0]["value"].ToString();
                sReturn += "<reply>";
                sReturn += "<NumeroServizio>" + Util.ConvertWithANDReplace(svalue) + "</NumeroServizio>";
                sReturn += "</reply>";

                tableAdapterSupport.Dispose();


                return sReturn;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("GetNumeroServizio", "999", ex.Message);
            }


        }

    }
}
