using Laser.Orchard.Mobile.ViewModels;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace Laser.Orchard.Mobile.Controllers
{
    public class TransferDeliveryReportController : Controller
    {
        public ILogger Logger { get; set; }

        public TransferDeliveryReportController() {
            Logger = NullLogger.Instance;
        }


        //
        // GET: /TransferDeliveryReport/
        public ContentResult Update()
        {
            string strResp = "OK";

            try {

                if (Request.InputStream != null && Request.InputStream.ToString().CompareTo("") != 0) {

                    // Read XML posted via HTTP
                    System.IO.StreamReader reader = new System.IO.StreamReader(Request.InputStream);

                    String xmlData = reader.ReadToEnd();
                    reader.Close();

                    Logger.Debug("Contenuto richiesta: " + xmlData);

                    if (xmlData.CompareTo("") != 0) {
                        Encoding usedEncoding = Encoding.UTF8;
                        
                        // Deserializza oggetto postato
                        Logger.Debug("deserializzo il messaggio");
                        XmlSerializer serializer = new XmlSerializer(typeof(DeliveryReportVM));

                        MemoryStream ms = new MemoryStream(usedEncoding.GetBytes(xmlData));
                        object objReport = serializer.Deserialize(ms);

                        Logger.Debug("messaggio deserializzato");

                        DeliveryReportVM report = (DeliveryReportVM)objReport;

                        Logger.Debug("Driver Id: " + report.DriverId);
                        Logger.Debug("Id Sms: " + report.MessageId + " Identifier: " + report.MessageIdentifier);
                        Logger.Debug("Testo Sms: " + report.TestoSms);
                        Logger.Debug("Stato Sms: " + report.Stato);
                    }
                }
            } 
            catch (Exception ex) 
            {
                strResp = "KO";
                Logger.Error("TransferDeliveryReportController Update: " + ex);
            }

            return Content(strResp);
        }
	}
}