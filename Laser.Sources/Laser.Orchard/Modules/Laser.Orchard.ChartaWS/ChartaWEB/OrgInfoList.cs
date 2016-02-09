using System;
using System.Collections.Generic;
using System.Web;
using log4net;
using ChartaDb.ChartaTableAdapters;
using Laser.Orchard.ChartaWS.ChartaWEB;
using Laser.Orchard.Commons.Services;
using System.Text;


namespace ChartaWEB
{
    public static class OrgInfoList
    {
        private static readonly ILog logger = LogManager.GetLogger("OrgInfoList");

        public static string ListaOrgInfo (string  pIdOrg)
        {
            try
            {
                //OrgInfoListTableAdapter objTAOrgInfo = new OrgInfoListTableAdapter();
                //ChartaDb.Charta.OrgInfoListDataTable objDtOrgInfo;

                //if (string.IsNullOrEmpty(pIdOrg))
                //{
                //    objDtOrgInfo = objTAOrgInfo.GetData(null);
                //}
                //else
                //{
                //    objDtOrgInfo = objTAOrgInfo.GetData(int.Parse(pIdOrg));
                //}


                //string sReturn = "<reply>";
                //sReturn += "<OrgInfoList>";

                //foreach (ChartaDb.Charta.OrgInfoListRow dr in objDtOrgInfo)
                //{
                //    sReturn += "<OrgInfo id=\"" + dr.id_org + "\" >";
                //    sReturn += "<Nome>" + Util.ConvertWithANDReplace(dr.nome.ToString()) + "</Nome>";
                //    sReturn += "<Indirizzo>" + Util.ConvertWithANDReplace(dr.indirizzo.ToString()) + "</Indirizzo>";
                //    sReturn += "<Comune>" +  Util.ConvertWithANDReplace(dr.comune.ToString()) + "</Comune>";
                //    sReturn += "<Provincia>" + Util.ConvertWithANDReplace(dr.provincia.ToString()) + "</Provincia>";
                //    sReturn += "<Cap>" +  Util.ConvertWithANDReplace(dr.cap.ToString()) + "</Cap>";
                //    sReturn += "<Telefono>" + Util.ConvertWithANDReplace(dr.telefono.ToString()) + "</Telefono>";
                //    sReturn += "</OrgInfo>";

                //}
                //objDtOrgInfo.Dispose();
                //objTAOrgInfo.Dispose();

                //sReturn += "</OrgInfoList></reply>";
                //*********************************************************
                var lista = new List<OrgInfo>();
                OrgInfo orgInfo = null;
                using (OrgInfoListTableAdapter objTAOrgInfo = new OrgInfoListTableAdapter())
                {
                    ChartaDb.Charta.OrgInfoListDataTable objDtOrgInfo;
                    if (string.IsNullOrEmpty(pIdOrg))
                    {
                        objDtOrgInfo = objTAOrgInfo.GetData(null);
                    }
                    else
                    {
                        objDtOrgInfo = objTAOrgInfo.GetData(int.Parse(pIdOrg));
                    }
                    try
                    {
                        foreach (ChartaDb.Charta.OrgInfoListRow dr in objDtOrgInfo)
                        {
                            orgInfo = new OrgInfo();
                            orgInfo.Id = dr.id_org;
                            orgInfo.Nome = dr.nome;
                            orgInfo.Indirizzo = dr.indirizzo;
                            orgInfo.Comune = dr.comune;
                            orgInfo.Provincia = dr.provincia;
                            orgInfo.Cap = dr.cap;
                            orgInfo.Telefono = dr.telefono;
                            lista.Add(orgInfo);
                        }
                    }
                    finally
                    {
                        objDtOrgInfo.Dispose();
                    }
                }

                // serializza il risultato
                System.Xml.Linq.XElement dump = null;
                ObjectDumper dumper = new ObjectDumper(10, null, false, true, null);
                var sb = new StringBuilder();
                sb.Append("{"); // json start
                sb.Append("\"l\":[{"); // lista start
                dump = dumper.Dump(lista.ToArray(), "OrgInfoList");
                JsonConverter.ConvertToJSon(dump, sb, false, true);
                sb.Append("}]"); // lista end
                sb.Append("}"); // json end
                string sReturn = sb.ToString().Replace("\t", " ");
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
