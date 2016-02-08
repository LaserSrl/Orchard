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
            StringBuilder outputBuilder = new StringBuilder();

            string a = "<reply command=\"{0}\" errcode=\"{1}\" errstring=\"{2}\" /> ";
            outputBuilder.AppendFormat(a, cmd, ErrorCode, Error);

            return outputBuilder.ToString();

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

       

    }
}
