using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Tokens;
using RazorEngine;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Configuration;
using RazorEngine.Templating;
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
using System.Dynamic;
using System.Data.Entity.Design.PluralizationServices;

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
                            if (arr.ToString() != "[]") {
                                if (arr.GetType().Name == "JValue")
                                    myjarray.Add(arr);
                                else
                                    myjarray.Add(jsonflusher((JObject)arr));
                            }

                        }
                        newJsonObject.Add(property.Name, myjarray);
                        // newJsonObject.Add(property.Name, jsonflusher((JObject)property.Value));
                    }
                    else if (property.Value.GetType().Name == "JObject") {
                        newJsonObject.Add(property.Name.Replace(" ", ""), jsonflusher((JObject)property.Value));
                    }
                }
            }
            return newJsonObject;

        }
        public dynamic GetContentfromField(Dictionary<string, object> contesto, string externalUrl, string nomexlst, string contentType = "", HttpVerbOptions httpMethod = HttpVerbOptions.GET, HttpDataTypeOptions httpDataType = HttpDataTypeOptions.JSON, string bodyRequest = "") {
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
                    XmlDocument newdoc = new XmlDocument();
                    newdoc = JsonConvert.DeserializeXmlNode(webpagecontent, "root");
                    correggiXML(newdoc);
                    webpagecontent = newdoc.InnerXml;
                }


                ci = RazorTransform(webpagecontent.Replace(" xmlns=\"\"", ""), nomexlst, contentType);

            }
            catch (Exception ex) {
                Logger.Error(ex, UrlToGet);
            }
            return (ci);
        }

        //private static dynamic XmlToDynamic(XmlReader file, XElement node = null) {
        ////    if (String.IsNullOrWhiteSpace(file) && node == null) return null;
        //     if (file==null && node == null) return null;
        //    // If a file is not empty then load the xml and overwrite node with the
        //    // root element of the loaded document
        //   // node = !String.IsNullOrWhiteSpace(file) ? XDocument.Load(file).Root : node;
        //    node =!(file==null)? XDocument.Load(file).Root : node;
        //    IDictionary<String, dynamic> result = new ExpandoObject();

        //    // implement fix as suggested by [ndinges]
        //    var pluralizationService =
        //        PluralizationService.CreateService(CultureInfo.CreateSpecificCulture("en-us"));

        //    // use parallel as we dont really care of the order of our properties
        //    node.Elements().AsParallel().ForAll(gn => {
        //        // Determine if node is a collection container
        //        var isCollection = gn.HasElements &&
        //            (
        //            // if multiple child elements and all the node names are the same
        //                gn.Elements().Count() > 1 &&
        //                gn.Elements().All(
        //                    e => e.Name.LocalName.ToLower() == gn.Elements().First().Name.LocalName) ||

        //                // if there's only one child element then determine using the PluralizationService if
        //            // the pluralization of the child elements name matches the parent node. 
        //                gn.Name.LocalName.ToLower() == pluralizationService.Pluralize(
        //                    gn.Elements().First().Name.LocalName).ToLower()
        //            );

        //        // If the current node is a container node then we want to skip adding
        //        // the container node itself, but instead we load the children elements
        //        // of the current node. If the current node has child elements then load
        //        // those child elements recursively
        //        var items = isCollection ? gn.Elements().ToList() : new List<XElement>() { gn };

        //        var values = new List<dynamic>();

        //        // use parallel as we dont really care of the order of our properties
        //        // and it will help processing larger XMLs
        //        items.AsParallel().ForAll(i => values.Add((i.HasElements) ?
        //           XmlToDynamic(null, i) : i.Value.Trim()));

        //        // Add the object name + value or value collection to the dictionary
        //        result[gn.Name.LocalName] = isCollection ? values : values.FirstOrDefault();
        //    });
        //    return result;
        //}
        public List<XmlNode> listanodi = new List<XmlNode>();
        private void correggiXML(XmlDocument xml) {
          
            foreach (XmlNode nodo in xml.ChildNodes) {
                doiteratenode(nodo, xml);
            }
            foreach (XmlNode sottonodo in listanodi) {
                int n;
                bool isNumeric = int.TryParse(sottonodo.Name, out n);
                if (isNumeric ) {
                    RenameNode(xml, sottonodo, "lasernumeric");
                }
                if (sottonodo.Name.ToLower() == "description" || sottonodo.Name.ToLower() == "abstract" || sottonodo.Name.ToLower() == "extension"){
                    RenameNode(xml, sottonodo, sottonodo.Name+"text");
                }
            }
  
        }
        private void doiteratenode(XmlNode nodo, XmlDocument xml) {
            foreach (XmlNode sottonodo in nodo.ChildNodes) {
                int n;
                bool isNumeric = int.TryParse(sottonodo.Name, out n);
                if (isNumeric || sottonodo.Name.ToLower() == "description" || sottonodo.Name.ToLower() == "abstract" || sottonodo.Name.ToLower() == "extension") {
                    listanodi.Add(sottonodo);
                   // RenameNode(xml, sottonodo, "lasernumeric");
                }
                doiteratenode(sottonodo, xml);
            }
        }
        void RenameNode(XmlDocument doc,XmlNode e, string newName) {
            XmlNode newNode = doc.CreateNode(e.NodeType, newName, null);
            while (e.HasChildNodes) {
                newNode.AppendChild(e.FirstChild);
            }
            XmlAttributeCollection ac = e.Attributes;
            while (ac.Count > 0) {
                newNode.Attributes.Append(ac[0]);
            }
            XmlNode parent = e.ParentNode;
            parent.ReplaceChild(newNode, e);
        }

        private dynamic RazorTransform(string xmlpage, string xsltname, string contentType = "") {
            string output = "";
            string myfile = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Xslt\" + contentType + xsltname + ".cshtml";
            if (!System.IO.File.Exists(myfile)) {
                myfile = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\Xslt\" + xsltname + ".cshtml";
            }
            if (System.IO.File.Exists(myfile)) {
                   string mytemplate = File.ReadAllText(myfile);
                   string myfile2 = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\common.cshtml";
                    if (System.IO.File.Exists(myfile)) {
                        mytemplate= File.ReadAllText(myfile2)+mytemplate;;
                    }
                if (!string.IsNullOrEmpty(mytemplate)) {
                    var config = new TemplateServiceConfiguration();
                    string result = "";
                    //XmlTextReader reader = new XmlTextReader(new StringReader(xmlpage));
                    //XmlDocument docum = new XmlDocument();
                    //docum.LoadXml(xmlpage);
                    //XmlReader reader = XmlReader.Create(new StringReader(xmlpage));
                    //System.Text.StringBuilder sbXML = new System.Text.StringBuilder();
                    //while (reader.Read()) {
                    //    @sbXML.AppendLine(reader.ReadOuterXml());
                    //}
                    // object docum = XmlToDynamic(myXPathDoc);
                    //XmlDocument xmlDoc = new XmlDocument();
                    //xmlDoc.LoadXml(xmlpage);
                    //correggiXML(xmlDoc);
                    //xmlpage=xmlDoc.ToString();
                    //object docum = DynamicXml.Parse(xmlpage);

                    //DocParser dp = new DocParser(DocParser.DocTypes.Xml, xmlpage); //can also retrieve xml from external service, etc
                    //var robots = dp.GetElements("*");
                    var docwww = XDocument.Parse(xmlpage);

               
              //   temp er = new temp();
              //  XmlDocument mydoc = er.ToXmlDocument(docwww);
                //foreach (XmlNode bookToModify in mydoc.SelectNodes("/root/_data/lasernumeric/film")) {
                //    if (!bookToModify.HasChildNodes) {
                //        bookToModify.ParentNode.ParentNode.RemoveChild(bookToModify.ParentNode);
                //    }
                //}
                //    mydoc = er.aggiungipadre(mydoc, "root", "_dataList", "_data");
                //    mydoc = er.aggiungipadre(mydoc, "root/_dataList/_data/media", "ImageList", "image");
                //    mydoc = er.rendinumeric(mydoc, "root/_dataList/_data/id");
                //    mydoc = er.cambianome(mydoc, "root/_dataList/_data/id", "OriginalId");
                ////    mydoc = er.RimuoviNodo(mydoc, "root/_dataList");
                //    mydoc = er.aggiungifiglio(mydoc, "root/_dataList/_data/media/ImageList", "image");
                //    mydoc = er.RimuoviAlberaturaTranne(mydoc, "root/_dataList");

             //     er.SpanXDocument(er.ToXDocument(mydoc).Root);
                //   er.SpanXDocument(docwww.Root);
                    using (var service = RazorEngineService.Create(config)) {
                        
                        result = service.RunCompile(mytemplate, "htmlRawTemplatea", null, docwww);
                    }
                    output = result.Replace("\r\n", "");
                    //if (!string.IsNullOrEmpty(resultnobr)) {

                    // }
                }
                else
                    output = "";
                while (output.StartsWith("\t")) {
                    output=output.Substring(1);
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
            else {
                return XsltTransform(xmlpage, xsltname, contentType);
            }
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
            }
            else {
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

            if (httpMethod == HttpVerbOptions.POST) {
                if (httpDataType == HttpDataTypeOptions.JSON) {
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

    public class DynamicXml : DynamicObject {
        XElement _root;
        private DynamicXml(XElement root) {
            _root = root;
        }

        public static DynamicXml Parse(string xmlString) {
            return new DynamicXml(XDocument.Parse(xmlString).Root);
        }

        public static DynamicXml Load(string filename) {
            return new DynamicXml(XDocument.Load(filename).Root);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            result = null;

            var att = _root.Attribute(binder.Name);
            if (att != null) {
                result = att.Value;
                return true;
            }

            var nodes = _root.Elements(binder.Name);
            if (nodes.Count() > 1) {
                result = nodes.Select(n => new DynamicXml(n)).ToList();
                return true;
            }

            var node = _root.Element(binder.Name);
            if (node != null) {
                if (node.HasElements) {
                    result = new DynamicXml(node);
                }
                else {
                    result = node.Value;
                }
                return true;
            }

            return true;
        }







     
    }
}