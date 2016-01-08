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
            // da mettere poi in un service
            var resultRecordNumber = 0;
            //TODO: Migliorare questa logica, da testare in situazioni complesse e per le performance.
            IEnumerable<ContentItem> contents = _queryPickerService.GetContentItemsAndCombined(model.SelectedIds);
            //foreach (int queryId in model.SelectedIds) {
            //    var queries = _queryPickerService.GetContentQueries(_queryRepository.Get(queryId));
            //    resultRecordNumber = Math.Min(queries.Sum(x => x.Where(w=>w.).Count()), resultRecordNumber);
            //}
            model.TotalItemsCount = resultRecordNumber;

            return ContentShape("Parts_QueryPicker_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: "Parts/QueryPicker_Edit",
                        Model: model,
                        Prefix: Prefix));
        }
    }
}