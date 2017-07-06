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
    static public class LocInfoList
    {
        private static readonly ILog logger = LogManager.GetLogger("LocInfoList");

        public static string ListaOrgInfo (string code, string comune, string provincia)
        {
            try
            {
                //LocInfoListTableAdapter objTALocInfo = new LocInfoListTableAdapter();
                //if (code == "") code = null;
                //if (comune == "") comune = null;
                //if (provincia == "") provincia = null;

                //ChartaDb.Charta.LocInfoListDataTable objDTLoc = objTALocInfo.GetData(code, comune, provincia);

                //string sReturn = "<reply>";
                //sReturn += "<LocInfoList>";

                //foreach (ChartaDb.Charta.LocInfoListRow dr in objDTLoc)
                //{
                //    sReturn += "<LocInfo id=\"" + dr.id_org + "\" >";
                //    sReturn += "<id_org>" + dr.id_org + "</id_org>";
                //    sReturn += "<code>" + dr.code + "</code>";
                //    sReturn += "<nome>" + Util.ConvertWithANDReplace(dr.nome) + "</nome>";
                //    sReturn += "<tipologia>" + Util.ConvertWithANDReplace(dr.tipologia) + "</tipologia>";
                //    sReturn += "<indirizzo>" + Util.ConvertWithANDReplace(dr.indirizzo) + "</indirizzo>";
                //    sReturn += "<comune>" + Util.ConvertWithANDReplace(dr.comune) + "</comune>";
                //    sReturn += "<provincia>" + Util.ConvertWithANDReplace(dr.provincia) + "</provincia>";
                //    sReturn += "<cap>" + Util.ConvertWithANDReplace(dr.cap) + "</cap>";
                //    sReturn += "<descrizione>" + Util.Convert(dr.descrizione) + "</descrizione>";
                //    sReturn += "<telefono>" + Util.ConvertWithANDReplace(dr.telefono) + "</telefono>";
                //    sReturn += "<fax>" + Util.ConvertWithANDReplace(dr.fax) + "</fax>";
                //    sReturn += "<email>" + Util.ConvertWithANDReplace(dr.email) + "</email>";
                //    sReturn += "<raggiungibilita>" + Util.Convert(dr.raggiungibilita) + "</raggiungibilita>";
                //    sReturn += "<info_parcheggio>" + Util.Convert(dr.info_parcheggio) + "</info_parcheggio>";
                //    sReturn += "<info_disabili>" + Util.Convert(dr.info_disabili) + "</info_disabili>";
                //    sReturn += "<orari_apertura>" + Util.Convert(dr.info_disabili) + "</orari_apertura>";
                //    sReturn += "<sito>" + Util.ConvertWithANDReplace(dr.sito) + "</sito>";
                //    sReturn += "<immagine>" + Util.ConvertWithANDReplace(dr.immagine) + "</immagine>";
                //    sReturn += "</LocInfo>";
                //}
                //objDTLoc.Dispose();
                //objTALocInfo.Dispose();

                //sReturn += "</LocInfoList></reply>";
                //************************************************
                var lista = new List<LocInfo>();
                LocInfo locInfo = null;
                using (LocInfoListTableAdapter objTALocInfo = new LocInfoListTableAdapter())
                {
                    if (code == "") code = null;
                    if (comune == "") comune = null;
                    if (provincia == "") provincia = null;

                    using (ChartaDb.Charta.LocInfoListDataTable objDTLoc = objTALocInfo.GetData(code, comune, provincia))
                    {
                        foreach (ChartaDb.Charta.LocInfoListRow dr in objDTLoc)
                        {
                            locInfo = new LocInfo();
                            locInfo.Id = dr.id_org;
                            locInfo.Sid = "LocInfo-" + dr.id_org;
                            locInfo.IdOrg = dr.id_org;
                            locInfo.Code = dr.code;
                            locInfo.Nome = dr.nome;
                            locInfo.Tipologia = dr.tipologia;
                            locInfo.Indirizzo = dr.indirizzo;
                            locInfo.Comune = dr.comune;
                            locInfo.Provincia = dr.provincia;
                            locInfo.Cap = dr.cap;
                            locInfo.Descrizione = dr.descrizione;
                            locInfo.Telefono = dr.telefono;
                            locInfo.Fax = dr.fax;
                            locInfo.Email = dr.email;
                            locInfo.Raggiugibilita = dr.raggiungibilita;
                            locInfo.InfoParcheggio = dr.info_parcheggio;
                            locInfo.InfoDisabili = dr.info_disabili;
                            locInfo.OrariApertura = dr.orari_apertura;
                            locInfo.Sito = dr.sito;
                            locInfo.Immagine = dr.immagine;
                            lista.Add(locInfo);
                        }
                    }
                }

                // serializza il risultato
                System.Xml.Linq.XElement dump = null;
                ObjectDumper dumper = new ObjectDumper(10, null, false, true, null);
                var sb = new StringBuilder();
                sb.Append("{\"m\":[{\"n\":\"Reply\", \"v\":\"Reply\"}]"); // json start
                sb.Append(",\"l\":[{"); // lista start
                dump = dumper.Dump(lista.ToArray(), "LocInfoList");
                JsonConverter.ConvertToJSon(dump, sb, false, true);
                sb.Append("}]"); // lista end
                sb.Append("}"); // json end
                string sReturn = sb.ToString().Replace("\t", " ");
                return sReturn;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("LocInfoList", "999", ex.Message); 
            }
        }

        private static void serializzaLocinfoDistinct(int index, StringBuilder sb, LocInfoDistinct locInfoDistinct, List<string> lista)
        {
            //if (index > 0)
            //{
            //    sb.Append("},{");
            //}
            //// serializza la LocInfodistinct
            //var dumper = new ObjectDumper(10, null, false, true, null);
            //var dump = dumper.Dump(locInfoDistinct, string.Format("[{0}]", index));
            //JsonConverter.ConvertToJSon(dump, sb, false, true);

            //// serializza i codici
            //sb.Append(",\"l\":[{"); // lista start
            //dumper = new ObjectDumper(10, null, false, true, null);
            //dump = dumper.Dump(lista.ToArray(), "Codici");
            //JsonConverter.ConvertToJSon(dump, sb, false, true);
            //sb.Append("}]"); // lista end

            // serializza la LocInfodistinct
            sb.AppendFormat("{{\"n\":\"[{0}]\",\"v\":\"LocInfoDistinct\",\"m\":[", index);
            sb.AppendFormat("{{\"n\":\"Nome\",\"v\":{0}}}", Util.EncodeForJson(locInfoDistinct.Nome));
            sb.AppendFormat(",{{\"n\":\"Sid\",\"v\":{0}}}", Util.EncodeForJson(locInfoDistinct.Sid));
            sb.AppendFormat(",{{\"n\":\"Comune\",\"v\":{0}}}", Util.EncodeForJson(locInfoDistinct.Comune));

            // serializza i codici come attributi multipli
            sb.Append(",{\"n\":\"Codici\",\"v\":\"Codice[]\",\"m\":[");
            for (int idx = 0; idx < lista.Count; idx++)
            {
                sb.AppendFormat("{{\"n\":\"[{0}]\",\"v\":{1}}}", idx, Util.EncodeForJson(lista[idx]));
            }
            sb.Append("]}]}");
        }

        public static string ListaOrgInfoDistinct(string code, string comune, string provincia)
        {
            try
            {
                //LocInfoListDistinctTableAdapter objTALocInfo = new LocInfoListDistinctTableAdapter();
                //if (code == "") code = null;
                //if (comune == "") comune = null;
                //if (provincia == "") provincia = null;

                //ChartaDb.Charta.LocInfoListDistinctDataTable objDTLoc = objTALocInfo.GetData(code, comune, provincia);

                //string sReturn = "<reply>";
                //sReturn += "<LocInfoListDistinct>";


                //string lastNome = "";
                //string lastCom = "";
                //string comuni = "";

                //bool firstNome = true;
                //int NCount = 0;
                //string phN = "";

                //foreach (ChartaDb.Charta.LocInfoListDistinctRow dr in objDTLoc)
                //{
                //    phN = "PH_N" + NCount.ToString();
                //    if (dr.nome.CompareTo(lastNome) != 0)
                //    {
                //        if (!firstNome)
                //        {
                //            sReturn = sReturn.Replace(phN, comuni);
                //            sReturn += "</N>";

                //            lastCom = "";
                //            comuni = "";
                //            NCount++;
                //            phN = "PH_N" + NCount.ToString();
                //        }
                //        else
                //            firstNome = false;

                //        sReturn += "<N n=\"" + Util.ConvertWithANDReplace(dr.nome) + "\" com=\"" + phN + "\">";
                //        lastNome = dr.nome;
                //    }

                //    if (lastCom.CompareTo(dr.comune) != 0)
                //    {
                //        if (comuni.CompareTo("") == 0)
                //            comuni += Util.ConvertWithANDReplace(dr.comune);
                //        else
                //            comuni += "," + Util.ConvertWithANDReplace(dr.comune);
                //    }

                //    lastCom = dr.comune;

                //    sReturn += "<c>" + dr.code + "</c>";
                //}

                //sReturn = sReturn.Replace(phN, comuni);

                //objDTLoc.Dispose();
                //objTALocInfo.Dispose();

                //if (!firstNome)
                //    sReturn += "</N>";

                //sReturn += "</LocInfoListDistinct></reply>";
                //********************************************************
                int index = 0;
                List<string> lista = null;
                LocInfoDistinct locInfoDistinct = null;
                var sb = new StringBuilder();
                sb.Append("{\"m\":[{\"n\":\"Reply\", \"v\":\"Reply\"}]"); // json start
                sb.Append(",\"l\":[{ \"n\":\"LocInfoListDistinct\",\"v\":\"LocInfoDistinct[]\",\"m\":["); // lista start
                using (LocInfoListDistinctTableAdapter objTALocInfo = new LocInfoListDistinctTableAdapter())
                {
                    if (code == "") code = null;
                    if (comune == "") comune = null;
                    if (provincia == "") provincia = null;

                    using (ChartaDb.Charta.LocInfoListDistinctDataTable objDTLoc = objTALocInfo.GetData(code, comune, provincia))
                    {
                        string lastNome = "";
                        string lastCom = "";

                        foreach (ChartaDb.Charta.LocInfoListDistinctRow dr in objDTLoc)
                        {
                            if ((dr.nome.CompareTo(lastNome) != 0) || (dr.comune.CompareTo(lastCom) != 0)) // rottura di chiave
                            {
                                if (lastNome != "")
                                {
                                    // serializza la locInfoDistinct precedente
                                    serializzaLocinfoDistinct(index, sb, locInfoDistinct, lista);
                                    index++;
                                }

                                locInfoDistinct = new LocInfoDistinct();
                                locInfoDistinct.Nome = dr.nome;
                                locInfoDistinct.Comune = dr.comune;
                                locInfoDistinct.Sid = "LocInfoDistinct-" + dr.nome;
                                lastNome = dr.nome;
                                lastCom = dr.comune;
                                lista = new List<string>();
                            }
                            lista.Add(dr.code);
                        }
                        // serializza l'ultima locInfoDistinct
                        serializzaLocinfoDistinct(index, sb, locInfoDistinct, lista);
                    }
                }
                sb.Append("]}]"); // lista end
                sb.Append("}"); // json end

                string sReturn = sb.ToString().Replace("}{", "},{");
                return sReturn;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("LocInfoListDistinct", "999", ex.Message);
            }
        }
    }
}
