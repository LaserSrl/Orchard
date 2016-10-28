﻿using System;
using System.Collections.Generic;
using System.Web;
using log4net;
using ChartaDb.ChartaTableAdapters;
using Laser.Orchard.Commons.Services;
using System.Text;
using Laser.Orchard.ChartaWS.ChartaWEB;


namespace ChartaWEB
{
    public static class CityList
    {
        private static readonly ILog logger = LogManager.GetLogger("CityList");

        public static  string ListaCity ()
        {
            try
            {
                var lista = new List<string>();
                //City city = null;
                using (CityListTableAdapter objTACity = new CityListTableAdapter())
                {
                    using (ChartaDb.Charta.CityListDataTable objDtCity = objTACity.GetData())
                    {
                        //string sReturn = "<reply>";
                        //sReturn += "<CityList>";

                        //foreach (ChartaDb.Charta.CityListRow dr in objDtCity)
                        //{
                        //    sReturn += "<City>" + Util.ConvertWithANDReplace(dr.vcity) + "</City>";
                        //}
                        //objDtCity.Dispose();
                        //objTACity.Dispose();

                        //sReturn += "</CityList>" + "</reply>";

                        //*****************************************************************
                        foreach (ChartaDb.Charta.CityListRow dr in objDtCity)
                        {
                            lista.Add(dr.vcity);
                        }
                    }
                }
                // serializza il risultato
                //System.Xml.Linq.XElement dump = null;
                //ObjectDumper dumper = new ObjectDumper(10, null, false, true, null);
                var sb = new StringBuilder();
                sb.Append("{\"m\":["); // json start

                for (int idx = 0; idx < lista.Count; idx++)
                {
                    sb.AppendFormat("{{ \"n\":\"[{1}]\", \"v\":\"City\", \"m\":[ {{\"n\":\"Name\", \"v\":{0} }},{{\"n\":\"Sid\", \"v\":{0} }}]}}", Util.EncodeForJson(lista[idx]), idx); 
                }

                //dump = dumper.Dump(lista.ToArray(), "CityList");
                //JsonConverter.ConvertToJSon(dump, sb, false, true);
                sb.Append("]}"); // json end
                string sReturn = sb.ToString().Replace("}{", "},{");

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
