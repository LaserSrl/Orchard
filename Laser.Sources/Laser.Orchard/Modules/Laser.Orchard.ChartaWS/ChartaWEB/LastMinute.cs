using System;
using System.Collections.Generic;
using System.Web;
using ChartaDb.ChartaTableAdapters;
using log4net;
using System.Data;
using Laser.Orchard.ChartaWS.ChartaWEB;
using Laser.Orchard.Commons.Services;
using System.Text;
using System.Globalization;

namespace ChartaWEB
{
    public class LastMinute
    {
        private static readonly ILog logger = LogManager.GetLogger("LastMinute");

        public static string ListaLastMinute ()
        {
            try
            {
                //GetLastMinuteTableAdapter objTaLast = new GetLastMinuteTableAdapter();
                //ChartaDb.Charta.GetLastMinuteDataTable objDtLast = objTaLast.GetData();

                //string sReturn = "<reply>";
                //sReturn += "<last_minute_list  numLastMinute=\"" + objDtLast.Rows.Count + "\" >";
                //foreach (ChartaDb.Charta.GetLastMinuteRow dr in objDtLast)
                //{
                //    sReturn += "<perform vcode=\"" + Util.ConvertWithANDReplace(dr.vode) + "\" code=\"" + Util.ConvertWithANDReplace(dr.pcode) + "\"";
                //    sReturn += " titolo=\"" + Util.ConvertWithANDReplace(dr.Titolo) + "\" nome_teatro=\"" + Util.ConvertWithANDReplace(dr.NomeTeatro) + "\" ";
                //    sReturn += " categoria=\"" + Util.ConvertWithANDReplace(dr.Categoria) + "\" id_evento=\"" + Util.ConvertWithANDReplace(dr.id_evento.ToString()) + "\" ";
                //    sReturn += " comune=\"" + Util.ConvertWithANDReplace(dr.comune) + "\" provincia=\"" + Util.ConvertWithANDReplace(dr.provincia) + "\" ";
                //    sReturn += " data=\"" + Util.ConvertWithANDReplace(dr.data.ToString()) + "\" ora=\"" + Util.ConvertWithANDReplace(dr.ora.ToString()) + "\" ";
                //    sReturn += " img=\"" + Util.ConvertWithANDReplace(dr.immagine_mini) + "\"";
                //    sReturn += " lat=\"" + Util.ConvertWithANDReplace(dr.LAT) + "\"";
                //    sReturn += " lon=\"" + Util.ConvertWithANDReplace(dr.LON) + "\"";
                //    sReturn += " exact=\"" + Util.ConvertWithANDReplace(dr.EXACT.ToString()) + "\" />";

                //}

                //SupportTableAdapter tableAdapterSupport = new SupportTableAdapter();
                //DataRow[] dr1 = tableAdapterSupport.GetData().Select("key = 'baseImagePath'  ");
                //string _BaseImagePath = dr1[0]["value"].ToString();
                //sReturn += "<baseimagepath>" + _BaseImagePath + "</baseimagepath>";

                //sReturn += "</last_minute_list></reply>";

                //objDtLast.Dispose(); 
                //objTaLast.Dispose();

                //****************************************************
                var culture = CultureInfo.InvariantCulture;
                Perform perf = null;
                var lista = new List<Perform>();
                string baseImagePath = "";
                using (SupportTableAdapter tableAdapterSupport = new SupportTableAdapter())
                {
                    DataRow[] dr1 = tableAdapterSupport.GetData().Select("key = 'baseImagePath'  ");
                    baseImagePath = dr1[0]["value"].ToString();
                }
                using (GetLastMinuteTableAdapter objTaLast = new GetLastMinuteTableAdapter())
                {
                    using (ChartaDb.Charta.GetLastMinuteDataTable objDtLast = objTaLast.GetData())
                    {
                        foreach (ChartaDb.Charta.GetLastMinuteRow dr in objDtLast)
                        {
                            perf = new Perform();
                            perf.VCode = dr.vode;
                            perf.Code = dr.pcode.Trim();
                            perf.Titolo = dr.Titolo;
                            perf.NomeTeatro = dr.NomeTeatro;
                            perf.Categoria = dr.Categoria;
                            perf.IdEvento = dr.id_evento;
                            perf.Comune = dr.comune;
                            perf.Provincia = dr.provincia;
                            perf.DataEOra = DateTime.ParseExact(dr.data + dr.ora, "yyyyMMddHH.mm", culture);
                            perf.Img = Util.ConcatUrlPath(baseImagePath, dr.immagine_mini);
                            perf.Lat = decimal.Parse(dr.LAT, culture);
                            perf.Lng = decimal.Parse(dr.LON, culture);
                            perf.Exact = (dr.EXACT)? 1 : 0;
                            lista.Add(perf);
                        }
                    }
                }

                // serializza il risultato
                var sb = new StringBuilder();
                sb.Append("{\"m\":[{\"n\":\"Reply\",\"v\":\"Reply\"}], \"l\":[{"); // lista start
                var dumper = new ObjectDumper(10, null, false, true, null);
                var dump = dumper.Dump(lista.ToArray(), "LastMinute");
                JsonConverter.ConvertToJSon(dump, sb, false, true);
                sb.Append("}]}"); // lista end

                string sReturn = sb.ToString().Replace("\t", " ");
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
