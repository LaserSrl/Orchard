using System;
using System.Collections.Generic;
using System.Web;
using log4net;
using ChartaDb.ChartaTableAdapters;


namespace ChartaWEB
{
    public static class OrgInfoList
    {
        private static readonly ILog logger = LogManager.GetLogger("OrgInfoList");

        public static string ListaOrgInfo (string  pIdOrg)
        {
            try
            {
                OrgInfoListTableAdapter objTAOrgInfo = new OrgInfoListTableAdapter();
                ChartaDb.Charta.OrgInfoListDataTable objDtOrgInfo;

                if (string.IsNullOrEmpty(pIdOrg))
                {
                    objDtOrgInfo = objTAOrgInfo.GetData(null);
                }
                else
                {
                    objDtOrgInfo = objTAOrgInfo.GetData(int.Parse(pIdOrg));
                }


                string sReturn = "<reply>";
                sReturn += "<OrgInfoList>";

                foreach (ChartaDb.Charta.OrgInfoListRow dr in objDtOrgInfo)
                {
                    sReturn += "<OrgInfo id=\"" + dr.id_org + "\" >";
                    sReturn += "<Nome>" + Util.ConvertWithANDReplace(dr.nome.ToString()) + "</Nome>";
                    sReturn += "<Indirizzo>" + Util.ConvertWithANDReplace(dr.indirizzo.ToString()) + "</Indirizzo>";
                    sReturn += "<Comune>" +  Util.ConvertWithANDReplace(dr.comune.ToString()) + "</Comune>";
                    sReturn += "<Provincia>" + Util.ConvertWithANDReplace(dr.provincia.ToString()) + "</Provincia>";
                    sReturn += "<Cap>" +  Util.ConvertWithANDReplace(dr.cap.ToString()) + "</Cap>";
                    sReturn += "<Telefono>" + Util.ConvertWithANDReplace(dr.telefono.ToString()) + "</Telefono>";
                    sReturn += "</OrgInfo>";

                }
                objDtOrgInfo.Dispose();
                objTAOrgInfo.Dispose();

                sReturn += "</OrgInfoList></reply>";

                return sReturn;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("OrgInfoList", "999", ex.Message); 
            }
        }
    }
}
