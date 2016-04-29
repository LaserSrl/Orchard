using Contrib.Widgets.Services;
using Laser.Orchard.Commons.Services;
using Laser.Orchard.Events.Services;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.WebServices.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Localization.Models;
using Orchard.Logging;
using Orchard.Projections.Services;
using Orchard.Security;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
//using Newtonsoft.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Orchard.OutputCache.Filters;
using Laser.Orchard.StartupConfig.Exceptions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections;
using Orchard.DisplayManagement.Shapes;
using Newtonsoft.Json.Converters;
using Orchard.Fields.Fields;
using Orchard.Taxonomies.Fields;
using System.Dynamic;
namespace Laser.Orchard.WebServices.Controllers {
    public class WebApiController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IProjectionManager _projectionManager;
        private readonly ITaxonomyService _taxonomyService;



        private readonly ShellSettings _shellSetting;
        private readonly IUtilsServices _utilsServices;
        private IWidgetManager _widgetManager;
        private IEventsService _eventsService;
        private readonly ICsrfTokenHelper _csrfTokenHelper;
        private readonly IAuthenticationService _authenticationService;

        private readonly string[] _skipPartNames;
        private readonly string[] _skipPartTypes;
        private readonly string[] _skipPartProperties;
        private readonly string[] _skipFieldTypes;
        private readonly string[] _skipFieldProperties;
        private readonly string[] _skipAlwaysProperties;
        private readonly Type[] _basicTypes;

        private readonly HttpRequest _request;

        private List<string> processedItems;
        private int _maxLevel = 10;  // default

        //
        // GET: /Json/
        public WebApiController(IOrchardServices orchardServices,
            IProjectionManager projectionManager,
            ITaxonomyService taxonomyService,
            ShellSettings shellSetting,
            IUtilsServices utilsServices,
            ICsrfTokenHelper csrfTokenHelper,
            IAuthenticationService authenticationService) {
            _request = System.Web.HttpContext.Current.Request;

            _orchardServices = orchardServices;
            _projectionManager = projectionManager;
            _taxonomyService = taxonomyService;
            _shellSetting = shellSetting;
            Logger = NullLogger.Instance;
            _utilsServices = utilsServices;
            _csrfTokenHelper = csrfTokenHelper;
            _authenticationService = authenticationService;
            _skipPartNames = new string[]{
                "InfosetPart","FieldIndexPart","IdentityPart","UserPart","UserRolesPart", "AdminMenuPart", "MenuPart"};
            _skipPartTypes = new string[]{
                "ContentItem","Zones","TypeDefinition","TypePartDefinition","PartDefinition", "Settings", "Fields", "Record"};
            _skipAlwaysProperties = new string[]{
                "ContentItemRecord","ContentItemVersionRecord"};
            _skipPartProperties = new string[] { };
            _skipFieldTypes = new string[]{
                "FieldDefinition","PartFieldDefinition"};
            _skipFieldProperties = new string[]{
                "Storage", "Name", "DisplayName", "Setting"};
            _basicTypes = new Type[] {
                typeof(string),
                typeof(decimal),
                typeof(float),
                typeof(int),
                typeof(bool),
                typeof(DateTime),
                typeof(Enum)
            };
            processedItems = new List<string>();
        }

        public ILogger Logger { get; set; }

        public ActionResult Terms(string alias, int maxLevel = 10) {
            JObject json;
            _maxLevel = maxLevel;
            var content = GetContentByAlias(alias);
            dynamic contentToSerialize = null, termPart = null;
            try {
                if (content.ContentItem.ContentType.EndsWith("Taxonomy")) {
                    contentToSerialize = content;
                    json = new JObject(SerializeObject(content, 0));
                    NormalizeSingleProperty(json);
                    return Content(json.ToString(Newtonsoft.Json.Formatting.None), "application/json");
                } else if (content.ContentItem.ContentType.EndsWith("Term") || !String.IsNullOrWhiteSpace(content.ContentItem.TypeDefinition.Settings["Taxonomy"])) {
                    termPart = ((dynamic)content.ContentItem).TermPart;
                    if (termPart != null) {
                        json = new JObject(SerializeObject(content, 0));
                        contentToSerialize = _taxonomyService.GetChildren(termPart, false);
                        var resultArray = new JArray();
                        foreach (var resulted in contentToSerialize) {
                            resultArray.Add(new JObject(SerializeObject(resulted, 0)));
                        }
                        json.Add("SubTerms", resultArray);
                        NormalizeSingleProperty(json);
                        return Content(json.ToString(Newtonsoft.Json.Formatting.None), "application/json");
                    }
                }
            } catch {
            }
            return null;
        }

        public ActionResult Display(string alias, int page = 1, int pageSize = 10, int maxLevel = 10) {
            var content = GetContentByAlias(alias);
            _maxLevel = maxLevel;
            return GetJson(content, page, pageSize);
        }

        private ActionResult GetJson(IContent content, int page = 1, int pageSize = 10) {
            JObject json;
            var policy = content.As<Policy.Models.PolicyPart>();
            if (policy != null && (policy.HasPendingPolicies ?? false)) { // Se l'oggetto ha delle pending policies allora devo serivre la lista delle pending policies
                json = new JObject();
                var resultArray = new JArray();
                foreach (var pendingPolicy in policy.PendingPolicies) {
                    resultArray.Add(new JObject(SerializeContentItem((ContentItem)pendingPolicy, 0)));
                }
                json.Add("PendingPolicies", resultArray);
            } else {// Se l'oggetto non ha delle pending policies allora devo serivre il content stesso
                dynamic shape = _orchardServices.ContentManager.BuildDisplay(content); // Forse non serve nemmeno
                var filteredContent = shape.ContentItem;

                json = new JObject(SerializeObject(filteredContent, 0));
                dynamic part;

                #region [Projections]
                // Projection
                try {
                    part = ((dynamic)filteredContent).ProjectionPart;
                } catch {
                    part = null;
                }
                if (part != null) {
                    var queryId = part.Record.QueryPartRecord.Id;
                    var queryItems = _projectionManager.GetContentItems(queryId, (page - 1) * pageSize, pageSize);
                    var resultArray = new JArray();
                    foreach (var resulted in queryItems) {
                        resultArray.Add(new JObject(SerializeContentItem((ContentItem)resulted, 0)));
                    }
                    if ((json.Root.HasValues) && (json.Root.First.HasValues) && (json.Root.First.First.HasValues)) {
                        JProperty array = new JProperty("ProjectionPageResults", resultArray);
                        json.Root.First.First.Last.AddAfterSelf(array);
                    } else {
                        json.Add("ProjectionPageResults", resultArray);
                    }
                }
                #endregion
            }

            NormalizeSingleProperty(json);

            return Content(json.ToString(Newtonsoft.Json.Formatting.None), "application/json");
        }

        /// <summary>
        /// Accorpa gli oggetti che hanno una sola proprietà con la proprietà padre.
        /// Es. Creator: { Value: 2 } diventa Creator: 2.
        /// </summary>
        /// <param name="json"></param>
        private void NormalizeSingleProperty(JObject json) {
            List<JToken> nodeList = new List<JToken>();

            // scandisce tutto l'albero dei nodi e salva i nodi potenzialmente "interessanti" in una lista
            nodeList.Add(json.Root);
            for (int i = 0; i < nodeList.Count; i++) {
                foreach (var tempNode in nodeList[i].Children()) {
                    if (tempNode.HasValues) {
                        nodeList.Add(tempNode);
                    }
                }
            }

            // scorre tutti i nodi per cercare quelli da accorpare
            foreach (var tempNode in nodeList) {
                if (tempNode.Count() == 1) {
                    if (tempNode.First.HasValues == false) {
                        if ((tempNode.Parent != null) && (tempNode.Parent.Count == 1)) {
                            if ((tempNode.Parent.Parent != null) 
                                && (tempNode.Parent.Parent.Count == 1) 
                                && (tempNode.Parent.Parent.Type == JTokenType.Property)) {
                                    (tempNode.Parent.Parent as JProperty).Value = tempNode.First;
                            }
                        }
                    }
                }
            }
        }

        private IContent GetContentByAlias(string displayAlias) {
            IContent item = null;
            var autoroutePart = _orchardServices.ContentManager.Query<AutoroutePart, AutoroutePartRecord>()
                .ForVersion(VersionOptions.Published)
                .Where(w => w.DisplayAlias == displayAlias).List().SingleOrDefault();

            if (autoroutePart != null && autoroutePart.ContentItem != null) {
                item = autoroutePart.ContentItem;
            } else {
                new HttpException(404, ("Not found"));
                return null;
            }
            return item;

        }


        protected JProperty SerializeContentItem(ContentItem item, int actualLevel) {
            if ((actualLevel + 1) > _maxLevel) {
                return new JProperty("content item", "omissis...");
            }
            JProperty jsonItem;
            var jsonProps = new JObject(
                new JProperty("Id", item.Id),
                new JProperty("Version", item.Version));

            var partsObject = new JObject();
            var parts = item.Parts
                .Where(cp => !cp.PartDefinition.Name.Contains("`") && !_skipPartNames.Contains(cp.PartDefinition.Name)
                );
            foreach (var part in parts) {
                jsonProps.Add(SerializePart(part, actualLevel + 1));
            }

            jsonItem = new JProperty(item.ContentType,
                jsonProps
                );

            return jsonItem;
        }

        protected JProperty SerializePart(ContentPart part, int actualLevel) {
            // ciclo sulle properties delle parti
            var properties = part.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop =>
                !_skipPartTypes.Contains(prop.Name) //skip 
                );
            var partObject = new JObject();
            foreach (var property in properties) {
                try {
                    if (!_skipPartProperties.Contains(property.Name)) {
                        object val = property.GetValue(part, BindingFlags.GetProperty, null, null, null);
                        if (val != null) {
                            PopulateJObject(ref partObject, property, val, _skipPartProperties, actualLevel);
                        }
                    }
                } catch {

                }
            }

            //// now add the fields to the json object....
            foreach (var contentField in part.Fields) {
                var fieldObject = SerializeField(contentField, actualLevel);
                partObject.Add(fieldObject);
            }


            try {
                if (part.GetType() == typeof(ContentPart) && !part.PartDefinition.Name.EndsWith("Part")) {
                    return new JProperty(part.PartDefinition.Name + "DPart", partObject);
                } else {
                    return new JProperty(part.PartDefinition.Name, partObject);
                }
            } catch {
                return new JProperty(Guid.NewGuid().ToString(), partObject);
            }

        }

        protected JProperty SerializeField(ContentField field, int actualLevel) {
            var fieldObject = new JObject();
            if (field.FieldDefinition.Name == "EnumerationField") {
                var enumField = (EnumerationField)field;
                string[] selected = enumField.SelectedValues;
                string[] options = enumField.PartFieldDefinition.Settings["EnumerationFieldSettings.Options"].Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                fieldObject.Add("Options", JToken.FromObject(options));
                fieldObject.Add("SelectedValues", JToken.FromObject(selected));
            } else if (field.FieldDefinition.Name == "TaxonomyField") {
                var taxoField = (TaxonomyField)field;
                fieldObject.Add("Terms", JToken.FromObject(taxoField.Terms.Select(x => x.Id).ToList()));
                var taxo = taxoField.PartFieldDefinition.Settings["TaxonomyFieldSettings.Taxonomy"];
                var taxoPart = _taxonomyService.GetTaxonomyByName(taxo);
                JArray arr = new JArray();
                fieldObject.Add("Taxonomy", arr);
                foreach (var term in taxoPart.Terms) {
                    CustomTermPart customTermPart = new CustomTermPart {
                        Id = term.Id,
                        Name = term.Name,
                        Path = term.Path,
                        Selectable = term.Selectable,
                        Slug = term.Slug
                    };
                    JToken jObj = JToken.FromObject(customTermPart);
                    arr.Add(jObj);
                    var contentPartList = term.ContentItem.Parts.Where(x => (x.TypeDefinition.Name == "ContentType") && (x.PartDefinition.Name == term.ContentItem.TypeDefinition.Name)); // part aggiunta da Orchard per contenere i fields diretti
                    foreach (var contentPart in contentPartList) {
                        foreach (var innerField in contentPart.Fields) {
                            jObj.Last.AddAfterSelf(SerializeField(innerField, actualLevel));
                        }
                    }
                }
            } else {
                var properties = field.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop =>
                    !_skipFieldTypes.Contains(prop.Name) //skip 
                    );

                foreach (var property in properties) {
                    try {
                        if (!_skipFieldProperties.Contains(property.Name)) {
                            object val = property.GetValue(field, BindingFlags.GetProperty, null, null, null);
                            if (val != null) {
                                PopulateJObject(ref fieldObject, property, val, _skipFieldProperties, actualLevel);
                            }
                        }
                    } catch {

                    }
                }
            }

            return new JProperty(field.Name, fieldObject);
        }

        private JProperty SerializeObject(object item, int actualLevel, string[] skipProperties = null) {
            if ((actualLevel + 1) > _maxLevel) {
                return new JProperty("object", "omissis...");
            }
            try {
                if (((dynamic)item).Id != null) {
                    if (processedItems.Contains(String.Format("{0}({1})", item.GetType().Name, ((dynamic)item).Id)))
                        return null;
                }
            } catch {
            }
            skipProperties = skipProperties ?? new string[0];
            try {
                if (item is ContentPart) {
                    return SerializePart((ContentPart)item, actualLevel);
                } else if (item is ContentField) {
                    return SerializeField((ContentField)item, actualLevel);
                } else if (item is ContentItem) {
                    return SerializeContentItem((ContentItem)item, actualLevel + 1);
                } else if (item is Array || item.GetType().IsGenericType) { // Lista
                    //DA AFFINARE QUESTA PARTE ATTUALMENTE NON FUNZIONA!

                    JArray array = new JArray();
                    foreach (var itemArray in (IList)item) {

                        if (!IsBasicType(itemArray.GetType())) {
                            array.Add(new JObject { SerializeObject(itemArray, actualLevel + 1, skipProperties) });
                        } else {
                            var valItem = itemArray;
                            FormatValue(ref valItem);
                            array.Add(valItem);
                        }
                    }
                    PopulateProcessedItems(item.GetType().Name, ((dynamic)item).Id);
                    return new JProperty(item.GetType().Name, array);

                } else if (item.GetType().IsClass) {

                    //DA AFFINARE QUESTA PARTE ATTUALMENTE NON FUNZIONA!
                    //IL RISULTATO DOVREBBE ESSERE CHE TUTTI GLI ENUMERATORS POSSANO ESSERE CONVERTITI IN STRINGHE (PER UNA MIGLIORE LEGGIBILITA')
                    //var members = item.GetType()
                    //.GetFields(BindingFlags.Instance | BindingFlags.Public).Cast<MemberInfo>()
                    //.Union(item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    //.Where(m => !skipProperties.Contains(m.Name) && !_skipAlwaysProperties.Contains(m.Name))
                    //;
                    //List<JProperty> properties = new List<JProperty>();
                    //foreach (var member in members) {
                    //    var propertyInfo = item.GetType().GetProperty(member.Name);
                    //    if (!IsBasicType(propertyInfo.PropertyType)) {
                    //        properties.Add(SerializeObject(propertyInfo.GetValue(item), skipProperties));
                    //    } else {
                    //        properties.Add(new JProperty(member.Name, item.GetType().GetProperty(member.Name).GetValue(item)));
                    //    }
                    //}
                    //PopulateProcessedItems(item.GetType().Name, ((dynamic)item).Id);
                    //return new JProperty(item.GetType().Name, properties);
                    // END DA AFFINARE QUESTA PARTE ATTUALMENTE NON FUNZIONA!

                    JObject propertiesObject;
                    var serializer = JsonSerializerInstance();
                    propertiesObject = JObject.FromObject(item, serializer);
                    foreach (var skip in skipProperties) {
                        propertiesObject.Remove(skip);
                    }
                    PopulateProcessedItems(item.GetType().Name, ((dynamic)item).Id);
                    return new JProperty(item.GetType().Name, propertiesObject);
                } else {
                    PopulateProcessedItems(item.GetType().Name, ((dynamic)item).Id);
                    return new JProperty(item.GetType().Name, item);
                }
            } catch (Exception ex) {
                return new JProperty(item.GetType().Name, ex.Message);
            }

        }

        private void PopulateJObject(ref JObject jObject, PropertyInfo property, object val, string[] skipProperties, int actualLevel) {

            JObject propertiesObject;
            var serializer = JsonSerializerInstance();
            if (val is Array || val.GetType().IsGenericType) {
                JArray array = new JArray();
                foreach (var itemArray in (IEnumerable)val) {

                    if (!IsBasicType(itemArray.GetType())) {
                        array.Add(new JObject { SerializeObject(itemArray, actualLevel, skipProperties) });
                    } else {
                        var valItem = itemArray;
                        FormatValue(ref valItem);
                        array.Add(valItem);
                    }
                }
                jObject.Add(new JProperty(property.Name, array));

            } else {
                // jObject.Add(SerializeObject(val, skipProperties));
            }
            if (!IsBasicType(val.GetType())) {
                try {
                    propertiesObject = JObject.FromObject(val, serializer);
                    foreach (var skip in skipProperties) {
                        propertiesObject.Remove(skip);
                    }
                    jObject.Add(property.Name, propertiesObject);
                } catch {
                    jObject.Add(new JProperty(property.Name, val.GetType().FullName));
                }
            } else {
                FormatValue(ref val);
                jObject.Add(new JProperty(property.Name, val));
            }
        }

        private bool IsBasicType(Type type) {
            return _basicTypes.Contains(type) || type.IsEnum;
        }

        private void FormatValue(ref object val) {
            if (val.GetType().IsEnum) {
                val = val.ToString();
            }
        }

        private void PopulateProcessedItems(string key, dynamic id) {
            if (id != null)
                processedItems.Add(String.Format("{0}({1})", key, id.ToString()));
        }

        private JsonSerializer JsonSerializerInstance() {
            return new JsonSerializer {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatString = "#MM-dd-yyyy hh.mm.ss#",
            };
        }
    }

    public class EnumStringConverter : Newtonsoft.Json.Converters.StringEnumConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (value.GetType().IsEnum) {
                writer.WriteValue(value.ToString());// or something else
                return;
            }
            base.WriteJson(writer, value, serializer);
        }
    }

    class CustomTermPart {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Slug { get; set; }
        public bool Selectable { get; set; }
    }
}