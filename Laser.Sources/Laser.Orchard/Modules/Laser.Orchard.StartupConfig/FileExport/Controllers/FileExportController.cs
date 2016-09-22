using Laser.Orchard.StartupConfig.FileExport.ViewModels;
using Orchard;
using Orchard.Security.Permissions;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.FileExport.Controllers {
    public class FileExportController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly INotifier _notifier;
        private readonly ShellSettings _shellSettings;
        private readonly string _fileExportRelativePath;
        private Localizer T { get; set; }

        public FileExportController(IOrchardServices orchardServices, INotifier notifier, ShellSettings shellSettings) {
            _orchardServices = orchardServices;
            _notifier = notifier;
            _shellSettings = shellSettings;
            T = NullLocalizer.Instance;
            _fileExportRelativePath = string.Format("~/App_Data/Sites/{0}/Export", _shellSettings.Name);
        }
        private bool CheckPermission(string folderName) {
            var accessPermission = Permission.Named(string.Format("AccessExport{0}", folderName));
            return _orchardServices.Authorizer.Authorize(accessPermission);
        }
        [HttpGet]
        [Admin]
        public ActionResult Index(FilesListVM model, PagerParameters pagerParameters) {
            // controlla i permessi dell'utente
            if (CheckPermission(model.FolderName) == false) {
                return new HttpUnauthorizedResult();
            }
            // crea la struttura di cartelle se necessario
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(Server.MapPath(_fileExportRelativePath));
            if (dir.Exists == false) {
                dir.Create();
            }
            dir = new System.IO.DirectoryInfo(Server.MapPath(_fileExportRelativePath + "/" + model.FolderName));
            if (dir.Exists == false) {
                dir.Create();
            }
            var files = dir.GetFiles("*", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var file in files) {
                model.FileInfos.Add(file);
            }
            model.FileInfos = model.FileInfos.OrderByDescending(x => x.LastWriteTimeUtc).ToList();
            // gestione della paginazione
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(model.FileInfos.Count);
            model.FileInfos = model.FileInfos.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();
            model.Pager = pagerShape;
            return View("Index", model);
        }
        public ActionResult DownloadCsvFile(string fName, string folderName) {
            // controlla i permessi dell'utente
            if (CheckPermission(folderName) == false) {
                return new HttpUnauthorizedResult();
            }
            var fPath = Server.MapPath(_fileExportRelativePath + "/" + folderName + "/" + fName);
            return File(fPath, "application/octet-stream", fName);
        }
        [Admin]
        public ActionResult RemoveExportFile(string fName, string folderName, string urlBack) {
            // controlla i permessi dell'utente
            if (CheckPermission(folderName) == false) {
                return new HttpUnauthorizedResult();
            }
            var fPath = Server.MapPath(_fileExportRelativePath + "/" + folderName + "/" + fName);
            System.IO.FileInfo file = new System.IO.FileInfo(fPath);
            if (file.Exists) {
                file.Delete();
                _notifier.Information(T("File removed."));
            }
            else {
                _notifier.Error(T("File does not exist. It should have been removed by someone else."));
            }
            return RedirectToAction("Index", new { UrlBack = urlBack, FolderName = folderName });
        }
    }
}