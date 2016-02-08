using System;
using System.Collections.Generic;
using System.Web;
using log4net;
using ChartaDb.ChartaTableAdapters;


namespace ChartaWEB
{
    static public class LocInfoList
    {
        private static readonly ILog logger = LogManager.GetLogger("LocInfoList");

        public static string ListaOrgInfo (string code, string comune, string provincia)
        {
            try
            {
                LocInfoListTableAdapter objTALocInfo = new LocInfoListTableAdapter();
                if (code == "") code = null;
                if (comune == "") comune = null;
                if (provincia == "") provincia = null;

                ChartaDb.Charta.LocInfoListDataTable objDTLoc = objTALocInfo.GetData(code, comune, provincia);

                string sReturn = "<reply>";
                sReturn += "<LocInfoList>";

                foreach (ChartaDb.Charta.LocInfoListRow dr in objDTLoc)
                {
                    sReturn += "<LocInfo id=\"" + dr.id_org  + "\" >";
                    sReturn += "<id_org>" + dr.id_org  + "</id_org>";
                    sReturn += "<code>" + dr.code + "</code>";
                    sReturn += "<nome>" + Util.ConvertWithANDReplace(dr.nome) +  "</nome>";
                    sReturn += "<tipologia>" + Util.ConvertWithANDReplace(dr.tipologia) + "</tipologia>";
                    sReturn += "<indirizzo>" + Util.ConvertWithANDReplace(dr.indirizzo ) + "</indirizzo>"; 
                    sReturn += "<comune>" + Util.ConvertWithANDReplace(dr.comune) + "</comune>";
                    sReturn += "<provincia>" + Util.ConvertWithANDReplace(dr.provincia) + "</provincia>";
                    sReturn += "<cap>" + Util.ConvertWithANDReplace(dr.cap) + "</cap>";
                    sReturn += "<descrizione>" + Util.Convert(dr.descrizione) + "</descrizione>"; 
                    sReturn += "<telefono>" + Util.ConvertWithANDReplace(dr.telefono) + "</telefono>";
                    sReturn += "<fax>" + Util.ConvertWithANDReplace(dr.fax) + "</fax>";
                    sReturn += "<email>" + Util.ConvertWithANDReplace(dr.email) + "</email>" ;
                    sReturn += "<raggiungibilita>" + Util.Convert(dr.raggiungibilita) + "</raggiungibilita>";
                    sReturn += "<info_parcheggio>" + Util.Convert(dr.info_parcheggio) + "</info_parcheggio>";
                    sReturn += "<info_disabili>" + Util.Convert(dr.info_disabili) + "</info_disabili>";
                    sReturn += "<orari_apertura>" + Util.Convert(dr.info_disabili) + "</orari_apertura>";
                    sReturn += "<sito>" + Util.ConvertWithANDReplace(dr.sito) + "</sito>"; 
                    sReturn += "<immagine>" + Util.ConvertWithANDReplace(dr.immagine) + "</immagine>";         
                    sReturn += "</LocInfo>";
                }
                objDTLoc.Dispose();
                objTALocInfo.Dispose();

                sReturn += "</LocInfoList></reply>";

                return sReturn;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("LocInfoList", "999", ex.Message); 
            }

        }


        public static string ListaOrgInfoDistinct(string code, string comune, string provincia)
        {
            try
            {
                LocInfoListDistinctTableAdapter objTALocInfo = new LocInfoListDistinctTableAdapter();
                if (code == "") code = null;
                if (comune == "") comune = null;
                if (provincia == "") provincia = null;

                ChartaDb.Charta.LocInfoListDistinctDataTable objDTLoc = objTALocInfo.GetData(code, comune, provincia);

                string sReturn = "<reply>";
                sReturn += "<LocInfoListDistinct>";


                string lastNome = "";
                string lastCom = "";
                string comuni = "";

                bool firstNome = true;
                int NCount = 0;
                string phN = "";

                foreach (ChartaDb.Charta.LocInfoListDistinctRow dr in objDTLoc)
                {
                   
                    phN = "PH_N" + NCount.ToString();
                    if (dr.nome.CompareTo(lastNome) != 0)
                    {
                        if (!firstNome)
                        {
                            sReturn = sReturn.Replace(phN, comuni);
                            sReturn += "</N>";

                            lastCom = "";
                            comuni = "";
                            NCount++;
                            phN = "PH_N" + NCount.ToString();
                        }
                        else
                            firstNome = false;

                        


                        sReturn += "<N n=\"" + Util.ConvertWithANDReplace(dr.nome) + "\" com=\"" + phN + "\">";

                        lastNome = dr.nome;
                       
                    }

                    if (lastCom.CompareTo(dr.comune) != 0)
                    {
                        if (comuni.CompareTo("") == 0)
                            comuni += Util.ConvertWithANDReplace(dr.comune);
                        else
                            comuni += "," + Util.ConvertWithANDReplace(dr.comune);
                    }

                    lastCom = dr.comune;

                    sReturn += "<c>" + dr.code + "</c>";


                    //se ho solo un record rimaneva il placeholder PH_N0
                    /*if (objDTLoc.Rows.Count == 1)
                    {
                        sReturn = sReturn.Replace(phN, comuni);
                    }*/


                }


                sReturn = sReturn.Replace(phN, comuni);

               /* foreach (ChartaDb.Charta.LocInfoListDistinctRow dr in objDTLoc)
                {
                    if (dr.comune.CompareTo(lastComune) != 0)
                    {
                        if (!firstNome)
                        {
                            sReturn += "</N>";
                            firstNome = true;
                        }

                        if (!firstComune)
                            sReturn += "</Com>";
                        else
                            firstComune = false;

                        sReturn += "<Com c=\"" + Util.Convert(dr.comune) + "\" >";
                        lastComune = dr.comune;

                    }

                    if (dr.nome.CompareTo(lastNome) != 0)
                    {
                        if (!firstNome)
                            sReturn += "</N>";
                        else
                            firstNome = false;

                        sReturn += "<N n=\"" + Util.Convert(dr.nome) + "\" >";
                        lastNome = dr.nome;
                    }
                    
                    sReturn += "<c>" + dr.code + "</c>";

                    
                }  */
                objDTLoc.Dispose();
                objTALocInfo.Dispose();

                if (!firstNome)
                    sReturn += "</N>";

               /* if (!firstComune)
                    sReturn += "</Com>";*/

                sReturn += "</LocInfoListDistinct></reply>";

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
