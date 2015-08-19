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
using Orchard.Environment.Configuration;
using System.Web.Hosting;
using Orchard.Logging;
using Orchard.Localization.Models;
using Laser.Orchard.Events.Services;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Core.Contents;


namespace Laser.Orchard.WebServices.Controllers {
    public class JsonController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IProjectionManager _projectionManager;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ShellSettings _shellSetting;
        private IWidgetManager _widgetManager;
        private IEventsService _eventsService;
        public ILogger Logger { get; set; }


        //
        // GET: /Json/
        public JsonController(IOrchardServices orchardServices,
            IProjectionManager projectionManager,
            ITaxonomyService taxonomyService,
            ShellSettings shellSetting
            ) {
            _orchardServices = orchardServices;
            _projectionManager = projectionManager;
            _taxonomyService = taxonomyService;
            _shellSetting = shellSetting;
            Logger = NullLogger.Instance;
        }


        public ActionResult GetObjectByAlias(string displayAlias, SourceTypes sourceType = SourceTypes.ContentItem, ResultTarget resultTarget = ResultTarget.Contents, string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true) {
            var autoroutePart = _orchardServices.ContentManager.Query<AutoroutePart, AutoroutePartRecord>()
                .ForVersion(VersionOptions.Published)
                .Where(w => w.DisplayAlias == displayAlias).List().SingleOrDefault();
            IContent item = null;
            if (autoroutePart != null) {
                item = autoroutePart.ContentItem;
            }
            int id = ((ContentItem)autoroutePart.ContentItem).Id;
            string CiType = ((ContentItem)autoroutePart.ContentItem).ContentType;
            int masterid = 0;
            int teoric_masterid = 0;
            try {
                teoric_masterid = ((ContentItem)autoroutePart.ContentItem).As<LocalizationPart>().MasterContentItem.Id;
                masterid = teoric_masterid;
            }
            catch {
                masterid = id;
            }
            var contentsLocalized = _orchardServices.ContentManager.Query(CiType).Where<LocalizationPartRecord>(l => l.MasterContentItemId == masterid || l.Id == masterid).List();
            List<dynamic> ListShape = new List<dynamic>();
            foreach (ContentItem singleCi in contentsLocalized) {
                ListShape.Add(_orchardServices.ContentManager.BuildDisplay(singleCi));
            }
            //          _orchardServices.ContentManager.Query(((ContentItem)autoroutePart.ContentItem).ContentType).Where<LocalizationPartRecord>(l => l.MasterContentItemId == content.ContentItem.Id && l.CultureId == cultureRecord.Id)
            //        var elencoItemLocalized = _orchardServices.ContentManager.Query(autoroutePart.ContentItem.ContentType).Where<LocalizationPartRecord>(l => l.MasterContentItemId == masterid || l.Id == masterid);//content.ContentItem.Id && l.CultureId == cultureRecord.Id)
            //foreach ((ContentItem)ci in  elencoItemLocalized.){


            //}


            //  dynamic shape = _orchardServices.ContentManager.BuildDisplay(item);
            var namespaces = this.GetType().FullName.Split('.').AsEnumerable();
            namespaces = namespaces.Except(new string[] { this.GetType().Name });
            namespaces = namespaces.Except(new string[] { namespaces.Last() });
            var area = string.Join(".", namespaces);
            string myview = "~/" + @"App_Data/Sites/" + _shellSetting.Name + "/WebServices/" + item.ContentItem.ContentType + ".cshtml";
            string myfile = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSetting.Name + @"\WebServices\" + item.ContentItem.ContentType + ".cshtml";
            if (System.IO.File.Exists(myfile))
                return View(myview, (object)ListShape);

            else
                return null;
        }

        // 
        // GET: /Json/GetByAlias
        // Attributes:
        // displayAlias: url di ingresso Es: displayAlias=produttore-hermes
        // filterSubItemsParts: elennco csv delle parti da estrarre in presenza di array di ContentItems Es: filterSubItemsParts=TitlePart,AutoroutePart,MapPart
        public ContentResult GetByAlias(string displayAlias, SourceTypes sourceType = SourceTypes.ContentItem, ResultTarget resultTarget = ResultTarget.Contents, string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true) {
            //   Logger.Error("inizio"+DateTime.Now.ToString());
            var autoroutePart = _orchardServices.ContentManager.Query<AutoroutePart, AutoroutePartRecord>()
                .ForVersion(VersionOptions.Published)
                .Where(w => w.DisplayAlias == displayAlias).List().SingleOrDefault();
            IContent item = null;
            if (autoroutePart != null) {
                item = autoroutePart.ContentItem;
            }
            ContentResult cr = GetContent(item, sourceType, resultTarget, mfilter, page, pageSize, tinyResponse);
            //    Logger.Error("Fine:"+DateTime.Now.ToString());
            return cr;
        }
        // 
        // GET: /Json/GetById
        // Attributes:
        // contentId: id del content Es: contentId=1
        // filterSubItemsParts: elennco csv delle parti da estrarre in presenza di array di ContentItems Es: filterSubItemsParts=TitlePart,AutoroutePart,MapPart
        public ContentResult GetById(int contentId, SourceTypes sourceType = SourceTypes.ContentItem, ResultTarget resultTarget = ResultTarget.Contents, string mfilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true) {
            IContent item = _orchardServices.ContentManager.Get(contentId, VersionOptions.Published);
            return GetContent(item, sourceType, resultTarget, mfilter, page, pageSize, tinyResponse);
        }

        private ContentResult GetContent(IContent content, SourceTypes sourceType = SourceTypes.ContentItem, ResultTarget resultTarget = ResultTarget.Contents, string fieldspartsFilter = "", int page = 1, int pageSize = 10, bool tinyResponse = true) {
            var jsonString = "{}";
            if (!_orchardServices.Authorizer.Authorize(Permissions.ViewContent, content)) { 
                //TODO: Not Authorized answer
            }
            var _filterContentFieldsParts = fieldspartsFilter.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            XElement dump;
            XElement projectionDump = null;
            // il dump dell'oggetto principale non filtra per field
            ObjectDumper dumper = new ObjectDumper(10, null, false, tinyResponse);

            dynamic shape, specificShape;
             shape = _orchardServices.ContentManager.BuildDisplay(content);
            List<XElement> listContent = new List<XElement>();
            if (sourceType == SourceTypes.ContentItem) {
                dump = dumper.Dump(content, "Model");
            }
            else {
                dump = dumper.Dump(shape, "Model");
            }
            //dump.XPathSelectElements("");
            //var filteredDump = dump.Descendants();
            var sb = new StringBuilder();
            //ConvertToJSon(dump, sb);
            ConvertToJSon(dump, sb);
            sb.Insert(0, "{");
            sb.Append(", \"l\":[");
            // Dopo avere convertito il contentItem in JSON aggiungo i Json delle eventuali liste 
            dynamic part = null;
            var firstList = true;
            var listDumpedContentIds = new List<int>();

            #region [ProjectionPart ]
            try {
                part = shape.ContentItem.ProjectionPart;
            }
            catch {
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
                    dumper = new ObjectDumper(10, _filterContentFieldsParts, false, tinyResponse);
                    projectionDump = dumper.Dump(item, String.Format("[{0}]", i));
                    ConvertToJSon(projectionDump, sb);
                    sb.Append("}");
                    i++;
                }
                sb.Append("]");
                sb.Append("}");

            }
            part = null;
            #endregion
            #region [CalendarPart ]
            try {
                part = shape.ContentItem.CalendarPart;
            } catch {
                part = null;
            }
            if (part != null) {
                if (!firstList) {
                    sb.Append(",");
                }
                firstList = false;
                if (_orchardServices.WorkContext.TryResolve<IEventsService>(out _eventsService))
                { // non sempre questo modulo è attivo quindi se non riesce a risolvere il servizio, bypassa la chiamata
                    var queryItems = _eventsService.GetAggregatedList(part, page, pageSize);
                    int i = 0;
                    sb.Append("{");
                    sb.AppendFormat("\"n\": \"{0}\"", "EventList");
                    sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                    sb.Append(", \"m\": [");

                    foreach (var item in queryItems) {
                        if (i > 0) {
                            sb.Append(",");
                        }
                        sb.Append("{");
                        dumper = new ObjectDumper(10, _filterContentFieldsParts, false, tinyResponse);
                        projectionDump = dumper.Dump(item, String.Format("[{0}]", i));
                        ConvertToJSon(projectionDump, sb);
                        sb.Append("}");
                        i++;
                    }
                    sb.Append("]");
                    sb.Append("}");
                }
            }
            part = null;
            #endregion

            #region [ExernalField]
            var ExtertalFields = (dynamic)
                 (from parte in ((ContentItem)shape.ContentItem).Parts
                  from field in parte.Fields
                  where (field.GetType().Name == "FieldExternal" && ((dynamic)field).Setting.GenerateL)
                  select field).FirstOrDefault();
            if (ExtertalFields != null) {
                if (!firstList) {
                    sb.Append(",");
                }
                firstList = false;
                //sb.Append("{");
                //sb.AppendFormat("\"n\": \"{0}\"", "ExternalContent");
                //sb.AppendFormat(", \"v\": \"{0}\"", "ExternalContent");
                //sb.Append(", \"m\": [");

                sb.Append("{");
                dumper = new ObjectDumper(10, _filterContentFieldsParts, false, tinyResponse);
                //nameDynamicJsonArray = "List<generic>";
                projectionDump = dumper.Dump(cleanobj(ExtertalFields.ContentObject), ExtertalFields.Name, "List<generic>");
                ConvertToJSon(projectionDump, sb);
                //    sb.Append("}]}");
                sb.Append("}");
            }
            #endregion
            #region [ WidgetsContainerPart ]
            try {
                part = shape.ContentItem.WidgetsContainerPart;
            }
            catch {
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
                        dumper = new ObjectDumper(10, _filterContentFieldsParts, false, tinyResponse);
                        projectionDump = dumper.Dump(item, String.Format("[{0}]", i));
                        ConvertToJSon(projectionDump, sb);
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
                if (shape.ContentItem.ContentType.EndsWith("Term") || !String.IsNullOrWhiteSpace(shape.ContentItem.TypeDefinition.Settings["Taxonomy"])) {
                    part = shape.ContentItem.TermPart;
                }
            }
            catch {
                part = null;
            }
            if (part != null) {
                if (!firstList) {
                    sb.Append(",");
                }
                firstList = false;
                dynamic termContentItems;
                if (resultTarget == ResultTarget.Terms) {
                    termContentItems = _taxonomyService.GetChildren(part, true);
                }
                else if (resultTarget == ResultTarget.SubTerms) {
                    termContentItems = _taxonomyService.GetChildren(part, false);
                }
                else {
                    termContentItems = _taxonomyService.GetContentItems(part, (page - 1) * pageSize, pageSize);
                }

                int i = 0;
                sb.Append("{");
                if (resultTarget == ResultTarget.Contents) {

                    sb.AppendFormat("\"n\": \"{0}\"", "TaxonomyTermList");
                    sb.AppendFormat(", \"v\": \"{0}\"", "ContentItem[]");
                }
                else {
                    sb.AppendFormat("\"n\": \"{0}\"", "TermPartList");
                    sb.AppendFormat(", \"v\": \"{0}\"", "TermPart[]");
                }
                sb.Append(", \"m\": [");

                foreach (var item in termContentItems) {
                    if (i > 0) {
                        sb.Append(",");
                    }
                    sb.Append("{");
                    dumper = new ObjectDumper(10, _filterContentFieldsParts, false, tinyResponse);
                    if (resultTarget == ResultTarget.Contents) {
                        projectionDump = dumper.Dump(item.ContentItem, String.Format("[{0}]", i));
                        ConvertToJSon(projectionDump, sb);
                    }
                    else {
                        var dumperForPart = new ObjectDumper(10, _filterContentFieldsParts, true, tinyResponse);
                        projectionDump = dumperForPart.Dump(item, "TermPart");
                        ConvertToJSon(projectionDump, sb);
                    }
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

            jsonString = sb.ToString().Replace("\t"," ");
            return new ContentResult { Content = jsonString, ContentType = "application/json" };

        }
        private dynamic cleanobj(dynamic objec) {
            if (objec != null)
                if (objec.ToRemove != null) {
                    return cleanobj(objec.ToRemove);
                }
            return objec;
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
