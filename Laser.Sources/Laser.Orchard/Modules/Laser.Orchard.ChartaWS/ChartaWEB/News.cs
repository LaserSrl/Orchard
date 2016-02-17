using System;
using System.Collections.Generic;
using System.Web;
using ChartaDb.ChartaTableAdapters;
using log4net;
using System.Data;
using Laser.Orchard.ChartaWS.ChartaWEB;
using Laser.Orchard.Commons.Services;
using System.Text;


namespace ChartaWEB
{
    public class News
    {
        private static readonly ILog logger = LogManager.GetLogger("News");

        public string BaseImagePath { get; set; }
        public int NumLastMinute { get; set; }

        public static string ListaNews ()
        {
            try
            {
                //ChartaDb.Charta.GetNewsDataTable objDtNews = new ChartaDb.Charta.GetNewsDataTable();
                //GetNewsTableAdapter objTaNews = new GetNewsTableAdapter();

                //try
                //{
                //    objTaNews.Fill(objDtNews);
                //}
                //catch (ConstraintException)
                //{
                //    DataRow[] errs = objDtNews.GetErrors();
                //    foreach (DataRow err in errs)
                //    {
                //        string error = err.RowError;
                //    }
                //}

                //string sReturn = "<reply>";
                //sReturn += "<news_list newsListNum=\"" + objDtNews.Rows.Count + "\" >";
                //foreach (ChartaDb.Charta.GetNewsRow   dr in objDtNews)
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

                //sReturn += "</news_list></reply>";

                //objDtNews.Dispose();
                //objTaNews.Dispose();

                //***********************************************************
                Perform perf = null;
                var lista = new List<Perform>();
                News objResult = new News();
                using (ChartaDb.Charta.GetNewsDataTable objDtNews = new ChartaDb.Charta.GetNewsDataTable())
                {
                    using (GetNewsTableAdapter objTaNews = new GetNewsTableAdapter())
                    {
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

                        foreach (ChartaDb.Charta.GetNewsRow dr in objDtNews)
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
                            perf.Data = dr.data;
                            perf.Ora = dr.ora;
                            perf.Img = dr.immagine_mini;
                            perf.Lat = dr.LAT;
                            perf.Lon = dr.LON;
                            perf.Exact = dr.EXACT;

                            lista.Add(perf);
                        }
                    }
                }
                using (SupportTableAdapter tableAdapterSupport = new SupportTableAdapter())
                {
                    DataRow[] dr1 = tableAdapterSupport.GetData().Select("key = 'baseImagePath'  ");
                    objResult.BaseImagePath = dr1[0]["value"].ToString();
                }
                objResult.NumLastMinute = lista.Count;

                // serializza il risultato
                System.Xml.Linq.XElement dump = null;
                ObjectDumper dumper = new ObjectDumper(10, null, false, true, null);
                dump = dumper.Dump(objResult, "reply");
                var sb = new StringBuilder();
                JsonConverter.ConvertToJSon(dump, sb, false, true);
                sb.Insert(0, "{"); // json start

                // aggiunge la lista
                sb.Append(", \"l\":[{"); // lista start
                ObjectDumper dumper2 = new ObjectDumper(10, null, false, true, null);
                dump = dumper2.Dump(lista.ToArray(), "PerformList");
                JsonConverter.ConvertToJSon(dump, sb, false, true);
                sb.Append("}]"); // lista end

                sb.Append("}"); // json end
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
