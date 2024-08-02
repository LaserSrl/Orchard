using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Features;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Upgrade.Controllers {
    [Admin]
    public class EnumerationFieldsController : Controller {
        private readonly IFeatureManager _featureManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INotifier _notifier;
        private readonly IContentManager _contentManager;
        IEnumerable<IContentHandler> _contentHandlers;

        public EnumerationFieldsController(IFeatureManager featureManager,
            IContentDefinitionManager contentDefinitionManager,
            INotifier notifier,
            IContentManager contentManager,
            IEnumerable<IContentHandler> contentHandlers) {

            T = NullLocalizer.Instance;
            _featureManager = featureManager;
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            _contentManager = contentManager;
            _contentHandlers = contentHandlers;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            ViewBag.CanMigrate = false;

            if (_featureManager.GetEnabledFeatures().All(x => x.Id != "Orchard.Fields")) {
                _notifier.Warning(T("You need to enable Orchard.Fields in order to migrate Enumeration Field values."));
            } else {
                ViewBag.CanMigrate = true;
            }

            return View();
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPOST() {
            // Content types
            var contentTypes = _contentDefinitionManager.ListTypeDefinitions()
                // Where a part
                .Where(ct => ct.Parts
                    // Contains a Enumeration Field
                    .Any(pd => pd.PartDefinition.Fields
                        .Any(pfd => pfd.FieldDefinition.Name.Equals("EnumerationField", StringComparison.OrdinalIgnoreCase))))
                .Select(ct => ct.Name)
                .ToArray();

            var query = _contentManager.Query()
                .ForType(contentTypes);

            var contentItems = query.List();

            foreach(var ci in contentItems) {
                UpdateItem(ci);
            }

            return RedirectToAction("Index");
        }

        private ContentItem UpdateItem(ContentItem ci) {
            var updateContext = new UpdateContentContext(ci);
            _contentHandlers.Invoke(handler => handler.Updating(updateContext), Logger);

            // Don't do anything here, handlers only need to be called to "upgrade" field values.

            _contentHandlers.Invoke(handler => handler.Updated(updateContext), Logger);

            if (ci.IsPublished()) {
                ci.VersionRecord.Published = false;
                _contentManager.Publish(ci);
            }
            return ci;
        }
    }
}