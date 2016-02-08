using System;
using System.Collections.Generic;
using System.Web;
using log4net;
using ChartaDb.ChartaTableAdapters;


namespace ChartaWEB
{
    public static class CityList
    {
        private static readonly ILog logger = LogManager.GetLogger("CityList");

        public static  string ListaCity ()
        {

            try
            {
                CityListTableAdapter objTACity = new CityListTableAdapter();
                ChartaDb.Charta.CityListDataTable objDtCity = objTACity.GetData();


                string sReturn = "<reply>";
                sReturn += "<CityList>";

                foreach (ChartaDb.Charta.CityListRow dr in objDtCity)
                {
                    sReturn += "<City>" + Util.ConvertWithANDReplace(dr.vcity) + "</City>";
                }
                objDtCity.Dispose();
                objTACity.Dispose();

                sReturn += "</CityList>" + "</reply>";

                return sReturn;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("CityList", "999", ex.Message); 
            }

        }
    }
}
