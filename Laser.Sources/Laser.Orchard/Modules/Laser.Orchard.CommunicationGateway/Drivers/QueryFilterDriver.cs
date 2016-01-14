//using Laser.Orchard.CommunicationGateway.Models;
//using Laser.Orchard.CommunicationGateway.ViewModels;
//using Laser.Orchard.Queries.Services;
//using Orchard;
//using Orchard.ContentManagement;
//using Orchard.ContentManagement.Drivers;
//using Orchard.Localization;
//using Orchard.Logging;
//using System;
//using System.Collections.Generic;
//using System.Web.Mvc;

//namespace Laser.Orchard.CommunicationGateway.Drivers {

//    public class QueryFilterDriver : ContentPartDriver<QueryFilterPart> {
//        private readonly IOrchardServices _orchardServices;
//        private readonly ICustomQuery _customQuery;
//        public ILogger Logger { get; set; }
//        public Localizer T { get; set; }

//        protected override string Prefix {
//            get { return "Laser.Orchard.CommunicationGateway"; }
//        }

//        public QueryFilterDriver(IOrchardServices orchardServices, ICustomQuery customQuery) {
//            _orchardServices = orchardServices;
//            _customQuery = customQuery;
//            Logger = NullLogger.Instance;
//            T = NullLocalizer.Instance;
//        }

//        //protected override DriverResult Display(QueryFilterPart part, string displayType, dynamic shapeHelper) {
//        //    return ContentShape("Parts_QueryFilter",
//        //            () => shapeHelper.Parts_QueryFilter(part.QueryTitle, part.QueryId));
//        //}

//        protected override DriverResult Editor(QueryFilterPart part, dynamic shapeHelper) {
//            Dictionary<string, int> elenco = _customQuery.Get("Communication");
//            List<SelectListItem> lSelectList = new List<SelectListItem>();
//            foreach (KeyValuePair<string, int> entry in elenco) {
//                lSelectList.Insert(0, new SelectListItem() { Value = entry.Value.ToString(), Text = entry.Key });
//            }
//            lSelectList.Insert(0, new SelectListItem() { Value = "0", Text = T("None").ToString() });
//            QueryFilterVM qfVM = new QueryFilterVM();
//            // qfVM.QueryTitle = part.QueryTitle;
//            qfVM.QueryId = part.QueryId.ToString();
//            qfVM.ElencoQuery = new SelectList((IEnumerable<SelectListItem>)lSelectList, "Value", "Text", qfVM.QueryId);
//            return ContentShape("Parts_QueryFilter",
//                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/QueryFilter",
//                                    Model: qfVM,
//                                    Prefix: Prefix));
//        }

//        protected override DriverResult Editor(QueryFilterPart part, IUpdateModel updater, dynamic shapeHelper) {
//            //var partSettings = part.Settings.GetModel<MapPartSettings>();
//            //var mapEdit = new MapEditModel {
//            //    Map = part
//            //};
//            QueryFilterVM qfVM = new QueryFilterVM();
//            updater.TryUpdateModel(qfVM, Prefix, null, new string[] { "ElencoQuery" });
//            part.QueryId = Int32.Parse(qfVM.QueryId);
//            //    updater.AddModelError("MapPartIsRequired", T("A point on the map is required."));
//            return Editor(part, shapeHelper);
//        }
//    }
//}