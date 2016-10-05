using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Laser.Orchard.Queries.Models;
using Laser.Orchard.Queries.Services;
using Laser.Orchard.Queries.ViewModels;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.Queries.Drivers {
    public class QueryPickerPartDriver : ContentPartDriver<QueryPickerPart> {
        private readonly IQueryPickerService _queryPickerService;
        private readonly IRepository<QueryPartRecord> _queryRepository;
        private readonly IProjectionManager _projectionManager;
        private readonly IContentManager _contentManager;

        public QueryPickerPartDriver(IQueryPickerService queryPickerService, IRepository<QueryPartRecord> queryRepository, IProjectionManager projectionManager, IContentManager contentManager) {
            _queryPickerService = queryPickerService;
            _queryRepository = queryRepository;
            _projectionManager = projectionManager;
            _contentManager = contentManager;
        }

        protected override string Prefix {
            get {
                return "QueryPickerPart";
            }
        }
        protected override DriverResult Editor(QueryPickerPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(QueryPickerPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = new QueryPickerVM {
                SelectedIds = part.Ids,
                AvailableQueries = new SelectList(_queryPickerService.GetUserDefinedQueries().Select(x =>
                    new {
                        Value = x.Id,
                        Text = ((dynamic)x).TitlePart.Title
                    }
                    ), "Value", "Text", part.Ids)
            };
            if (updater != null && updater.TryUpdateModel(model, Prefix, null, null)) {
                part.Ids = model.SelectedIds;
            }
            var resultRecordNumber = 0;
            // TODO: rendere dinamico e injettabile l'array dei contenttypes
            var combinedQueries = _queryPickerService.GetCombinedContentQuery(model.SelectedIds, null, null);
            resultRecordNumber = combinedQueries.Count();
            model.TotalItemsCount = resultRecordNumber;

            return ContentShape("Parts_QueryPicker_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/QueryPicker_Edit",
                        Model: model,
                        Prefix: Prefix));
        }


        protected override void Importing(QueryPickerPart part, ImportContentContext context) {
            var importedIds = context.Attribute(part.PartDefinition.Name, "Ids");
          
            if (importedIds != null) {
                for (int x = 0; x <= importedIds.Count(); x++) {
                    part.Ids[x] = importedIds[x];
                }
            }
        }

        protected override void Exporting(QueryPickerPart part, ExportContentContext context) {
           
            if (part.Ids.Count() > 0) {
                context.Element(part.PartDefinition.Name).SetAttributeValue("Ids", part.Ids);
                var IdsList = context.Element(part.PartDefinition.Name).Element("Ids");
                for (int x = 0; x == part.Ids.Count(); x++) {
                    IdsList.Element("Ids").SetAttributeValue("Ids", part.Ids[x]);
                }
            }
        }




    }
}