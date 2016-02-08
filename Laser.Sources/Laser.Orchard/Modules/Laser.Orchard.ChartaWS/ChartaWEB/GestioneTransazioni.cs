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
    /// <summary>
    /// Classe che gestisce i comandi di Richiesta Posti e Invio Anagrafica. Oltre a fare da proxy esegue salvataggi 
    /// su database.
    /// </summary>
    public class GestioneTransazioni
    {
        private static readonly ILog logger = LogManager.GetLogger("GestioneTransazioni");

        private const int OK = 1;
        private const int ERRORNEWTRANSAZIONE = 1001;
        private const int ERRORINVIOANAGRAFICA = 1002;
        private const int ERRORICHIESTASTATOTRAN = 1003;



        private static int idSeat;
        private static string description;
        private static string zone;
        private static float price;
        private static float presale;
        private static float commission;
        private static float iva;
        private static string idReduction;
        private static List<Riduzioni> objListR = new List<Riduzioni>();
        private static Riduzioni objRiduzioni;
        private static string idTransaction;


        /// <summary>
        /// Gestisce il comando di richiesta posti e salva le informazio della transazione su DataBase.
        /// </summary>
        /// <param name="_Response">Oggetto response della pagina chiamante</param>
        /// <param name="_queryRequest">Parametri da inviare a Charta</param>
        /// <returns>Ritorna la string XML di Charta. In caso di errori ritorna l'errore che si è verificato</returns>
        public static string RichiediPosti (HttpResponse _Response, string _queryRequest, string remoteUrl)
        {
            try
            {
                string sReturn = "";
                logger.Debug(remoteUrl + _queryRequest);
                string xml= Util.ChiamataProxy(_queryRequest, remoteUrl);

                logger.Debug("RichiediPosti, parametri: " + _queryRequest);

                /*gestisco il ritorno*/
                XmlDocument objDocument = new XmlDocument();
                objDocument.LoadXml(xml); 
                
                XmlNodeList objNodeList = objDocument.SelectNodes("reply/warning" );
                if (objNodeList != null && objNodeList.Count > 0)
                {
                    //Errori da Charta che inoltro.
                    //Non viene gestita nessuna transazione
                    return xml;
                }

                objNodeList = objDocument.SelectNodes("reply/transaction");
                idTransaction = "";

                if (objNodeList != null && objNodeList.Count > 0)
                {
                    XmlNode objNodeTrans = objNodeList[0];

                    idTransaction = objNodeTrans.Attributes["custref"].Value;
                    string amount = objNodeTrans.Attributes["amount"].Value; 
                    double? import= 0;
                    short? Risultato= 0;
                    object objReturn = null;

                    if (!string.IsNullOrEmpty(amount) )  
                        import = double.Parse(amount ) ;

                    //Inserisco i dati nel DataBase
                    TransazioniTableAdapter objTATran = new TransazioniTableAdapter();
                    objReturn = objTATran.NuovaTransazione(idTransaction, import, ref Risultato);

                    switch (Risultato)
                    {
                        case OK:

                            try
                            {
                                logger.Debug("RichiediPosti, Salvataggio transazione eseguito correttamente. IdTransazione: " + idTransaction);
                                // Salvo i Dati del XML di ritorno!!

                                foreach (XmlNode objGrandNod in objNodeTrans.ChildNodes)
                                {
                                    if (objGrandNod.Name == "seat")
                                    {
                                        InitDati();

                                        idSeat = int.Parse(objGrandNod.Attributes["id"].Value.ToString());

                                        foreach (XmlNode objSeat in objGrandNod)
                                        {
                                            switch (objSeat.Name )
                                            {
                                                case "description":
                                                    description = Util.GetValue(objSeat.InnerText, 40);
                                                    break;
                                                case "zone":
                                                    zone = Util.GetValue(objSeat.InnerText, 50);
                                                    break;
                                                case "price":
                                                    price = float.Parse(objSeat.InnerText);
                                                    break;
                                                case "presale":
                                                    presale = float.Parse(objSeat.InnerText);
                                                    break;
                                                case "commission":
                                                    commission = float.Parse(objSeat.InnerText);
                                                    break;
                                                case "iva":
                                                    iva = float.Parse(objSeat.InnerText);
                                                    break;
                                                case "reduction":
                                                    {
                                                        idReduction = Util.GetValue(objSeat.Attributes["id"].Value.ToString(), 50);
                                                        foreach (XmlNode objRed in objSeat)
                                                        {
                                                            if (objRed.Name == "description")
                                                            {
                                                                objRiduzioni = new Riduzioni();
                                                                objRiduzioni.idRiduzione = idReduction;
                                                                objRiduzioni.descrizione = Util.GetValue(objRed.InnerText, 50);
                                                                objListR.Add(objRiduzioni);
                                                            }
                                                        }
                                                        break;
                                                    }
                                            }
                                        }
                                        SalvaDati();

                                    }
                                }

                                logger.Debug("RichiediPosti. Salvataggio dati XML dei posti su Db in modo corretto.");

                                sReturn = xml;
                            }
                            catch (Exception ex)
                            {
                                sReturn = Util.GestioneErrore("bestseat", ERRORNEWTRANSAZIONE.ToString(), "Richiesta posti. Salvataggio transazione non eseguito. " + ex.Message );
                            }
                            break;
                        case ERRORNEWTRANSAZIONE :
                        default:
                            logger.Debug("RichiediPosti, Salvataggio transazione NON Eseguito. IdTransazione: " + idTransaction);
                            sReturn = Util.GestioneErrore("bestseat", ERRORNEWTRANSAZIONE.ToString(), "Richiesta posti. Salvataggio transazione non eseguito.");
                            break;

                    }
                    objTATran.Dispose();

                }
                else
                    sReturn = xml;

                return sReturn ;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return  Util.GestioneErrore("bestseat", ERRORNEWTRANSAZIONE.ToString(), "Richiesta posti. Salvataggio transazione non eseguito."); 
            }
        }

        /// <summary>
        /// Gestisce ilcomando richiesta Anagrafica.
        /// </summary>
        /// <param name="_Response">Oggetto response della pagina chiamante</param>
        /// <param name="_queryRequest">Parametri da inviare a Charta</param>
        /// <param name="pIdTrans">Id della transazione</param>
        /// <param name="nome">Nome da associare alla transazione</param>
        /// <param name="cognome">Cognome da associare alla transazione</param>
        /// <param name="telefono">Telefono da associare alla transazione</param>
        /// <param name="email">Email da associare alla transazione</param>
        /// <returns>Ritorna la string XML di Charta. In caso di errori ritorna l'errore che si è verificato</returns>
        public static string InvioAnagrafica (HttpResponse _Response, string _queryRequest, string pIdTrans, string nome , string cognome, string telefono, string email, string remoteUrl)
        {
            try
            {
                short? Risultato = 0;
                string sReturn = "";

                TransazioniTableAdapter objTrans = new TransazioniTableAdapter();
                objTrans.AnagraficaTransazione(pIdTrans, nome, cognome, telefono, email,ref Risultato);

                switch (Risultato)
                {
                    case OK:
                        logger.Debug("Invio Anagrafica, Salvataggio eseguito correttamente. IdTransazione: " + pIdTrans);
                        logger.Debug(remoteUrl + _queryRequest);

                        sReturn = Util.ChiamataProxy(_queryRequest, remoteUrl);

                        XmlDocument objDocument = new XmlDocument();
                        objDocument.LoadXml(sReturn);
                        XmlNodeList objNodeList = objDocument.SelectNodes("reply");
                        if (objNodeList != null && objNodeList.Count > 0)
                        {
                            //<reply command="customer" errcode="-1" errstring="Il comando non e' stato abilitato per l'utente" /> 
                            try
                            {
                                string sErr = objNodeList[0].Attributes["errcode"].Value.ToString();
                                if (!string.IsNullOrEmpty(sErr))
                                {
                                    //E' presente un errore , aggiorno lo stato dell'anagrafica
                                    objTrans.ErroreAnagrafica(pIdTrans, ref Risultato);
                                    switch (Risultato)
                                    {
                                        case OK:
                                            //non deve fare nulla
                                            break;
                                        default:
                                            sReturn = Util.GestioneErrore("cusotmer", ERRORINVIOANAGRAFICA.ToString(), "Invio Anagrafica, Update Errore Anagrafica non eseguito.");
                                            break;
                                    }
                                }
                            }
                            catch
                            {
                                //Non deve fare nulla, in questo caso è andato ok il comando anagrafica richiessta
                            }

                        }
                        break;

                    case ERRORINVIOANAGRAFICA:
                    default:
                        logger.Debug("Invio Anagrafica, Salvataggio NON Eseguito. IdTransazione: " + pIdTrans);
                        sReturn = Util.GestioneErrore("cusotmer", ERRORINVIOANAGRAFICA.ToString(), "Invio Anagrafica, Salvataggio transazione non eseguito.");
                        break;
                }

                return sReturn;
                
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("cusotmer", ERRORINVIOANAGRAFICA.ToString(), "Invio Anagrafica, Salvataggio transazione non eseguito."); ;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idTran"></param>
        /// <returns></returns>
        public static string StatoTran (string idTran)
        { 
            try
            {
                string sReturn = string.Empty;
                string svalue = "-1";
                string smsg = "";

                if ( ! string.IsNullOrEmpty(idTran) )
                {
                    StatoTranTableAdapter tabeStatoTran = new StatoTranTableAdapter();
                    Charta.StatoTranDataTable objDt =  tabeStatoTran.GetData(idTran);

                    foreach (ChartaDb.Charta.StatoTranRow r in objDt)
                    {
                        svalue = r.Stato.ToString();
                        smsg = r.messaggio;
                    }

                    // Vecchia gestione comenado transazione
                    //TransactionsTableAdapter tableAdapterTran = new TransactionsTableAdapter();
                    //DataRow[] dr = tableAdapterTran.GetData().Select(" transaction_id = '" + idTran + "' ");
                    //if (dr.Length >0 )
                    //{
                    //    svalue = dr[0]["stato"].ToString();
                    //}
                    //tableAdapterTran.Dispose();

                }
                
                sReturn += "<reply>";
                sReturn += "<StatoTran>" + Util.ConvertWithANDReplace(svalue) + "</StatoTran>";
                sReturn += "<Messaggio>" + Util.ConvertWithANDReplace(smsg) + "</Messaggio>";
                sReturn += "</reply>";

                return sReturn;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return Util.GestioneErrore("StatoTran", ERRORICHIESTASTATOTRAN.ToString(), "Errore richiesta stato transazione."); ;
            }
        }


        /// <summary>
        /// Inizializzazione dei dati da salvare
        /// </summary>
        private static void InitDati ()
        { 
            idSeat = 0 ;
            description = "";
            zone = "";
            price = 0; 
            presale = 0;
            commission = 0;
            iva = 0;
            idReduction = "";
            objListR.Clear() ;
        }

        /// <summary>
        /// Gestisce il salvataggio dati
        /// </summary>
        private static void SalvaDati ()
        {
            int idSeat = SalvaSeat();
            SalvaRiduzioni(idSeat);
        }

        /// <summary>
        /// Salva i dati nella Tabella Seat
        /// </summary>
        /// <returns></returns>
        private static int SalvaSeat ()
        {
            try
            {
                SeatTableAdapter objTASeat = new SeatTableAdapter();
                DataRow[] dr ;
                
                objTASeat.Insert(idSeat, idTransaction, description, zone, price, presale, commission, iva);
                dr = objTASeat.GetData().Select("transaction_id = '" + idTransaction + "' AND id_seat = " + idSeat );
                return ((Charta.SeatRow )dr[0]).id;

            }
            catch (Exception ex)
            {
                logger.Error("Error: GestioneTransazioni.SalvaSeat ", ex);
                throw new Exception("Errore durante il salvataggio dati. SalvaSeat", ex);
            }
        }


        /// <summary>
        /// Salva i dati nella tabella Riduzioni
        /// </summary>
        /// <param name="idTransazione"></param>
        private static void SalvaRiduzioni (int idSeat)
        {
            try
            {
                RiduzioneTableAdapter objTAR = new RiduzioneTableAdapter();

                foreach (Riduzioni objR in objListR)
                {
                    objTAR.Insert(idSeat, objR.idRiduzione, objR.descrizione);   
                }
                objTAR.Dispose();


            }
            catch (Exception ex)
            {
                logger.Error("Error: GestioneTransazioni.SalvaRiduzioni ", ex);
                throw new Exception("Errore durante il salvataggio dati. SalvaRiduzioni", ex);
            }
        }


    }


    /// <summary>
    /// 
    /// </summary>
    class Riduzioni
    {
        public string idRiduzione;
        public string descrizione;
    }
}
