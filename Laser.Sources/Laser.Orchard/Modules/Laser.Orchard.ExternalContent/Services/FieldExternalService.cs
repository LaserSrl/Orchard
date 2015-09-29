using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Tokens;
//using RazorEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Numerics;
using System.Web;
using System.Collections.Specialized;
//using System.Web.Razor;

namespace Laser.Orchard.ExternalContent.Services {
    public interface IFieldExternalService : IDependency {
        dynamic GetContentfromField(Dictionary<string, object> contesto, string field, string nomexlst, string contentType = "", HttpVerbOptions httpMethod = HttpVerbOptions.GET, HttpDataTypeOptions httpDataType = HttpDataTypeOptions.JSON, string bodyRequest = "");
        string GetUrl(Dictionary<string, object> contesto, string externalUrl);
    }

    public class ExtensionObject {
        public string HtmlEncode(string input) {
            return System.Web.HttpUtility.HtmlEncode(input);
        }
        public string HtmlDecode(string input) {
            return (System.Web.HttpUtility.HtmlDecode(input)).Replace("\t", " ");
        }
        public string GuidToId(string input) {
            BigInteger huge = BigInteger.Parse('0' + input.Replace("-", "").Replace(" ", ""), NumberStyles.AllowHexSpecifier);
            return (huge + 5000000).ToString();
        }

        public string ToTitleCase(string input) {
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            return myTI.ToTitleCase(input);
        }

    }

    public class FieldExternalService : IFieldExternalService {

        private readonly ITokenizer _tokenizer;
        private readonly ShellSettings _shellSetting;
        private readonly IWorkContextAccessor _workContext;
        public ILogger Logger { get; set; }
        public FieldExternalService(
            ITokenizer tokenizer
            , ShellSettings shellSetting
            , IWorkContextAccessor workContext
            ) {
            _tokenizer = tokenizer;
            _shellSetting = shellSetting;
            _workContext = workContext;
            Logger = NullLogger.Instance;
        }

        public string GetUrl(Dictionary<string, object> contesto, string externalUrl) {
            var tokenizedzedUrl = _tokenizer.Replace(externalUrl, contesto, new ReplaceOptions { Encoding = ReplaceOptions.UrlEncode });
            tokenizedzedUrl = tokenizedzedUrl.Replace("+", "%20");
        //    tokenizedzedUrl = tokenizedzedUrl.Replace(".", "%2E");
           Uri tokenizedzedUri;
           try {        
               tokenizedzedUri = new Uri(tokenizedzedUrl);
           }
           catch {
               // gestisco il caso in cui passo un'url e non i parametri di un'url
               tokenizedzedUrl = _tokenizer.Replace(externalUrl, contesto, new ReplaceOptions { Encoding = ReplaceOptions.NoEncode });
               tokenizedzedUri = new Uri(tokenizedzedUrl);
       
           }
               //}
           //catch {
           //    string[] partiurl= tokenizedzedUrl.Split('.');
           //    string nome_dominio = partiurl[0];
           //    for (int w = 1; w < partiurl.Count(); w++) {
           //        if (partiurl[w].IndexOf("%2f") < 0)
           //            nome_dominio +="."+ partiurl[w];
           //        else {
           //            nome_dominio +="."+ partiurl[w].Split(new string[] { "%2f" }, StringSplitOptions.None)[0];
           //            break;
           //        }
           //    }
           //    Uri Urifirst = new Uri(nome_dominio);
           //    string urlpath = tokenizedzedUrl.Substring(nome_dominio.Length, 10000);
           //    Uri Second = new Uri(Urifirst, urlpath);
           //    var uriBuilder = new UriBuilder(tokenizedzedUrl);
           //    // tokenizedzedUri = new Uri(uriString,uriKind);
           //    Uri myfinalUrl = uriBuilder.Uri;
           //    Uri.TryCreate(tokenizedzedUrl, UriKind.Absolute, out tokenizedzedUri);
           //}
           var finalUrl = String.Format("{0}{1}{2}{3}", tokenizedzedUri.Scheme, Uri.SchemeDelimiter, tokenizedzedUri.Authority, tokenizedzedUri.AbsolutePath);
            var queryStringParameters = tokenizedzedUri.Query.Split('&');
            var i = 0;
            foreach (var item in queryStringParameters) {
                if (!item.Trim().EndsWith("=")) {
                    finalUrl += ((i == 0 ? "?" : "&") + item.Replace("?", ""));
                    i++;
                }

            }
            return finalUrl;
        }
        private JObject jsonflusher(JObject jsonObject) {
            JObject newJsonObject = new JObject();
            //JObject newJsonObject = new JObject();
            JProperty property;
            foreach (var token in jsonObject.Children()) {
                if (token != null) {
                    property = (JProperty)token;
                    if (property.Value.Children().Count() == 0)
                        newJsonObject.Add(property.Name.Replace(" ", ""), property.Value);
                    else if (property.Value.GetType().Name == "JArray") {
                        JArray myjarray = new JArray();
                        foreach (var arr in property.Value) {
                            myjarray.Add(jsonflusher((JObject)arr));

                        }
                        newJsonObject.Add(property.Name, myjarray);
                        // newJsonObject.Add(property.Name, jsonflusher((JObject)property.Value));
                    } else if (property.Value.GetType().Name == "JObject") {
                        newJsonObject.Add(property.Name.Replace(" ", ""), jsonflusher((JObject)property.Value));
                    }
                }
            }
            return newJsonObject;

        }
        public dynamic GetContentfromField(Dictionary<string, object> contesto, string externalUrl, string nomexlst, string contentType = "", HttpVerbOptions httpMethod = HttpVerbOptions.GET, HttpDataTypeOptions httpDataType = HttpDataTypeOptions.JSON, string bodyRequest = "")
        {
            dynamic ci = null;
            string UrlToGet = "";
            try {
                UrlToGet = GetUrl(contesto, externalUrl);
                string webpagecontent = GetHttpPage(UrlToGet, httpMethod, httpDataType, bodyRequest).Trim();
                if (!webpagecontent.StartsWith("<")) {
                    if (webpagecontent.StartsWith("[")) {
                        webpagecontent = String.Concat("{\"", nomexlst, "List", "\":", webpagecontent, "}");
                    }


                    JObject jsonObject = JObject.Parse(webpagecontent);
                    JObject newJsonObject = new JObject();
                    newJsonObject = jsonflusher(jsonObject);
                    webpagecontent = newJsonObject.ToString();
                    webpagecontent = JsonConvert.DeserializeXmlNode(webpagecontent, "root").InnerXml;
                }


                ci = XsltTransform(webpagecontent.Replace(" xmlns=\"\"", ""), nomexlst, contentType);
           
             } catch (Exception ex) {
                Logger.Error(ex, UrlToGet);
            }
            return (ci);
        }




        private dynamic XsltTransform(string xmlpage, string xsltname, string contentType = "") {
            string output = "", myXmlFileMoreSpecific, myXmlFileLessSpecific, myXmlFile;
            var namespaces = this.GetType().FullName.Split('.').AsEnumerable();
            namespaces = namespaces.Except(new string[] { this.GetType().Name });
            namespaces = namespaces.Except(new string[] { namespaces.Last() });
            var area = string.Join(".", namespaces);
            // se esiste un xslt chiamato {ContentType}.{FieldName}.xslt ha priorità rispetto agli altri
            myXmlFile = myXmlFileLessSpecific = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Xslt\" + xsltname + ".xslt";
            myXmlFileMoreSpecific = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Xslt\" + contentType + "." + xsltname + ".xslt";
            if (File.Exists(myXmlFileMoreSpecific)) {
                myXmlFile = myXmlFileMoreSpecific;
            }




            //var namespaces = typeof(FieldExternalService).FullName.Split('.').AsEnumerable();
            //namespaces = namespaces.Except(new string[] { typeof(FieldExternalService).Name });
            //namespaces = namespaces.Except(new string[] { namespaces.Last() });
            //var area = string.Join(".", namespaces);
            //string TenantSite = (_shellSetting.Name == "Default") ? "" : @"\" + _shellSetting.Name;

            //string myXmlFile = HostingEnvironment.MapPath("~/") + @"Modules\" + area + @"\xslt" + TenantSite + @"\" + xsltname + ".xslt";
            if (File.Exists(myXmlFile)) {
                // xmlpage = @"<xml/>";

                XmlReader myXPathDoc = XmlReader.Create(new StringReader(xmlpage));

                myXPathDoc.Read();

                // XPathDocument myXPathDoc = new XPathDocument(new StringReader(xmlpage));
                XsltArgumentList argsList = new XsltArgumentList();

                argsList.AddExtensionObject("my:HttpUtility", new ExtensionObject());


                string cult = _workContext.GetContext().CurrentCulture;
                if (String.IsNullOrEmpty(cult))
                    cult = "it";
                else
                    cult = cult.Substring(0, 2);


                argsList.AddParam("LinguaParameter", "", cult);

                var allrequest = _workContext.GetContext().HttpContext.Request.QueryString.Keys;

                for (var i = 0; i < allrequest.Count; i++) {
                    string _key = allrequest[i];
                    string _value = _workContext.GetContext().HttpContext.Request.QueryString[_key].ToString();
                    argsList.AddParam(_key.ToLower().Trim(), "", _value);
                }


                XsltSettings settings = new XsltSettings();
                settings.EnableScript = true;

                XslCompiledTransform myXslTrans;
                var enableXsltDebug = false;
#if DEBUG
                enableXsltDebug = true;
#endif
                myXslTrans = new XslCompiledTransform(enableXsltDebug);
                myXslTrans.Load(myXmlFile, settings, new XmlUrlResolver());

                StringWriter sw = new StringWriter();
                XmlWriter xmlWriter = new XmlTextWriter(sw);
                myXslTrans.Transform(myXPathDoc, argsList, xmlWriter);

                output = sw.ToString();
            } else {
                output = xmlpage;
                Logger.Error("file not exist ->" + myXmlFile);
            }
            string xml = RemoveAllNamespaces(output);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNode newNode = doc.DocumentElement;
            string JsonData = JsonConvert.SerializeXmlNode(newNode);
            JsonData = JsonData.Replace("\":lasernumeric", "");
            JsonData = JsonData.Replace("lasernumeric:\"", "");
            JsonData = JsonData.Replace("\":laserboolean", "");
            JsonData = JsonData.Replace("laserboolean:\"", "");
            JsonData = JsonData.Replace(@"\r\n", "");
            JsonData = JsonData.Replace("\":laserDate", "\"\\/Date(");
            JsonData = JsonData.Replace("laserDate:\"", ")\\/\"");
            dynamic dynamiccontent = Json.Decode(JsonData, typeof(object));
            return dynamiccontent;
        }


        private static string GetHttpPage(string uri, HttpVerbOptions httpMethod, HttpDataTypeOptions httpDataType, string bodyRequest) {

            //Uri uri = new Uri("https://mysite.com/auth");
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri) as HttpWebRequest;
            //request.Accept = "application/xml";

            //// authentication
            //var cache = new CredentialCache();
            //cache.Add(uri, "Basic", new NetworkCredential("user", "secret"));
            //request.Credentials = cache;

            //ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);

            //// response.
            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = null;
            String strResult;
            WebResponse objResponse;
            WebRequest objRequest = (HttpWebRequest)WebRequest.Create(uri);
            objRequest.Headers.Add(HttpRequestHeader.ContentEncoding, "gzip");
            //    objRequest.Method = WebRequestMethods.Http.Get;
            //  objRequest.Accept = "application/json";
            // HttpWebRequest
            //  objRequest.UseDefaultCredentials = true;
            //objRequest.Accept
            objRequest.Method = httpMethod.ToString();

            // valore di default del content type
            objRequest.ContentType = "application/x-www-form-urlencoded";

            if (httpMethod == HttpVerbOptions.POST)
            {
                if (httpDataType == HttpDataTypeOptions.JSON)
                {
                    // JSON
                    objRequest.ContentType = "application/json; charset=utf-8";
                }

                // body del post
                byte[] buffer = System.Text.UTF8Encoding.UTF8.GetBytes(bodyRequest);
                dataStream = objRequest.GetRequestStream();
                dataStream.Write(buffer, 0, buffer.Length);
                dataStream.Close();
            }

            objRequest.PreAuthenticate = false;
            objResponse = objRequest.GetResponse();
            using (StreamReader sr = new StreamReader(objResponse.GetResponseStream())) {
                strResult = sr.ReadToEnd();
                sr.Close();
            }
            //var eliminoacapo = strResult.Split(new string[] { ">\\r\\n" }, StringSplitOptions.None);
            //strResult = string.Join(">", eliminoacapo);
            return strResult;
        }

        private static string RemoveAllNamespaces(string xmlDocument) {
            XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));
            return xmlDocumentWithoutNs.ToString();
        }

        //Core recursion function
        private static XElement RemoveAllNamespaces(XElement xmlDocument) {
            if (!xmlDocument.HasElements) {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }

    }


}