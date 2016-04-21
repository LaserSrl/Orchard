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

        private readonly Type[] _basicTypes;

        private readonly HttpRequest _request;

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
            _skipPartProperties = new string[]{
                "ContentItemRecord","ContentItemVersionRecord"};
            _skipFieldTypes = new string[]{
                "FieldDefinition","PartFieldDefinition"};
            _skipFieldProperties = new string[]{
                "Storage", "Name", "DisplayName"};
            _basicTypes = new Type[] {
                typeof(string),
                typeof(decimal),
                typeof(float),
                typeof(int),
                typeof(bool),
                typeof(DateTime),
                typeof(Enum)
            };

        }

        public ILogger Logger { get; set; }

        public ActionResult Display(string alias, int page = 1, int pageSize = 10) {
            var content = GetContentByAlias(alias);
            return GetJson(content,page, pageSize);
        }

        private ActionResult GetJson(IContent content, int page = 1, int pageSize = 10) {
            Shape shape = _orchardServices.ContentManager.BuildDisplay(content); // Forse non serve nemmeno

            var json = new JObject(SerializeContentItem((ContentItem)content));
            dynamic part;

            #region [Projections]
            // Projection
            try {
                part = ((dynamic)content).ProjectionPart;
            } catch {
                part = null;
            }
            if (part != null) {
                var queryId = part.Record.QueryPartRecord.Id;
                var queryItems = _projectionManager.GetContentItems(queryId, (page - 1) * pageSize, pageSize);
                var resultArray = new JArray();
                foreach (var resulted in queryItems) {
                    resultArray.Add(new JObject(SerializeContentItem((ContentItem)resulted)));
                }
                json.Add("ContentItems", resultArray);

            }
            #endregion
            return Content(json.ToString(Newtonsoft.Json.Formatting.None), "application/json");
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


        protected JProperty SerializeContentItem(ContentItem item) {
            JProperty jsonItem;
            var jsonProps = new JObject(
                new JProperty("Id", item.Id),
                new JProperty("Version", item.Version));

            var partsObject = new JObject();
            var parts = item.Parts
                .Where(cp => !cp.PartDefinition.Name.Contains("`") && !_skipPartNames.Contains(cp.PartDefinition.Name)
                );
            foreach (var part in parts) {
                jsonProps.Add(SerializePart(part));
            }

            jsonItem = new JProperty(item.ContentType,
                jsonProps
                );

            return jsonItem;
        }

        protected JProperty SerializePart(ContentPart part) {
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
                            PopulateJObject(ref partObject, property, val, _skipPartProperties);
                        }
                    }
                } catch {

                }
            }

            //// now add the fields to the json object....
            foreach (var contentField in part.Fields) {
                var fieldObject = SerializeField(contentField);
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

        protected JProperty SerializeField(ContentField field) {
            var fieldObject = new JObject();
            var properties = field.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop =>
                !_skipFieldTypes.Contains(prop.Name) //skip 
                );

            foreach (var property in properties) {
                try {
                    if (!_skipFieldProperties.Contains(property.Name)) {
                        object val = property.GetValue(field, BindingFlags.GetProperty, null, null, null);
                        if (val != null) {
                            PopulateJObject(ref fieldObject, property, val, _skipFieldProperties);
                        }
                    }
                } catch {

                }
            }


            return new JProperty(field.Name, fieldObject);
        }

        private JProperty SerializeObject(object item, string[] skipProperties = null) {
            skipProperties = skipProperties ?? new string[0];
            if (item is ContentPart) {
                return SerializePart((ContentPart)item);
            } else if (item is ContentField) {
                return SerializeField((ContentField)item);
            } else if (item is ContentItem) {
                return SerializeContentItem((ContentItem)item);
            } else if (item.GetType().IsClass) {
                JObject propertiesObject;
                var serializer = new JsonSerializer {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                propertiesObject = JObject.FromObject(item, serializer);
                foreach (var skip in skipProperties) {
                    propertiesObject.Remove(skip);
                }
                return new JProperty(item.GetType().Name, propertiesObject);
            }
                // else if (!_basicTypes.Contains(item.GetType())) {
                //    JObject propertiesObject;
                //    var serializer = new JsonSerializer {
                //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                //    };

            //    try {
                //        propertiesObject = JObject.FromObject(item, serializer);
                //        foreach (var skip in skipProperties) {
                //            propertiesObject.Remove(skip);
                //        }
                //        return new JProperty(item.GetType().Name, propertiesObject);
                //    } catch {
                //        return new JProperty(item.GetType().Name, item.GetType().FullName);
                //    }
                //} 
                else {
                return new JProperty(item.GetType().Name, item);
            }

        }

        private void PopulateJObject(ref JObject jObject, PropertyInfo property, object val, string[] skipProperties) {

            JObject propertiesObject;
            var serializer = new JsonSerializer {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            if (val is Array || val.GetType().IsGenericType) {
                JArray array = new JArray();
                foreach (var itemArray in (IList)val) {
                    if (!_basicTypes.Contains(itemArray.GetType())) {
                        array.Add(new JObject { SerializeObject(itemArray) });
                    } else {
                        array.Add(itemArray);
                    }
                }
                jObject.Add(new JProperty(property.Name, array));

            } else {
                // jObject.Add(SerializeObject(val, skipProperties));
            }
            if (!_basicTypes.Contains(val.GetType())) {
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
                jObject.Add(new JProperty(property.Name, val));
            }
        }
    }
}