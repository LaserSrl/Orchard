using Laser.Orchard.CommunicationGateway.Helpers;
using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.CommunicationGateway.ViewModels;
using NHibernate;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Fields.Fields;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Laser.Orchard.CommunicationGateway.Controllers {

    public class ContactsAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly string contentType = "CommunicationContact";
        private readonly dynamic TestPermission = Permissions.ManageContact;
        private readonly ICommunicationService _communicationService;
        private readonly IExportContactService _exportContactService;
        private readonly ShellSettings _shellSettings;
        private readonly ISessionLocator _session;
        private readonly INotifier _notifier;
        private Localizer T { get; set; }

        public ContactsAdminController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager,
             ICommunicationService communicationService,
             ISessionLocator session,
             IExportContactService exportContactService,
            ShellSettings shellSettings
            ) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _communicationService = communicationService;
            _exportContactService = exportContactService;
            _shellSettings = shellSettings;
            _session = session;
        }

        [Admin]
        public ActionResult Synchronize() {
            if (_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner)) {
                _communicationService.Synchronize();
            }
            return RedirectToAction("Index", "ContactsAdmin");
        }

        [Admin]
        public ActionResult Edit(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            object model;
            if (id == 0) {
                var newContent = _contentManager.New(contentType);
                //if (idCampaign > 0) {
                //    List<int> lids = new List<int>();
                //    lids.Add(idCampaign);
                //    ((dynamic)newContent).CommunicationAdvertisingPart.Campaign.Ids = lids.ToArray();
                //}
                //  model = _contentManager.BuildEditor(newContent);
                //   _contentManager.Create(newContent);
                model = _contentManager.BuildEditor(newContent);
            } else
                model = _contentManager.BuildEditor(_contentManager.Get(id, VersionOptions.Latest));
            return View((object)model);
        }

        [HttpPost, ActionName("Edit"), Admin]
        public ActionResult EditPOST(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            var typeDef = _contentManager.GetContentTypeDefinitions().FirstOrDefault(x => x.Name == contentType);
            bool draftable = typeDef.Settings.GetModel<ContentTypeSettings>().Draftable;

            ContentItem content;
            if (id == 0) {
                var newContent = _contentManager.New(contentType);

                if (draftable) {
                    _contentManager.Create(newContent, VersionOptions.Draft);
                } else {
                    _contentManager.Create(newContent);
                }

                content = newContent;
            } else {
                if (draftable) {
                    content = _contentManager.Get(id, VersionOptions.DraftRequired);
                } else {
                    content = _contentManager.Get(id);
                }
            }

            var model = _contentManager.UpdateEditor(content, this);
            if (!ModelState.IsValid) {
                foreach (string key in ModelState.Keys) {
                    if (ModelState[key].Errors.Count > 0)
                        foreach (var error in ModelState[key].Errors)
                            _notifier.Add(NotifyType.Error, T(error.ErrorMessage));
                }
                _orchardServices.TransactionManager.Cancel();
                return View(model);
            } else {
                if (draftable) {
                    _contentManager.Publish(content);
                }
            }
            _notifier.Add(NotifyType.Information, T("Contact saved"));
            return RedirectToAction("Edit", new { id = content.Id });
        }

        [HttpPost]
        [Admin]
        public ActionResult Remove(Int32 id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            ContentItem content = _contentManager.Get(id);
            _contentManager.Remove(content);
            return RedirectToAction("Index", "ContactsAdmin");
        }


        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, SearchVM search) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            if (HttpContext.Request["submitFrom"] == null || HttpContext.Request["submitFrom"].ToString() != "submit.Export") {
                return IndexSearch(page, pageSize, search);
            } else {
                return Export(search);
            }
        }

        [HttpPost]
        [Admin]
        public ActionResult IndexSearch(int? page, int? pageSize, SearchVM search) {
            // variabili di appoggio
            List<int> arr = null;

            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            IEnumerable<ContentItem> contentItems = null;
            int totItems = 0;
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, page, pageSize);
            dynamic Options = new System.Dynamic.ExpandoObject();
            var expression = search.Expression;
            IContentQuery<ContentItem> contentQuery = _contentManager.Query(VersionOptions.Latest).ForType(contentType);//.OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc); //Performance issues on heavy ContentItems numbers #6247
            if (!string.IsNullOrEmpty(search.Expression)) {
                switch (search.Field) {
                    case SearchFieldEnum.Name:
                        contentQuery = contentQuery.Where<TitlePartRecord>(w => w.Title.Contains(expression));
                        totItems = contentQuery.Count();
                        contentItems = contentQuery.Slice(pager.GetStartIndex(), pager.PageSize);
                        break;
                    case SearchFieldEnum.Mail:
                        string myQueryMail = @"select cir.Id
                                    from Orchard.ContentManagement.Records.ContentItemVersionRecord as civr
                                    join civr.ContentItemRecord as cir
                                    join cir.EmailContactPartRecord as EmailPart
                                    join EmailPart.EmailRecord as EmailRecord
                                    where EmailRecord.Email like '%' + :mail + '%'
                                    order by cir.Id";
                        var elencoIdMail = _session.For(null)
                            .CreateQuery(myQueryMail)
                            .SetParameter("mail", expression)
                            .List();

                        // alternativa
                        //                        string myQueryMail = @"select EmailContactPartRecord_Id 
                        //                                            from Laser_Orchard_CommunicationGateway_CommunicationEmailRecord 
                        //                                            where Email like '%' + :mail + '%'";
                        //                        var elencoIdMail = _session.For(null)
                        //                            .CreateSQLQuery(myQueryMail)
                        //                            .SetParameter("mail", expression)
                        //                            .List();

                        totItems = elencoIdMail.Count;

                        // tiene conto solo degli item presenti nella pagina da visualizzare
                        arr = new List<int>();
                        for (int idx = 0; (idx < pager.PageSize) && ((idx + pager.GetStartIndex()) < totItems); idx++) {
                            arr.Add((int)(elencoIdMail[idx + pager.GetStartIndex()]));
                        }
                        elencoIdMail = null;
                        contentItems = contentQuery.Where<CommunicationContactPartRecord>(x => arr.Contains(x.Id)).List();
                        break;
                    case SearchFieldEnum.Phone:
                        string myQuerySms = @"select cir.Id
                                    from Orchard.ContentManagement.Records.ContentItemVersionRecord as civr
                                    join civr.ContentItemRecord as cir
                                    join cir.SmsContactPartRecord as SmsPart
                                    join SmsPart.SmsRecord as SmsRecord
                                    where SmsRecord.Sms like '%' + :sms + '%'
                                    order by cir.Id";
                        var elencoIdSms = _session.For(null)
                            .CreateQuery(myQuerySms)
                            .SetParameter("sms", expression)
                            .List();

                        // alternativa
                        //                        string myQuerySms = @"select SmsContactPartRecord_Id 
                        //                                            from Laser_Orchard_CommunicationGateway_CommunicationSmsRecord 
                        //                                            where sms like '%' + :sms + '%'";
                        //                        var elencoIdSms = _session.For(null)
                        //                            .CreateSQLQuery(myQuerySms)
                        //                            .SetParameter("sms", expression)
                        //                            .List();

                        totItems = elencoIdSms.Count;

                        // tiene conto solo degli item presenti nella pagina da visualizzare
                        arr = new List<int>();
                        for (int idx = 0; (idx < pager.PageSize) && ((idx + pager.GetStartIndex()) < totItems); idx++) {
                            arr.Add((int)(elencoIdSms[idx + pager.GetStartIndex()]));
                        }
                        elencoIdSms = null;
                        contentItems = contentQuery.Where<CommunicationContactPartRecord>(x => arr.Contains(x.Id)).List();
                        break;
                }
            } else {
                totItems = contentQuery.Count();
                contentItems = contentQuery.Slice(pager.GetStartIndex(), pager.PageSize);
            }
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(totItems);
            var pageOfContentItems = contentItems
                .Select(p => new ContentIndexVM {
                    Id = p.Id,
                    Title = ((dynamic)p).TitlePart.Title,
                    ModifiedUtc = ((dynamic)p).CommonPart.ModifiedUtc,
                    UserName = ((dynamic)p).CommonPart.Owner != null ? ((dynamic)p).CommonPart.Owner.UserName : "Anonymous",
                    PreviewEmail = (((dynamic)p).EmailContactPart.EmailRecord.Count > 0) ? ((dynamic)p).EmailContactPart.EmailRecord[0].Email : "",
                    PreviewSms = (((dynamic)p).SmsContactPart.SmsRecord.Count > 0) ? ((dynamic)p).SmsContactPart.SmsRecord[0].Sms : ""
                }).ToList();

            if (pageOfContentItems == null) {
                pageOfContentItems = new List<ContentIndexVM>();
            }
            _orchardServices.New.List();
            var model = new SearchIndexVM(pageOfContentItems, search, pagerShape, Options);
            return View((object)model);
        }

        [HttpPost]
        [Admin]
        public ActionResult Export(SearchVM search) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            IEnumerable<ContentItem> contentItems = _exportContactService.GetContactList(search);
            List<ContactExport> listaContatti = new List<ContactExport>();

            foreach (ContentItem contenuto in contentItems) {
                // Contact Master non viene esportato
                if (!contenuto.As<CommunicationContactPart>().Master) {
                    listaContatti.Add(_exportContactService.GetInfoContactExport(contenuto));
                }
            }

            // Export CSV
            StringBuilder strBuilder = new StringBuilder();
            string Separator = ";";
            bool isColumnExist = false;

            foreach (ContactExport contatto in listaContatti) {

                if (!isColumnExist) {
                    #region column
                    strBuilder.Append("Id" + Separator);
                    strBuilder.Append("TitlePart.Title" + Separator);
                    foreach (Hashtable fieldColumn in contatto.Fields) {
                        foreach (DictionaryEntry nameCol in fieldColumn) {
                            strBuilder.Append(nameCol.Key + Separator);
                        }
                    }
                    strBuilder.Append("ContactPart.Sms" + Separator);
                    strBuilder.Append("ContactPart.Email" + Separator);
                    strBuilder.Append("\n");
                    #endregion

                    isColumnExist = true;
                }

                #region row
                strBuilder.Append(contatto.Id.ToString() + Separator);
                strBuilder.Append(contatto.Title + Separator);
                foreach (Hashtable fieldRow in contatto.Fields) {
                    foreach (DictionaryEntry valueRow in fieldRow) {
                        strBuilder.Append(valueRow.Value + Separator);
                    }
                }
                strBuilder.Append(string.Join(",", contatto.Sms) + Separator);
                strBuilder.Append(string.Join(",", contatto.Mail) + Separator);
                strBuilder.Append("\n");
                #endregion
            }

            string fileName = String.Format("contacts_{0}_{1:yyyyMMddHHmmss}.csv", _shellSettings.Name, DateTime.Now);
            byte[] buffer = Encoding.UTF8.GetBytes(strBuilder.ToString());

            FileContentResult file = new FileContentResult(buffer, "text/csv");
            file.FileDownloadName = fileName;

            return file;
        }



        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}