using Laser.Orchard.IXMSD.Models;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.StartupConfig.ViewModels;
using Laser.Orchard.StartupConfig.WebApiProtection.Filters;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Services;
using Orchard.Core.Contents;
using Orchard.Core.Contents.Settings;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.Projections.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Laser.Orchard.IXMSD.Controllers {
    [WebApiKeyFilter(false)]
    public class VideoController : ApiController {
        private readonly IOrchardServices _orchardServices;
        private readonly IUtilsServices _utilsServices;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentDefinitionService _contentDefinitionService;

        public ILogger _Logger { get; set; }

        public VideoController(IOrchardServices orchardServices, IUtilsServices utilsServices, IContentDefinitionManager contentDefinitionManager, IContentDefinitionService contentDefinitionService) {
            _orchardServices = orchardServices;
            _utilsServices = utilsServices;
            _contentDefinitionManager = contentDefinitionManager;
            _Logger = NullLogger.Instance;
            _contentDefinitionService = contentDefinitionService;
        }

        public Response Get(string nomefile, string NewUrl) {
#if DEBUG
            _Logger.Error("Richiesta modifica del video" + nomefile + "con url" + NewUrl);
#endif
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.Headers.GetValues("x-frame-options").ToString())) {
#if DEBUG
                _Logger.Error("x-frame-options:" + HttpContext.Current.Request.Headers.GetValues("x-frame-options").ToString());
#endif
                if (HttpContext.Current.Request.Headers.GetValues("x-frame-options")[0].ToString() == "SAMEORIGIN") {
#if DEBUG
                    _Logger.Error("x-frame-options[0]:" + HttpContext.Current.Request.Headers.GetValues("x-frame-options")[0].ToString());
#endif
                    var allmediapart = _orchardServices.ContentManager.Query<MediaPart, MediaPartRecord>().Where(x => x.FileName == nomefile).List().ToList();
                    foreach (MediaPart mp in allmediapart) {
                        mp.ContentItem.As<IXMSDPart>().ExternalMediaUrl = NewUrl;
                        AssegnaStato(mp.ContentItem.Id);
                    }
                    return (_utilsServices.GetResponse(ResponseType.Success));
                }
                else
                    return (_utilsServices.GetResponse(ResponseType.UnAuthorized));
            }
            else
                return (_utilsServices.GetResponse(ResponseType.UnAuthorized));
        }

        private void AssegnaStato(Int32 mediaid) {
            var listFields = new List<string>();
            var query = _orchardServices.ContentManager.Query(VersionOptions.Published, GetCreatableTypes(false).Select(ctd => ctd.Name).ToArray());
            var allCt = GetCreatableTypes(false);
            foreach (var ct in allCt) {
                var allMediaFld = _contentDefinitionService.GetType(ct.Name).Fields.Where(w =>
                    w._Definition.FieldDefinition.Name == "MediaLibraryPickerField");
                var allFieldNames = allMediaFld.Select(s => ct.Name + "." + s.Name + ".");
                listFields.AddRange(allFieldNames);
            }
            query = query.Join<FieldIndexPartRecord>().Where(w => w.StringFieldIndexRecords.Any(
                w2 => listFields.Contains(w2.PropertyName) && w2.Value.Contains("{" + mediaid.ToString() + "}")
               ));
            query = query.Join<FieldIndexPartRecord>().Where(w => w.StringFieldIndexRecords.Any(
                 w2 => w2.PropertyName == "PublishExtensionPart.PublishExtensionStatus." && w2.Value == "Created"
                ));
            var listitem = query.List();
            foreach (var t in listitem) {
                ((dynamic)t.As<PublishExtensionPart>().Fields.FirstOrDefault(x => x.Name == "PublishExtensionStatus")).Value = "Loaded";
            }
        }

        private IEnumerable<ContentTypeDefinition> GetCreatableTypes(bool andContainable) {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd =>
                _orchardServices.Authorizer.Authorize(Permissions.EditContent, _orchardServices.ContentManager.New(ctd.Name)) &&
                ctd.Settings.GetModel<ContentTypeSettings>().Creatable &&
                (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }
    }
}