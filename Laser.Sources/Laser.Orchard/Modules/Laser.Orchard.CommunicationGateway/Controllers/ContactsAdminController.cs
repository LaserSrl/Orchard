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
using Orchard.Fields.Fields;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Tasks.Scheduling;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard.Themes;
using Laser.Orchard.CommunicationGateway.Utils;
using Orchard.Environment.Configuration;
using Orchard.Users.Services;
using Laser.Orchard.OpenAuthentication.Services;
using Laser.Orchard.OpenAuthentication.Models;

namespace Laser.Orchard.CommunicationGateway.Controllers {

    public class ContactsAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly string contentType = "CommunicationContact";
        private readonly dynamic TestPermission = Permissions.ManageContact;
        private readonly ICommunicationService _communicationService;
        private readonly IScheduledTaskManager _taskManager;
        private readonly ISessionLocator _session;
        private readonly INotifier _notifier;
        private readonly ShellSettings _shellSettings;
        private readonly IUserService _userService;
        private Localizer T { get; set; }
        private readonly string _contactsExportRelativePath;
        private readonly string _contactsImportRelativePath;

        public ContactsAdminController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager,
            ICommunicationService communicationService,
            ISessionLocator session,
            IScheduledTaskManager taskManager,
            ShellSettings shellSettings,
            IUserService userService
            ) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _communicationService = communicationService;
            _taskManager = taskManager;
            _session = session;
            _shellSettings = shellSettings;
            _userService = userService;
            _contactsExportRelativePath = string.Format("~/Media/{0}/Export/Contacts", _shellSettings.Name);
            _contactsImportRelativePath = string.Format("~/Media/{0}/Import/Contacts", _shellSettings.Name);
        }

        [Admin]
        public ActionResult Synchronize() {
            if (_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner)) {
                // schedula il task per la sincronizzazione
                var scheduledSynchronizations = _taskManager.GetTasks(Handlers.SynchronizeContactTaskHandler.TaskType);
                if (scheduledSynchronizations.Count() == 0) {
                    _taskManager.CreateTask(Handlers.SynchronizeContactTaskHandler.TaskType, DateTime.UtcNow.AddSeconds(5), null);
                    _notifier.Add(NotifyType.Information, T("Synchronization started. Please come back on this page in a few minutes to check the result."));
                }
                else {
                    _notifier.Add(NotifyType.Information, T("Synchronization already scheduled at {0:dd-MM-yyyy HH:mm}. Please come back later on this page to check the result.",  DateTime.SpecifyKind(scheduledSynchronizations.Min(x => x.ScheduledUtc).Value, DateTimeKind.Utc).ToLocalTime()));
                }
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
                // verifica che il contact non sia legato a un utente
                if (content.As<CommunicationContactPart>().UserIdentifier != 0) {
                    return new HttpUnauthorizedResult();
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
            if (!_orchardServices.Authorizer.Authorize(TestPermission)) {
                return new HttpUnauthorizedResult();
            }
            return IndexSearch(page, pageSize, search);
        }

        [HttpGet]
        [Admin]
        public ActionResult IndexSearch(int? page, int? pageSize, SearchVM search) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            if (Request.QueryString["submit.Export"] != null) {
                return Export(search);
            }

            // variabili di appoggio
            List<int> arr = null;

            IEnumerable<ContentItem> contentItems = null;
            int totItems = 0;
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, page, pageSize);
            dynamic Options = new System.Dynamic.ExpandoObject();
            var expression = search.Expression;
            IContentQuery<ContentItem> contentQuery = _contentManager.Query(VersionOptions.Latest).ForType(contentType);//.OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc); //Performance issues on heavy ContentItems numbers #6247
            if (!(string.IsNullOrEmpty(search.Expression) && !search.CommercialUseAuthorization.HasValue && !search.ThirdPartyAuthorization.HasValue)) {
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
                                    where 1 = 1 ";
                        if (!string.IsNullOrEmpty(search.Expression)) myQueryMail += "and EmailRecord.Email like '%' + :mail + '%' ";
                        if (search.CommercialUseAuthorization.HasValue) myQueryMail += "and EmailRecord.AccettatoUsoCommerciale = :commuse ";
                        if (search.ThirdPartyAuthorization.HasValue) myQueryMail += "and EmailRecord.AutorizzatoTerzeParti = :tpuse ";
                        myQueryMail += "order by cir.Id";

                        var mailQueryToExecute = _session.For(null).CreateQuery(myQueryMail);
                        if (!string.IsNullOrEmpty(search.Expression)) mailQueryToExecute.SetParameter("mail", expression);
                        if (search.CommercialUseAuthorization.HasValue) mailQueryToExecute.SetParameter("commuse", search.CommercialUseAuthorization.Value, NHibernateUtil.Boolean);
                        if (search.ThirdPartyAuthorization.HasValue) mailQueryToExecute.SetParameter("tpuse", search.ThirdPartyAuthorization.Value, NHibernateUtil.Boolean);

                        var elencoIdMail = mailQueryToExecute.List();

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
                                    where 1 = 1 ";
                        if (!string.IsNullOrEmpty(search.Expression)) myQuerySms += "and SmsRecord.Sms like '%' + :sms + '%' ";
                        if (search.CommercialUseAuthorization.HasValue) myQuerySms += "and SmsRecord.AccettatoUsoCommerciale = :commuse ";
                        if (search.ThirdPartyAuthorization.HasValue) myQuerySms += "and SmsRecord.AutorizzatoTerzeParti = :tpuse ";
                        myQuerySms += "order by cir.Id";

                        var smsQueryToExecute = _session.For(null).CreateQuery(myQuerySms);
                        if (!string.IsNullOrEmpty(search.Expression)) smsQueryToExecute.SetParameter("sms", expression);
                        if (search.CommercialUseAuthorization.HasValue) smsQueryToExecute.SetParameter("commuse", search.CommercialUseAuthorization.Value, NHibernateUtil.Boolean);
                        if (search.ThirdPartyAuthorization.HasValue) smsQueryToExecute.SetParameter("tpuse", search.ThirdPartyAuthorization.Value, NHibernateUtil.Boolean);

                        var elencoIdSms = smsQueryToExecute.List();

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
                    PreviewSms = (((dynamic)p).SmsContactPart.SmsRecord.Count > 0) ? ((dynamic)p).SmsContactPart.SmsRecord[0].Prefix + "/" + ((dynamic)p).SmsContactPart.SmsRecord[0].Sms : "",
                    UserId = ((dynamic)p).CommunicationContactPart.UserIdentifier
                }).ToList();

            if (pageOfContentItems == null) {
                pageOfContentItems = new List<ContentIndexVM>();
            }
            //_orchardServices.New.List();
            AddProviderInfo(pageOfContentItems);
            var model = new SearchIndexVM(pageOfContentItems, search, pagerShape, Options);
            return View("Index", (object)model);
        }

        private void AddProviderInfo(List<ContentIndexVM> items) {
            IUserProviderServices userProviderService = null;
            UserProviderRecord provider = null;
            if(_orchardServices.WorkContext.TryResolve<IUserProviderServices>(out userProviderService)){
                foreach (var item in items) {
                    if (item.UserId != 0) {
                        provider = userProviderService.Get(item.UserId).FirstOrDefault();
                        if (provider == null) {
                            item.Provider = "Local";
                        } else {
                            item.Provider = provider.ProviderName;
                        }
                    } else {
                        item.Provider = "Contact";
                    }
                }
            }
        }

        private ActionResult Export(SearchVM search) {
            
            string parametri = "expression=" + Url.Encode(search.Expression) + "&field=" + Url.Encode(search.Field.ToString());
            parametri += search.CommercialUseAuthorization.HasValue ? String.Format("&commercialuse={0}", search.CommercialUseAuthorization.Value.ToString()) : "&commercialuse=";
            parametri += search.ThirdPartyAuthorization.HasValue ? String.Format("&thirdparty={0}", search.ThirdPartyAuthorization.Value.ToString()) : "&thirdparty=";

            var ContentExport = _orchardServices.ContentManager.Create("ExportTaskParameters");
            ((dynamic)ContentExport).ExportTaskParametersPart.Parameters.Value = parametri;

            _taskManager.CreateTask("Laser.Orchard.CommunicationGateway.ExportContact.Task", DateTime.UtcNow.AddSeconds(5), (ContentItem)ContentExport);

            _notifier.Add(NotifyType.Information, T("Export started. Please check 'Show Exported Files' to get the result."));
            return RedirectToAction("Index", "ContactsAdmin");
        }

        [Admin]
        public ActionResult ImportCsv(System.Web.HttpPostedFileBase csvFile) {
            string path = Server.MapPath(_contactsImportRelativePath);
            if (csvFile != null) {
                var len = csvFile.ContentLength;
                if (len > 0) {
                    DirectoryInfo dir = new DirectoryInfo(path);
                    if (dir.Exists == false) {
                        dir.Create();
                    }
                    FileInfo file = new FileInfo(string.Format("{0}{1}{2}", path, Path.DirectorySeparatorChar, csvFile.FileName));
                    if(file.Exists){
                        _notifier.Error(T("File already exist. Import aborted."));
                    }else{
                        csvFile.SaveAs(file.FullName);

                        // schedula il task per l'import
                        var scheduledImports = _taskManager.GetTasks(Handlers.ImportContactTaskHandler.TaskType).Count();
                        if(scheduledImports == 0){
                            _taskManager.CreateTask(Handlers.ImportContactTaskHandler.TaskType, DateTime.UtcNow.AddSeconds(5), null);
                        }
                        _notifier.Add(NotifyType.Information, T("Import started. Please check 'Show Import Logs' to get the result."));
                    }
                }
            } else {
                _notifier.Warning(T("Import Contacts: Please select a file to import."));
            }
            return IndexSearch(null, null, new SearchVM());
        }

        [Admin]
        public ActionResult ExportedFilesList(FilesListVM model) {
            model.FilePath = _contactsExportRelativePath.Replace("~", Request.ApplicationPath);
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Server.MapPath(_contactsExportRelativePath));
            if (dir.Exists == false) {
                dir.Create();
            }  
            var files = dir.GetFiles("*.csv", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                model.FileInfos.Add(file);
            }
            model.FileInfos = model.FileInfos.OrderByDescending(x => x.LastWriteTimeUtc).ToList();
            return View("ExportedFilesList", model);
        }

        [Admin]
        public ActionResult ImportedFilesLogs(FilesListVM model) {
            model.FilePath = _contactsImportRelativePath.Replace("~", Request.ApplicationPath);
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Server.MapPath(_contactsImportRelativePath));
            if (dir.Exists == false) {
                dir.Create();
            }
            var files = dir.GetFiles("*.log", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                model.FileInfos.Add(file);
            }
            model.FileInfos = model.FileInfos.OrderByDescending(x => x.LastWriteTimeUtc).ToList();
            return View("ImportedFilesLogs", model);
        }

        //[Admin]
        public ActionResult DownloadCsvFile(string fName) {
            //return new DownloadFileContentResult(Server.MapPath(_contactsExportRelativePath + "/" + fName), fName, "text/csv");
            //return File(Server.MapPath(_contactsExportRelativePath + "/" + fName), "application/octet-stream", fName);

            // il metodo seguente, contrariamente a quelli nelle righe precedenti, preserva la corretta codifica dei caratteri accentati
            Response.AppendHeader("content-disposition", "attachment; filename=" + fName);
            var result = new ContentResult();
            result.Content = System.IO.File.ReadAllText(Server.MapPath(_contactsExportRelativePath + "/" + fName));
            result.ContentEncoding = Encoding.UTF8;
            result.ContentType = "text/csv";
            return result;
        }

        [Admin]
        public ActionResult DownloadLogFile(string fName) {
            // il tipo MIME "application/octet-stream" serve a forzare il download del file
            //return new DownloadFileContentResult(Server.MapPath(_contactsImportRelativePath + "/" + fName), fName, "application/octet-stream");
            //return File(System.IO.File.ReadAllBytes(Server.MapPath(_contactsImportRelativePath + "/" + fName)), "application/octet-stream", fName);

            // il metodo seguente, contrariamente a quelli nelle righe precedenti, preserva la corretta codifica dei caratteri accentati
            Response.AppendHeader("content-disposition", "attachment; filename=" + fName);
            var result = new ContentResult();
            result.Content = System.IO.File.ReadAllText(Server.MapPath(_contactsImportRelativePath + "/" + fName));
            result.ContentEncoding = Encoding.UTF8;
            result.ContentType = "text/plain";
            return result;
        }

        [Admin]
        public ActionResult RemoveExportFile(string fName) {
            System.IO.FileInfo file = new System.IO.FileInfo(Server.MapPath(_contactsExportRelativePath + "/" + fName));
            if (file.Exists) {
                file.Delete();
                _notifier.Information(T("File removed."));
            } else {
                _notifier.Error(T("File does not exist. It should have been removed by someone else."));
            }
            return RedirectToAction("ExportedFilesList");
        }

        [Admin]
        public ActionResult RemoveImportLog(string fName) {
            System.IO.FileInfo file = new System.IO.FileInfo(Server.MapPath(_contactsImportRelativePath + "/" + fName));
            if (file.Exists) {
                file.Delete();
                _notifier.Information(T("File removed."));
            } else {
                _notifier.Error(T("File does not exist. It should have been removed by someone else."));
            }
            return RedirectToAction("ImportedFilesLogs");
        }

        [Admin]
        public ActionResult View(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            object model;
            if (id == 0) {
                var newContent = _contentManager.New(contentType);
                model = _contentManager.BuildDisplay(newContent, "Detail");
            } else {
                model = _contentManager.BuildDisplay(_contentManager.Get(id, VersionOptions.Latest), "Detail");
            }
            return View((object)model);

            //return RedirectToAction("Edit", new { id = contactId });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}