using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Xml;
using System.Net;
using System.Text;
using System.Reflection; 

namespace ChartaWEB
{
    static public class Util
    {
        public static string ConvertWithANDReplace (string str)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter xmlTw = new XmlTextWriter(sw);
            xmlTw.WriteString(str);

            //return sw.ToString();
            return sw.ToString().Replace("&", "&amp;").Replace("'", "&apos;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        public static string Convert(string str)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter xmlTw = new XmlTextWriter(sw);
            xmlTw.WriteString(str);

            return sw.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objResp"></param>
        /// <returns></returns>
        public static string ChiamataProxy (string sQuery, string remoteUrl)
        {
            string sReturn = "";
            string url = remoteUrl + sQuery;
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Credentials = CredentialCache.DefaultCredentials;
            webRequest.Accept = "text/xml";
            HttpWebResponse remoteResp = (HttpWebResponse)webRequest.GetResponse();
            System.IO.StreamReader reader;
            using (reader = new StreamReader(remoteResp.GetResponseStream()))
            {
                StringBuilder sb = new StringBuilder();

                try
                {
                    while (!reader.EndOfStream)
                    {
                        sb.Append((char)reader.Read());
                    }
                }
                catch (System.IO.IOException)
                { }

                sReturn = sb.ToString();
            }

            remoteResp.Close();
            return sReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ErrorCode"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static string GestioneErrore (string cmd, string ErrorCode, string Error)
        {
            //StringBuilder outputBuilder = new StringBuilder();
            //string a = "<reply command=\"{0}\" errcode=\"{1}\" errstring=\"{2}\" /> ";
            //outputBuilder.AppendFormat(a, cmd, ErrorCode, Error);
            //return outputBuilder.ToString();

            string sReturn = string.Format("{{\"command\":{0}, \"errcode\":{1}, \"errstring\":{2} }}", Util.EncodeForJson(cmd), Util.EncodeForJson(ErrorCode), Util.EncodeForJson(Error));
            return sReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="svalue"></param>
        /// <param name="MaxCrt"></param>
        /// <returns></returns>
        public static string GetValue (string svalue, int MaxCrt)
        {
            if (svalue.Length > MaxCrt)
                return svalue.Substring(0, MaxCrt);
            else
                return svalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sValue"></param>
        /// <returns></returns>
        public static DateTime StringCitToDate (string sValue)
        {
            try
            {
                int D = int.Parse(sValue.Substring(0, 2));
                int M = int.Parse(sValue.Substring(3, 2));
                int Y = int.Parse(sValue.Substring(6, 2));
                int H = int.Parse(sValue.Substring(11, 2));
                int m = int.Parse(sValue.Substring(14, 2));
                int s = int.Parse(sValue.Substring(17, 2));

                DateTime dData = new DateTime(Y, M, D, H, m, s);
                return dData;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Data na string ala converte in formato compatibile con JSON, già delimitata da doppi apici.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string EncodeForJson(string s)
        {
            // sostituisce i doppi apici (") con \"
            //return string.Format("\"{0}\"", s.Replace("\"", "\\\""));
            return Newtonsoft.Json.JsonConvert.SerializeObject(s);
        }

        /// <summary>
        /// Concatena due path di un URL: aggiunge uno slash solo se necessario.
        /// </summary>
        /// <param name="urlPath1"></param>
        /// <param name="urlPath2"></param>
        /// <returns></returns>
        public static string ConcatUrlPath(string urlPath1, string urlPath2)
        {
            string risultato = urlPath1;
            if (risultato.EndsWith("/") == false)
            {
                risultato += "/";
            }
            risultato += urlPath2;
            return risultato;
        }

        /// <summary>
        /// Converte una stringa XML in una stringa JSON secondo la logica seguente.
        /// Viene parsificato tutto l'xml.
        /// Ogni nodo viene convertito in un oggetto JSON di tipo {n:"tagName", v:"value", m:[{n:"attributo",v:"valore"}]}.
        /// Se un nodo XML ne contiene un altro, quest'ultimo diventa un suo membro:
        /// {n:"tagName", v:"value", m:[{n:"attributo",v:"valore"}, {n:"childTagName", v:"childValue", m:[{n:"childAttribute", v:"childAttributeValue"}  ] }]}.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static string XmlToJson(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return XmlNodeToJson(doc.DocumentElement);
        }

        /// <summary>
        /// Funzione ricorsiva per convertire un nodo xml in una stringa json.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static string XmlNodeToJson(XmlNode node)
        {
            string risultato = "";
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            // se il nodo ha la proprietà Attributes a null, allora è un testo o qualcosa di strano e non lo considero
            if (node.Attributes != null)
            {
                sb.AppendFormat("{{\"n\":\"{0}\",\"v\":\"{1}\",\"m\":[", node.Name, node.InnerText); // json start
                foreach (XmlAttribute attr in node.Attributes)
                {
                    if (isFirst == false)
                    {
                        sb.Append(",");
                    }
                    else
                    {
                        isFirst = false;
                    }
                    sb.AppendFormat("{{\"n\":\"{0}\",\"v\":\"{1}\"}}", attr.Name, attr.Value);
                }
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (isFirst == false)
                    {
                        sb.Append(",");
                    }
                    else
                    {
                        isFirst = false;
                    }
                    sb.Append(XmlNodeToJson(child));
                }
                sb.Append("]}"); // json end
            }
            risultato = sb.ToString();

            //elimina eventuali elenchi vuoti di membri
            risultato = risultato.Replace(",\"m\":[]", "");
            return risultato;
        }
    }
}
