using System;
using System.Collections.Generic;
using System.Web;
using log4net;
using System.Xml;



namespace ChartaWEB
{
    public class SMS
    {
        private static readonly ILog logger = LogManager.GetLogger("SMS");
        private string _FileTicketPull;

        private string _Telefono;
        private string _TestoSMS;
        private DateTime _DataOraRicezione;
        private string _DataOraRicezioneString;

       

        #region [ Property ]
        public string Telefono
        {
            get { return _Telefono; }
        }

        public string TestoSMS
        {
            get { return _TestoSMS; }
        }

        public DateTime DataOraRicezione
        {
            get { return _DataOraRicezione; }
        }

        public string DataOraRicezioneString
        {
            get { return _DataOraRicezioneString; }
            set { _DataOraRicezioneString = value; }
        }

        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sFile"></param>
        public SMS (string sFile)
        {
            _FileTicketPull = sFile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool GestioneRicezione ()
        {
            try
            {
                XmlDocument objXML = new XmlDocument();
                objXML.Load(_FileTicketPull);

                XmlNodeList objNodeList = objXML.SelectNodes("XML");
                if (objNodeList == null || objNodeList.Count  <=0 )
                {
                    return false;
                }

                foreach (XmlNode objNodeGrand in objNodeList  )
                {
                    foreach (XmlNode objNode in objNodeGrand)
                    {
                        switch (objNode.Name)
                        {
                            case "TESTO":
                                {
                                    _TestoSMS = objNode.InnerText;
                                    break;
                                }
                            case "FROM":
                                {
                                    _Telefono = objNode.InnerText;
                                    break;
                                }
                            case "ORA_RICEZIONE":
                                {
                                    _DataOraRicezioneString = objNode.InnerText;
                                    _DataOraRicezione = Util.StringCitToDate(_DataOraRicezioneString);
                                    break;
                                }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Gestione SMS errore." , ex);
                return false;
            }
        }

    }
}
