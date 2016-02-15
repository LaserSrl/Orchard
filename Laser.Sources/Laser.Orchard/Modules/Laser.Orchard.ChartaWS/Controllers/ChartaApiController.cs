using ChartaWEB;
using log4net;
using MovinBoxHandler;
using Orchard;
using Orchard.Email.Models;
using Orchard.Messaging.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http;
using Orchard.ContentManagement;
using Orchard.Email.Services;
using System.Collections.Generic;
using Laser.Orchard.ChartaWS.Models;

namespace Laser.Orchard.ChartaWS.Controllers
{
    public class ChartaApiController : ApiController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ChartaApiController));
        private readonly IOrchardServices _orchardServices;
        private readonly IMessageService _messageService;

        public ChartaApiController(IOrchardServices orchardServices, IMessageService messageService)
        {
            _orchardServices = orchardServices;
            _messageService = messageService;
        }

        public HttpResponseMessage Get()
        {
            string xml = "";
            string sXml = "";
            string id = "";
            string cmd = "";
            string idorg = null;
            string vcode = null;
            string comune = null;
            string provincia = null;
            string idtitolo = null;
            string quando = null;
            string catid = null;
            string sottocatid = null;
            string citta = null;
            string nome = null;
            string cognome = null;
            string telefono = null;
            string idTransazione = null;
            string email = null;
            string pcode = null;
            string pdata = null;
            string titoloartista = null;
            string deviceToken = null;
            string tipoDevice = null;
            string lat = null;
            string lon = null;
            string idprofila = null;
            string luogo = null;
            string prod = null;

            logger.Info("");
            logger.Info("");

            logger.Info("******  Inizio gestione Chiamata");
            WriteSrcIP();
            var Request = HttpContext.Current.Request;
            var chartaConfig = _orchardServices.WorkContext.CurrentSite.As<ChartaSiteSettingsPart>();

            #region [ Leggo gli eventuali parametri ]

            if (Request["id"] != null) id = Request["id"];
            //logger.Info("Parametro Id :" + id);
            if (Request["cmd"] != null) cmd = Request["cmd"];
            logger.Info("Parametro cmd :" + cmd);
            if (Request["idorg"] != null) idorg = Request["idorg"];
            //logger.Info("Parametro idorg :" + idorg);
            if (Request["vcode"] != null) vcode = Request["vcode"];
            //logger.Info("Parametro vcode :" + vcode);
            if (Request["comune"] != null) comune = Request["comune"];
            //logger.Info("Parametro comune :" + comune);
            if (Request["provincia"] != null) provincia = Request["provincia"];
            //logger.Info("Parametro provincia :" + provincia);
            if (Request["idtitolo"] != null) idtitolo = Request["idtitolo"];
            //logger.Info("Parametro idtitolo :" + idtitolo);
            if (Request["quando"] != null) quando = Request["quando"];
            //logger.Info("Parametro quando :" + quando);
            if (Request["citta"] != null) citta = Request["citta"];
            //logger.Info("Parametro citta :" + citta);
            if (Request["catid"] != null) catid = Request["catid"];
            //logger.Info("Parametro catid :" + catid);
            if (Request["sottocatid"] != null) sottocatid = Request["sottocatid"];
            //logger.Info("Parametro sottocatid :" + sottocatid);
            if (Request["Firstname"] != null) nome = Request["Firstname"];
            //logger.Info("Parametro Firstname :" + nome);
            if (Request["Lastname"] != null) cognome = Request["Lastname"];
            //logger.Info("Parametro Lastname :" + cognome);
            if (Request["custref"] != null) idTransazione = Request["custref"];
            //logger.Info("Parametro custref :" + idTransazione);
            if (Request["Telephone"] != null) telefono = Request["Telephone"];
            //logger.Info("Parametro Telephone :" + telefono);
            if (Request["Email"] != null) email = Request["Email"];
            //logger.Info("Parametro Email :" + email);
            if (Request["pcode"] != null) pcode = Request["pcode"];
            //logger.Info("Parametro pcode :" + pcode);
            if (Request["pdata"] != null) pdata = Request["pdata"];
            //logger.Info("Parametro pdata :" + pdata);
            if (Request["titolo_artista"] != null) titoloartista = '%' + Request["titolo_artista"] + '%';
            //logger.Info("Parametro titolo_artista :" + titoloartista);
            if (Request["device_token"] != null) deviceToken = Request["device_token"];
            //logger.Info("Parametro titolo_artista :" + titoloartista);
            if (Request["tipo_device"] != null) tipoDevice = Request["tipo_device"];
            //logger.Info("Parametro titolo_artista :" + titoloartista);
            if (Request["lat"] != null) lat = Request["lat"];
            //logger.Info("Parametro titolo_artista :" + titoloartista);
            if (Request["lon"] != null) lon = Request["lon"];
            //logger.Info("Parametro titolo_artista :" + titoloartista);
            if (Request["idprofila"] != null) idprofila = Request["idprofila"];
            //logger.Info("Parametro idprofila:" + idprofila );
            if (Request["luogo"] != null) luogo = Request["luogo"];
            //logger.Info("Parametro luogo:" + luogo );
            if (Request["prod"] != null) prod = Request["prod"];
            //logger.Info("Parametro prod:" + prod );

            #endregion

            Security objSec = new Security(chartaConfig);
            objSec.Id = id;
            objSec.Cmd = cmd;

            objSec.VerificaDati();

            if (objSec.CodError == 0)
            {
                logger.Debug("Comando riconosciuto - Sarà gestito internamente su questo server");
                //Gestione comandi
                Command objCmd = new Command(chartaConfig);
                objCmd.Idorg = idorg;
                objCmd.Code = vcode;
                objCmd.Comune = comune;
                objCmd.Provincia = provincia;
                objCmd.Idtitolo = idtitolo;
                objCmd.Quando = quando;
                objCmd.Citta = citta;
                objCmd.Catid = catid;
                objCmd.Sottocatid = sottocatid;
                objCmd.QueryRequest = Request.Url.Query;
                objCmd.Response = null; // questa property non viene mai usata
                objCmd.IdTransazione = idTransazione;
                objCmd.Nome = nome;
                objCmd.Cognome = cognome;
                objCmd.Email = email;
                objCmd.Telefono = telefono;
                objCmd.Pcode = pcode;
                objCmd.Pdata = pdata;
                objCmd.TitoloArtista = titoloartista;

                objCmd.DeviceToken = deviceToken;
                objCmd.TipoDevice = tipoDevice;
                objCmd.Lat = lat;
                objCmd.Lon = lon;
                objCmd.Luogo = luogo;
                objCmd.Idprofila = idprofila;
                objCmd.Prod = prod;

                LogDebugObject(objCmd);

                sXml = objCmd.GestioneComando(cmd);

                logger.Debug("dimensione risposta ->" + (sXml.Length / 1000) + "KB");

                if (sXml.Length < 2000)
                {
                    logger.Debug("xml ->" + sXml);
                }
                xml = sXml;
            }
            else
            {
                if (objSec.CodError == -2)
                {
                    try
                    {
                        logger.Debug("Comando non riconosciuto - Verrà inoltrato al WS di CHARTA");
                        logger.Debug(chartaConfig.RemoteUrl + Request.Url.Query);

                        xml = Util.ChiamataProxy(Request.Url.Query, chartaConfig.RemoteUrl);

                        if (xml.Length < 2000)
                        {
                            logger.Debug("ProxyResponseXml ->" + xml);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Errore in Default.aspx.", ex);
                    }
                }
                else
                {
                    sXml = GestioneErrore(objSec);
                    logger.Error(sXml);
                    xml = sXml;
                }
            }

            logger.Info("****** Fine gestione Chiamata");

            // create response and return 
            var result = new HttpResponseMessage(HttpStatusCode.OK);

            // converte il risultato in formato json se necessario
            if (xml.StartsWith("<"))
            {
                xml = Util.XmlToJson(xml);
            }

            //result.Content = new System.Net.Http.StringContent(xml, Encoding.GetEncoding(1252), "text/xml");
            result.Content = new System.Net.Http.StringContent(xml, Encoding.UTF8, "application/json");
            return result;
        }

        [HttpPost]
        public HttpResponseMessage Post()
        {
            string message = "";
            try
            {
                var Request = HttpContext.Current.Request;
                var chartaConfig = _orchardServices.WorkContext.CurrentSite.As<ChartaSiteSettingsPart>();
                HttpFileCollection _files = HttpContext.Current.Request.Files;

                logger.Info(_files);
                logger.Info("Parametri count: " + Request.Params.Count);
                logger.Info("Parametri stringa: " + Request.Params.ToString());

                string[] par = Request.Params.AllKeys;
                for (int i = 0; i < par.Length; i++)
                {
                    logger.Info("Key: " + par[i]);
                    logger.Info("Valore: " + Request.Params[par[i]]);
                }

                System.Web.HttpPostedFile _postedFile = null;
                String _fileName, _fileExtension;

                String filename = string.Empty;
                String data = string.Empty;
                String user = string.Empty;
                String password = string.Empty;

                user = "";
                password = "";

                ////***************************************************************************************
                //Letto dal file Web.config va salvato nella directory upload sita 
                //dove si trova il servizio sul server.....
                //*****************************************************************************************
                String percorsoUpload = string.Empty;

                //******************************************************************************************
                //il nome del file è passato compresa l'estensione che deve essere txt
                //verificare se l'estensione è txt altrimenti scartarlo.		       
                //nome file: IDENTE_DATAORA_NOMEFILE.txt
                //ggmmaaaahhmm
                //******************************************************************************************
                data = DateTime.Now.ToString("ddMMyyyyHHmmss");

                _postedFile = _files[0];

                logger.Info(_postedFile);

                _fileName = System.IO.Path.GetFileName(_files[0].FileName);

                logger.Info(_fileName);

                _fileExtension = System.IO.Path.GetExtension(_fileName);

                logger.Info(_fileExtension);

                percorsoUpload = chartaConfig.SaveFilePath + "\\";

                filename = data + "_" + _fileName;

                FileInfo file = new FileInfo(percorsoUpload + filename);
                if (file.Exists)
                {
                    file.Delete();
                }

                logger.Info(percorsoUpload + filename);
                _postedFile.SaveAs(percorsoUpload + filename);

                message = "Code=[0] il file è stato trasferito correttamente";
            }
            catch (Exception ex)
            {
                logger.Debug(ex);
                logger.Info(ex);
                logger.Warn(ex);
                logger.Fatal(ex);

                message = "Code=[-3] Errore durante il trasferimento del file";
            }

            // create response and return 
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new System.Net.Http.StringContent(message, Encoding.UTF8, "text/plain");
            return result;
        }

        [HttpPost]
        public HttpResponseMessage PaypalNotification()
        {
            string message = "";
            try
            {
                var Request = HttpContext.Current.Request;
                var chartaConfig = _orchardServices.WorkContext.CurrentSite.As<ChartaSiteSettingsPart>();
                var paypalEncoding = Request.ContentEncoding;

                //Post back to either sandbox or live
                string replyTo = chartaConfig.ReplyToPaypalUrl;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;
                HttpWebRequest req = HttpWebRequest.CreateHttp(replyTo); // (HttpWebRequest)WebRequest.Create(replyTo);
                
                //Set values for the request back
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                byte[] param = HttpContext.Current.Request.BinaryRead(HttpContext.Current.Request.ContentLength);
                
                //string strRequest = Encoding.ASCII.GetString(param);
                string strRequest = paypalEncoding.GetString(param);
                logger.Debug("strRequest: " + strRequest);

                //strRequest += "&cmd=_notify-validate";
                strRequest = "cmd=_notify-validate&" + strRequest;
                byte[] buffer = paypalEncoding.GetBytes(strRequest);
                req.ContentLength = buffer.LongLength;
                
                //Send the request to PayPal...
                string strResponse = "";
                Stream streamOut = req.GetRequestStream();
                streamOut.Write(buffer, 0, buffer.Length);
                streamOut.Close();

                // ... and get the response
                using (StreamReader streamIn = new StreamReader(req.GetResponse().GetResponseStream()))
                {
                    strResponse = streamIn.ReadToEnd();
                }

                logger.Debug("strResponse: " + strResponse);

                if (strResponse == "VERIFIED")
                {
                    logger.Debug("strResponse VERIFIED");

                    //check the payment_status is Completed
                    /*
                    http://localhost:55403/PaypalNotification.aspx?test_ipn=1&payment_type=echeck&payment_date=00%3A45%3A26+Dec+11%2C+2012+PST&payment_status=Completed&address_status=confirmed&payer_status=verified&first_name=John&last_name=Smith&payer_email=buyer%40paypalsandbox.com&payer_id=TESTBUYERID01&address_name=John+Smith&address_country=United+States&address_country_code=US&address_zip=95131&address_state=CA&address_city=San+Jose&address_street=123%2C+any+street&business=seller%40paypalsandbox.com&receiver_email=seller%40paypalsandbox.com&receiver_id=TESTSELLERID1&residence_country=US&item_name=something&item_number=AK-1234&quantity=1&shipping=3.04&tax=2.02&mc_currency=EUR&mc_fee=0.44&mc_gross=12.34&txn_type=web_accept&txn_id=261211845&notify_version=2.1&custom=alerole&invoice=abc1234&charset=windows-1252&verify_sign=AoD5M85lc03Jpv0Uqo8jux3SXiPCA2E-HT-HhdjNtZ6zdNHJ797zaiLi
                    */
                    string[] custom_field = null;
                    if (Request.Params["custom"] != null)
                    {
                        logger.Debug("custom in Request.Params = " + Request.Params["custom"].ToString());
                        custom_field = Request.Params["custom"].ToString().Split(';');
                    }
                    else if (Request["custom"] != null)
                    {
                        logger.Debug("custom in Request = " + Request["custom"].ToString());
                        custom_field = Request["custom"].ToString().Split(';');
                    }

                    if (custom_field != null)
                    {
                        //controllo il campo custom
                        string tranId = custom_field[0];
                        logger.Debug("cerco questo transIdCharta = " + tranId);

                        //Charta.GetTransactionRow transRow = MovinBoxHandler.ChartaDBTrans.GetTransaction(tranId);
                        ChartaDb.ChartaTableAdapters.TransactionsTableAdapter ta = new ChartaDb.ChartaTableAdapters.TransactionsTableAdapter();
                        ChartaDb.Charta.TransactionsDataTable dt = ta.GetDataByTransactionId(tranId);

                        //if (transRow == null)
                        if (dt.Rows.Count == 0)
                        {
                            logger.Warn("Transaction non trovata: " + tranId);
                            EventViewer.EventViewer.EventViewerError("ChartaWEB PaypalNotification Transaction non trovata: " + tranId);
                        }
                        else
                        {
                            ChartaDb.Charta.TransactionsRow transRow = (ChartaDb.Charta.TransactionsRow)dt.Rows[0];
                            string status = "";
                            if (Request["payment_status"] != null)
                            {
                                logger.Debug("payment_status = " + Request["payment_status"].ToString());
                                status = Request["payment_status"].ToString().ToLower();
                            }
                            else if (Request["status"] != null)
                            {
                                logger.Debug("status = " + Request["status"].ToString());
                                status = Request["status"].ToString().ToLower();
                            }

                            switch (status)
                            {
                                case "completed":
                                    //se in stato completed, verificare che la transazione non sia già stata settata a 5 (billbuyreq_ok).
                                    //se già billbuyreq_ok non richiamare la commit tansaction, non bisogna più fare niente nè verso charta nè su db nostro (fare solo logging)
                                    if (transRow.Stato == (int)ChartaDBTrans.StatiTran.billbuyreq_ok)
                                    {
                                        //già confermata, non devo fare niente
                                        logger.Info("stato transazione già a 5 confermata, non faccio niente: " + tranId);
                                    }
                                    else
                                    {
                                        int serviceRetryCount = chartaConfig.ServiceRetryCount;
                                        string ChartaTranCommitURL = chartaConfig.ChartaTranCommitUrl;
                                        ChartaProxyPaypal _chartaProxy = new ChartaProxyPaypal(serviceRetryCount, ChartaTranCommitURL);
                                        logger.Info("call CommitTransaction: " + tranId);
                                        if (!_chartaProxy.CommitTransaction(tranId))
                                        {
                                            logger.Info("AggiornaStatoTrans confirmtran_ko: " + tranId);
                                            AggiornaStatoTrans(tranId, ChartaDBTrans.StatiTran.billbuyreq_ko);

                                            try
                                            {
                                                //gestione try catch si può eliminare una volta collaudato con altre casistiche di paypal
                                                string reasonCode = "";
                                                if (Request["reason_code"] != null)
                                                {
                                                    logger.Debug("reason_code = " + Request["reason_code"].ToString());
                                                    reasonCode = Request["reason_code"].ToString().ToLower();
                                                }

                                                if (reasonCode.CompareTo("refund") != 0)
                                                {
                                                    logger.Info("Spedisco mail a Vivaticket, biglietto addebitato ma posti non confermati");
                                                    //invio mail a regione con body testo sms, subject codice univoco e numero di tel
                                                    //invio nel testo della mail la request di paypal per verifica più semplice
                                                    string oggettoMail = ReplaceCampi(chartaConfig.MailSubject, transRow, "");
                                                    string testoMail = ReplaceCampi(chartaConfig.MailBody, transRow, strRequest);

                                                    SpedisciMail(oggettoMail, testoMail, chartaConfig.MailTo, chartaConfig.MailCc);
                                                }
                                                else
                                                {
                                                    logger.Info("Ricevuto notifica operazione di refund");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.Error("Errore gestione invio mail a Vivaticket: " + ex);
                                                EventViewer.EventViewer.EventViewerError("Errore gestione invio mail a Vivaticket: " + ex.Message + " " + ex.StackTrace);
                                            }
                                        }
                                        else
                                        {
                                            logger.Info("AggiornaStatoTrans confirmtran_ok: " + tranId);
                                            AggiornaStatoTrans(tranId, ChartaDBTrans.StatiTran.billbuyreq_ok);
                                        }
                                    }
                                    break;
                                case "denied":
                                case "expired":
                                case "failed":
                                    logger.Info("AggiornaStatoTrans confirmtran_ko: " + tranId);
                                    AggiornaStatoTrans(tranId, ChartaDBTrans.StatiTran.billbuyreq_ko);
                                    break;
                                default:
                                    logger.Info("status non riconosciuto: " + status);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        logger.Info("Param custom non trovato");
                    }
                }
                else if (strResponse == "INVALID")
                {
                    //log for manual investigation
                    logger.Error("log for manual investigation strResponse INVALID");
                }
                else
                {
                    //log response/ipn data for manual investigation
                    logger.Error("log response/ipn data for manual investigation strResponse INVALID");
                }
            }
            catch (Exception ex)
            {
                logger.Error("ChartaWEB PaypalNotification: " + ex);
                //EventViewer.EventViewer.EventViewerError("ChartaWEB PaypalNotification: " + ex.Message + " " + ex.StackTrace);
            }

            // create response and return 
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new System.Net.Http.StringContent(message, Encoding.UTF8, "text/plain");
            return result;
        }

        private string ReplaceCampi(string str, ChartaDb.Charta.TransactionsRow transRow, string paypalRequest)
        {
            try
            {
                str = str.Replace("[PH_TRANSACTION_ID]", transRow.transaction_id)
                        .Replace("[PH_PAYPAL_REQUEST]", paypalRequest)
                        .Replace("[PH_NOME_COGNOME]", transRow.Nome + " " + transRow.Cognome)
                        .Replace("[PH_TELEFONO]", transRow.Telefono)
                        .Replace("[PH_EMAIL]", transRow.email)
                        //.Replace("[PH_DATA]", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss").Replace(".", ":"))
                        .Replace("[PH_DATA]", transRow.tstamp.ToString("dd/MM/yyyy HH:mm:ss").Replace(".", ":"))
                        .Replace("[PH_NEWLINE]", "\r\n");
                return str;
            }
            catch (Exception ex)
            {
                logger.Error("ChartaWEB ReplaceCampi: " + ex);
                return str;
            }
        }

        private void SpedisciMail(string oggetto, string testo, string to, string cc)
        {
            try
            {
                //MailMessage mailMessage = new MailMessage();
                //string[] sep = { ";" };
                //string[] toList = to.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                //foreach (string s in toList)
                //{
                //    mailMessage.To.Add(s);
                //}
                //string[] ccList = cc.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                //foreach (string s in ccList)
                //{
                //    mailMessage.CC.Add(s);
                //}
                //mailMessage.Subject = oggetto;
                //mailMessage.IsBodyHtml = false;
                //mailMessage.Body = testo;
                //mailMessage.Priority = MailPriority.High;

                //// crea il client SMTP utilizzando i settings del modulo Orchard.Email
                //var smtp = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
                //System.Net.Mail.SmtpClient client = new SmtpClient();
                //client.Host = smtp.Host;
                //client.Port = smtp.Port;
                //client.EnableSsl = smtp.EnableSsl;
                //if (smtp.RequireCredentials)
                //{
                //    client.UseDefaultCredentials = false;
                //    client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);
                //}
                //else
                //{
                //    client.UseDefaultCredentials = true;
                //}
                //mailMessage.From = new MailAddress(smtp.Address);
                //client.Send(mailMessage);

                //*******************************************************
                // utilizza il servizio mail di Orchard
                var data = new Dictionary<string, object>();
                data.Add("Subject", oggetto);
                data.Add("Body", testo);
                data.Add("Recipients", to); // to.Replace(';', ','));
                data.Add("CC", cc); // cc.Replace(';', ','));
                _messageService.Send(SmtpMessageChannel.MessageType, data);
            }
            catch (Exception e)
            {
                logger.Error("Errore invio mail con testo " + testo + " " + e);
                EventViewer.EventViewer.EventViewerError("Errore invio mail con testo " + testo + " " + e);
            }
        }

        private void AggiornaStatoTrans(string tranId, ChartaDBTrans.StatiTran statoTrans)
        {
            try
            {
                ChartaDBTrans.UpdateStatoTran(tranId, statoTrans);
            }
            catch (Exception ex)
            {
                logger.Error("PaypalNotification UpdateStatoTran: " + ex);
                EventViewer.EventViewer.EventViewerError("ChartaWEB PaypalNotification UpdateStatoTran: " + ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSec"></param>
        /// <returns></returns>
        private string GestioneErrore(Security pSec)
        {
            return Util.GestioneErrore(pSec.CodCmd, pSec.CodError.ToString(), pSec.ErrorDescr);
        }

        private void LogDebugObject(Command objCmd)
        {
            foreach (PropertyInfo info in objCmd.GetType().GetProperties())
            {
                if (info.CanRead)
                {

                    object value = info.GetValue(objCmd, null);
                    if (value != null)
                    {
                        if (value.ToString().Length > 0)
                        {
                            logger.Debug(String.Format("{0} -> {1}", info.Name, value));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void WriteSrcIP()
        {
            //string appData = Server.MapPath("~/App_Data/");
            //if (!appData.EndsWith("\\"))
            //    appData += "\\";
            //System.IO.StreamWriter file = new System.IO.StreamWriter(appData + "ip_addr_log.txt", true);
            //file.WriteLine(DateTime.Now + ": " + Request.UserHostAddress);
            //file.Close();

            logger.Info("Richiesta arrivata dall'indirizzo IP: " + HttpContext.Current.Request.UserHostAddress);
        }
    }
}