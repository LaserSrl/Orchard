using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Laser.Orchard.WebServices.Services;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using System.Xml.XPath;
using Orchard.Projections.Services;
using Contrib.Widgets.Services;
using Orchard.Taxonomies.Services;

namespace Laser.Orchard.WebServices.Controllers {
    public class JsonController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IProjectionManager _projectionManager;
        private readonly ITaxonomyService _taxonomyService;

        private IWidgetManager _widgetManager;

        //
        // GET: /Json/
        public JsonController(IOrchardServices orchardServices,
            IProjectionManager projectionManager,
            ITaxonomyService taxonomyService) {
            _orchardServices = orchardServices;
            _projectionManager = projectionManager;
            _taxonomyService = taxonomyService;
        }


        // 
        // GET: /Json/GetByAlias
        // Attributes:
        // displayAlias: url di ingresso Es: displayAlias=produttore-hermes
        // filterSubItemsParts: elennco csv delle parti da estrarre in presenza di array di ContentItems Es: filterSubItemsParts=TitlePart,AutoroutePart,MapPart
        public ContentResult GetByAlias(string displayAlias, SourceTypes sourceType = SourceTypes.ContentItem, string filterSubItemsParts = "", int page = 1, int pageSize = 10) {
            var autoroutePart = _orchardServices.ContentManager.Query<AutoroutePart, AutoroutePartRecord>()
                .ForVersion(VersionOptions.Published)
                .Where(w => w.DisplayAlias == displayAlias).List().SingleOrDefault();
            IContent item = null;
            if (autoroutePart != null) {
                item = autoroutePart.ContentItem;
            }
            return GetContent(item, sourceType, filterSubItemsParts, page, pageSize);
        }
        // 
        // GET: /Json/GetById
        // Attributes:
        // contentId: id del content Es: contentId=1
        // filterSubItemsParts: elennco csv delle parti da estrarre in presenza di array di ContentItems Es: filterSubItemsParts=TitlePart,AutoroutePart,MapPart
        public ContentResult GetById(int contentId, SourceTypes sourceType = SourceTypes.ContentItem, string filterSubItemsParts = "", int page = 1, int pageSize = 10) {
            IContent item = _orchardServices.ContentManager.Get(contentId, VersionOptions.Published);
            return GetContent(item, sourceType, filterSubItemsParts, page, pageSize);
        }

        private ContentResult GetContent(IContent content, SourceTypes sourceType = SourceTypes.ContentItem, string filterSubItemsParts = "", int page = 1, int pageSize = 10) {
            var jsonString = "{}";

            var _filterSubItemsParts = filterSubItemsParts.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            XElement dump;
            XElement projectionDump = null;
            ObjectDumper dumper = new ObjectDumper(10, null, _filterSubItemsParts);

            dynamic shape, specificShape;
            List<XElement> listContent = new List<XElement>();
            shape = _orchardServices.ContentManager.BuildDisplay(content);
            if (sourceType == SourceTypes.ContentItem) {
                dump = dumper.Dump(content, "Model");
            } else {
                dump = dumper.Dump(shape, "Model");
            }
            //dump.XPathSelectElements("");
            //var filteredDump = dump.Descendants();
            var sb = new StringBuilder();
            //ConvertToJSon(dump, sb);
            ConvertToJSon(dump, sb);
            List<ContentFlags> renderedContentList = new List<ContentFlags>(); // Resetto la lista altrimenti per il nodo "l" alcuni nodi padre risultano già renderizzati e non prosegue nella ricerca dei membri figli
            sb.Insert(0, "{");
            sb.Append(", \"l\":[");
            // Dopo avere convertito il contentItem in JSON aggiungo i Json delle eventuali liste 
            dynamic part = null;
            var firstList = true;
            var listDumpedContentIds = new List<int>();

            #region [ProjectionPart ]
            try {
                part = shape.ContentItem.ProjectionPart;
            } catch {
                part = null;
            }
            if (part != null) {
                if (!firstList) {
                    sb.Append(",");
                }
                firstList = false;
                var queryId = part.Record.QueryPartRecord.Id;
                var queryItems = _projectionManager.GetContentItems(queryId, (page - 1) * pageSize, pageSize);
                int i = 0;
                sb.Append("{");
                sb.AppendFormat("\"n\": \"{0}\"", "ProjectionList");
                sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                sb.Append(", \"m\": [");

                foreach (var item in queryItems) {
                    if (i > 0) {
                        sb.Append(",");
                    }
                    sb.Append("{");
                    dumper = new ObjectDumper(10, renderedContentList, _filterSubItemsParts);
                    projectionDump = dumper.Dump(item, String.Format("[{0}]", i));
                    ConvertToJSon(projectionDump, sb);
                    renderedContentList = dumper.RenderedContentList; // otteng la lista dei contenuti già renderizzati per evitare renderizzazioni dello stesso oggetto più volte
                    sb.Append("}");
                    i++;
                }
                sb.Append("]");
                sb.Append("}");

            }
            part = null;
            #endregion

            #region [ WidgetsContainerPart ]
            try {
                part = shape.ContentItem.WidgetsContainerPart;
            } catch {
                part = null;
            }
            if (part != null) {
                //var queryId = part.Record.QueryPartRecord.Id;
                if (_orchardServices.WorkContext.TryResolve<IWidgetManager>(out _widgetManager)) { // non semepre questo modulo è attivo quindi se non riesce a risolvere il servizio, bypassa la chiamata
                    if (!firstList) {
                        sb.Append(",");
                    }
                    firstList = false;
                    var queryItems = _widgetManager.GetWidgets(part.Id);
                    int i = 0;
                    sb.Append("{");
                    sb.AppendFormat("\"n\": \"{0}\"", "WidgetList");
                    sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                    sb.Append(", \"m\": [");

                    foreach (var item in queryItems) {
                        if (i > 0) {
                            sb.Append(",");
                        }
                        sb.Append("{");
                        dumper = new ObjectDumper(10, renderedContentList, _filterSubItemsParts);
                        projectionDump = dumper.Dump(item, String.Format("[{0}]", i));
                        ConvertToJSon(projectionDump, sb);
                        renderedContentList = dumper.RenderedContentList; // otteng la lista dei contenuti già renderizzati per evitare renderizzazioni dello stesso oggetto più volte
                        sb.Append("}");
                        i++;
                    }
                    sb.Append("]");
                    sb.Append("}");
                }

            }
            #endregion

            #region [ Taxonomy/TermsPart ]
            part = null;
            try {
                if (shape.ContentItem.ContentType.EndsWith("Term")) {
                    part = shape.ContentItem.TermPart;
                }
            } catch {
                part = null;
            }
            if (part != null) {
                if (!firstList) {
                    sb.Append(",");
                }
                firstList = false;
                var termContentItems = _taxonomyService.GetContentItems(part, (page - 1) * pageSize, pageSize);

                int i = 0;
                sb.Append("{");
                sb.AppendFormat("\"n\": \"{0}\"", "TaxonomyTermList");
                sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                sb.Append(", \"m\": [");

                foreach (var item in termContentItems) {
                    if (i > 0) {
                        sb.Append(",");
                    }
                    sb.Append("{");
                    dumper = new ObjectDumper(10, renderedContentList, _filterSubItemsParts);
                    projectionDump = dumper.Dump(item.ContentItem, String.Format("[{0}]", i));
                    ConvertToJSon(projectionDump, sb);
                    renderedContentList = dumper.RenderedContentList; // otteng la lista dei contenuti già renderizzati per evitare renderizzazioni dello stesso oggetto più volte
                    sb.Append("}");
                    i++;
                }
                sb.Append("]");
                sb.Append("}");

            }
            part = null;
            #endregion


            sb.Append("]"); // l : [
            sb.Append("}");

            jsonString = sb.ToString();
            return new ContentResult { Content = jsonString, ContentType = "application/json" };

        }

        public static void ConvertToJSon(XElement x, StringBuilder sb) {
            if (x == null) {
                return;
            }

            switch (x.Name.ToString()) {
                case "ul":
                    var first = true;
                    foreach (var li in x.Elements()) {
                        if (!first) sb.Append(",");
                        ConvertToJSon(li, sb);
                        first = false;
                    }
                    break;
                case "li":
                    var name = x.Element("h1").Value;
                    var value = x.Element("span").Value;

                    sb.AppendFormat("\"n\": \"{0}\", ", FormatJsonValue(name));
                    sb.AppendFormat("\"v\": \"{0}\"", FormatJsonValue(value));

                    var ul = x.Element("ul");
                    if (ul != null && ul.Descendants().Any()) {
                        sb.Append(", \"m\": [");
                        first = true;
                        foreach (var li in ul.Elements()) {
                            sb.Append(first ? "{ " : ", {");
                            ConvertToJSon(li, sb);
                            sb.Append(" }");
                            first = false;
                        }
                        sb.Append("]");
                    }

                    break;
            }
        }

        public static void ConvertToMinifiedJSon(XElement x, StringBuilder sb) {
            if (x == null) {
                return;
            }

            switch (x.Name.ToString()) {
                case "ul":
                    var first = true;
                    foreach (var li in x.Elements()) {
                        if (!first) sb.Append(",");
                        ConvertToMinifiedJSon(li, sb);
                        first = false;
                    }
                    break;
                case "li":
                    var name = x.Element("h1").Value;
                    var value = x.Element("span").Value;
                    var ul = x.Element("ul");

                    sb.AppendFormat("\"{0}\": \"{1}\"" +
                        ((ul != null && ul.Descendants().Any()) ? ", " : ""), FormatJsonValue(name), FormatJsonValue(value));

                    if (ul != null && ul.Descendants().Any()) {
                        sb.Append(" \"members\": [");
                        first = true;
                        foreach (var li in ul.Elements()) {
                            sb.Append(first ? "{ " : ", {");
                            ConvertToMinifiedJSon(li, sb);
                            sb.Append(" }");
                            first = false;
                        }
                        sb.Append("]");
                    }

                    break;
            }
        }

        public static string FormatJsonValue(string value) {
            if (String.IsNullOrEmpty(value)) {
                return String.Empty;
            }

            // replace " by \" in json strings
            return value.Replace(@"\", @"\\").Replace("\"", @"\""").Replace("\r\n", @"\n").Replace("\r", @"\n").Replace("\n", @"\n");
            return HttpUtility.HtmlEncode(value).Replace(@"\", @"\\").Replace("\"", @"\""").Replace("\r\n", @"\n").Replace("\r", @"\n").Replace("\n", @"\n");
        }

    }

    //public static class Extensions {
    //    public static IEnumerable<XElement> Children(this XElement root) {
    //        var nodes = new Stack<XElement>(new[] { root });
    //        while (nodes.Any()) {
    //            XElement node = nodes.Pop();
    //            yield return node;
    //            foreach (var n in node.Elements()) nodes.Push(n);
    //        }
    //    }

    //}
}
