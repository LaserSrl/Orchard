using Contrib.Widgets.Services;
using Laser.Orchard.Events.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.Projections.Services;
using Orchard.Security;
using Orchard.Taxonomies.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
namespace Laser.Orchard.WebServices.Controllers {
    [WebApiKeyFilter(true)]
    public class WebApiController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IProjectionManager _projectionManager;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IContentSerializationServices _contentSerializationServices;


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
        private readonly ICommonsServices _commonServices;

        private readonly HttpRequest _request;

        private List<string> processedItems;
        //private int _maxLevel = 10;  // default

        //
        // GET: /Json/
        public WebApiController(IOrchardServices orchardServices,
            IProjectionManager projectionManager,
            ITaxonomyService taxonomyService,
            ShellSettings shellSetting,
            IUtilsServices utilsServices,
            ICsrfTokenHelper csrfTokenHelper,
            IAuthenticationService authenticationService,
            ICommonsServices commonServices,
            IContentSerializationServices contentSerializationServices) {
            _request = System.Web.HttpContext.Current.Request;
            _commonServices = commonServices;
            _orchardServices = orchardServices;
            _projectionManager = projectionManager;
            _taxonomyService = taxonomyService;
            _shellSetting = shellSetting;
            Logger = NullLogger.Instance;
            _utilsServices = utilsServices;
            _csrfTokenHelper = csrfTokenHelper;
            _authenticationService = authenticationService;
            _contentSerializationServices = contentSerializationServices;
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
            var content = _commonServices.GetContentByAlias(alias);
            var json = _contentSerializationServices.Terms(content, maxLevel);
            return Content(json.ToString(Newtonsoft.Json.Formatting.None), "application/json");
        }

        public ActionResult Display(string alias, int page = 1, int pageSize = 10, int maxLevel = 10) {
            JObject json;

            IContent content;
            if (alias.ToLower() == "user+info" || alias.ToLower() == "user info") {
                #region [ Richiesta dati di uno user ]
                var currentUser = _authenticationService.GetAuthenticatedUser();
                if (currentUser == null) {
                    //  return Content((Json(_utilsServices.GetResponse(ResponseType.InvalidUser))).ToString(), "application/json");// { Message = "Error: No current User", Success = false,ErrorCode=ErrorCode.InvalidUser,ResolutionAction=ResolutionAction.Login });
                    var result = new ContentResult { ContentType = "application/json" };
                    result.Content = Newtonsoft.Json.JsonConvert.SerializeObject(_utilsServices.GetResponse(ResponseType.InvalidUser));
                    return result;
                } else
                    if (!_csrfTokenHelper.DoesCsrfTokenMatchAuthToken()) {
                        var result = new ContentResult { ContentType = "application/json" };
                        result.Content = Newtonsoft.Json.JsonConvert.SerializeObject(_utilsServices.GetResponse(ResponseType.InvalidXSRF));
                        return result;
                        //   Content((Json(_utilsServices.GetResponse(ResponseType.InvalidXSRF))).ToString(), "application/json");// { Message = "Error: No current User", Success = false,ErrorCode=ErrorCode.InvalidUser,ResolutionAction=ResolutionAction.Login });
                    } else {
                        #region utente validato
                        content = currentUser.ContentItem;
                        #endregion
                    }
                #endregion

            } else {
                content = _commonServices.GetContentByAlias(alias);
            }
            //_maxLevel = maxLevel;
            json = _contentSerializationServices.GetJson(content, page, pageSize);
            //_contentSerializationServices.NormalizeSingleProperty(json);
            return Content(json.ToString(Newtonsoft.Json.Formatting.None), "application/json");
            //return GetJson(content, page, pageSize);
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