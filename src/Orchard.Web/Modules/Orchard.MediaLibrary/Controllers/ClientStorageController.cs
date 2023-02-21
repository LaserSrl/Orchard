﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.MediaLibrary.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.MediaLibrary.Controllers {
    [Admin, Themed(false)]
    public class ClientStorageController : Controller {
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IMimeTypeProvider _mimeTypeProvider;
        private readonly IStorageProvider _storageProvider;

        public ClientStorageController(
            IMediaLibraryService mediaManagerService,
            IOrchardServices orchardServices,
            IMimeTypeProvider mimeTypeProvider,
            IStorageProvider storageProvider) {
            _mediaLibraryService = mediaManagerService;
            _mimeTypeProvider = mimeTypeProvider;
            _storageProvider = storageProvider; 
            Services = orchardServices;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;




        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index(string folderPath, string type, int? replaceId = null) {
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.SelectMediaContent, folderPath)) {
                return new HttpUnauthorizedResult();
            }

            // Check permission
            if (!_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            var viewModel = new ImportMediaViewModel {
                FolderPath = folderPath,
                Type = type,
            };

            if (replaceId != null) {
                var replaceMedia = Services.ContentManager.Get<MediaPart>(replaceId.Value);
                if (replaceMedia == null)
                    return HttpNotFound();

                viewModel.Replace = replaceMedia;
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult ChunkUpload([ModelBinder(typeof(ChunkUploadRequestBinder))] ChunkUploadRequest request) {
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, request.UploadFolder)) {
                return new HttpUnauthorizedResult();
            }

            if (!_mediaLibraryService.CanManageMediaFolder(request.UploadFolder)) {
                return new HttpUnauthorizedResult();
            }

            var filename = Path.GetFileName(request.OriginalFile.FileName);
            var statuses = new List<object>();

            var upload = true;

            // if the file has been pasted, provide a default name
            if (request.OriginalFile.ContentType.Equals("image/png", StringComparison.InvariantCultureIgnoreCase) && !filename.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)) {
                filename = "clipboard.png";
            }

            // skip file if the allowed extensions is defined and doesn't match
            var settings = Services.WorkContext.CurrentSite.As<MediaLibrarySettingsPart>();
            if (!settings.IsFileAllowed(filename)) {
                statuses.Add(new {
                    error = T("This file is not allowed: {0}", filename).Text,
                    progress = 1.0,
                });
                upload = false;
            }

            if (upload) {
                var path = _storageProvider.Combine("D:\\Laser.Orchard\\Laser.Orchard\\Orchard\\src\\Orchard.Web\\Media\\Default\\Pictures", filename); // create path

                FileStatus status = null;

                try {
                    if (request.IsChunk) {
                        if (request.IsFirst) {
                            // do some stuff that has to be done before the file starts uploading
                            var a = 0;
                        }

                        var inputStream = request.OriginalFile.InputStream;

                        using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write)) {
                            var buffer = new byte[1024];

                            var l = inputStream.Read(buffer, 0, 1024);
                            while (l > 0) {
                                fs.Write(buffer, 0, l);
                                l = inputStream.Read(buffer, 0, 1024);
                            }

                            fs.Flush();
                            fs.Close();
                        }

                        status = new FileStatus(new FileInfo(path));

                        if (request.IsLast) {
                            // do some stuff that has to be done after the file is uploaded
                            var a = 0;

                            try {
                                var mediaPart = _mediaLibraryService.ImportMedia(new FileStream(path, FileMode.Open, FileAccess.Read), request.UploadFolder, filename, request.MediaType);
                                Services.ContentManager.Create(mediaPart);

                                statuses.Add(new {
                                    id = mediaPart.Id,
                                    name = mediaPart.Title,
                                    type = mediaPart.MimeType,
                                    size = request.OriginalFile.ContentLength,
                                    progress = 1.0,
                                    url = mediaPart.FileName,
                                });
                            } catch (Exception ex) {
                                Logger.Error(ex, "Unexpected exception when uploading a media.");
                                statuses.Add(new {
                                    error = T(ex.Message).Text,
                                    progress = 1.0,
                                });
                            }
                        }
                    } else {
                        //file.SaveAs(path);
                        status = new FileStatus(new FileInfo(path));
                    }
                } catch {
                    statuses.Add(new {
                        error = T("Something went wrong").Text,
                        progress = 1.0,
                    });
                }

                //// this is just a browser json support/compatibility workaround
                //if (request.JsonAccepted)
                //    return Json(status);
                //else
                //    return Json(status, "text/plain");
            }
            

            // Return JSON
            return Json(statuses, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Upload(string folderPath, string type) {
            if (!_mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, folderPath)) {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
                return new HttpUnauthorizedResult();
            }

            // Check permission
            if (!_mediaLibraryService.CanManageMediaFolder(folderPath)) {
                return new HttpUnauthorizedResult();
            }

            var statuses = new List<object>();
            var settings = Services.WorkContext.CurrentSite.As<MediaLibrarySettingsPart>();

            // Loop through each file in the request
            for (int i = 0; i < HttpContext.Request.Files.Count; i++) {
                // Pointer to file
                var file = HttpContext.Request.Files[i];
                var filename = Path.GetFileName(file.FileName);

                // if the file has been pasted, provide a default name
                if (file.ContentType.Equals("image/png", StringComparison.InvariantCultureIgnoreCase) && !filename.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)) {
                    filename = "clipboard.png";
                }

                // skip file if the allowed extensions is defined and doesn't match
                if (!settings.IsFileAllowed(filename)) {
                    statuses.Add(new {
                        error = T("This file is not allowed: {0}", filename).Text,
                        progress = 1.0,
                    });
                    continue;
                }

                try {
                    var mediaPart = _mediaLibraryService.ImportMedia(file.InputStream, folderPath, filename, type);
                    Services.ContentManager.Create(mediaPart);

                    statuses.Add(new {
                        id = mediaPart.Id,
                        name = mediaPart.Title,
                        type = mediaPart.MimeType,
                        size = file.ContentLength,
                        progress = 1.0,
                        url = mediaPart.FileName,
                    });
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Unexpected exception when uploading a media.");
                    statuses.Add(new {
                        error = T(ex.Message).Text,
                        progress = 1.0,
                    });
                }
            }

            // Return JSON
            return Json(statuses, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Replace(int replaceId, string type) {
            if (!Services.Authorizer.Authorize(Permissions.ManageOwnMedia))
                return new HttpUnauthorizedResult();

            var replaceMedia = Services.ContentManager.Get<MediaPart>(replaceId);
            if (replaceMedia == null)
                return HttpNotFound();

            // Check permission
            if (!(_mediaLibraryService.CheckMediaFolderPermission(Permissions.EditMediaContent, replaceMedia.FolderPath) && _mediaLibraryService.CheckMediaFolderPermission(Permissions.ImportMediaContent, replaceMedia.FolderPath)) 
                && !_mediaLibraryService.CanManageMediaFolder(replaceMedia.FolderPath)) {
                return new HttpUnauthorizedResult();
            }

            var statuses = new List<object>();

            var settings = Services.WorkContext.CurrentSite.As<MediaLibrarySettingsPart>();
            
            // Loop through each file in the request
            for (int i = 0; i < HttpContext.Request.Files.Count; i++) {
                // Pointer to file
                var file = HttpContext.Request.Files[i];
                var filename = Path.GetFileName(file.FileName);

                // if the file has been pasted, provide a default name
                if (file.ContentType.Equals("image/png", StringComparison.InvariantCultureIgnoreCase) && !filename.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)) {
                    filename = "clipboard.png";
                }

                // skip file if the allowed extensions is defined and doesn't match
                if (!settings.IsFileAllowed(filename)) {
                    statuses.Add(new {
                        error = T("This file is not allowed: {0}", filename).Text,
                        progress = 1.0,
                    });
                    continue;
                }

                try {
                    var mimeType = _mimeTypeProvider.GetMimeType(filename);

                    string replaceContentType = _mediaLibraryService.MimeTypeToContentType(file.InputStream, mimeType, type) ?? type;
                    if (!replaceContentType.Equals(replaceMedia.TypeDefinition.Name, StringComparison.OrdinalIgnoreCase))
                        throw new Exception(T("Cannot replace {0} with {1}", replaceMedia.TypeDefinition.Name, replaceContentType).Text);

                    var mediaItemsUsingTheFile = Services.ContentManager.Query<MediaPart, MediaPartRecord>()
                                                                .ForVersion(VersionOptions.Latest)
                                                                .Where(x => x.FolderPath == replaceMedia.FolderPath && x.FileName == replaceMedia.FileName)
                                                                .Count();
                    if (mediaItemsUsingTheFile == 1) { // if the file is referenced only by the deleted media content, the file too can be removed.
                        try {
                            _mediaLibraryService.DeleteFile(replaceMedia.FolderPath, replaceMedia.FileName);
                        } catch (ArgumentException) { // File not found by FileSystemStorageProvider is thrown as ArgumentException.
                            statuses.Add(new {
                                error = T("Error when deleting file to replace: file {0} does not exist in folder {1}. Media has been updated anyway.", replaceMedia.FileName, replaceMedia.FolderPath).Text,
                                progress = 1.0
                            });
                        }
                    }
                    else {
                        // it changes the media file name
                        replaceMedia.FileName = filename;
                    }
                    _mediaLibraryService.UploadMediaFile(replaceMedia.FolderPath, replaceMedia.FileName, file.InputStream);
                    replaceMedia.MimeType = mimeType;

                    // Force a publish event which will update relevant Media properties
                    replaceMedia.ContentItem.VersionRecord.Published = false;
                    Services.ContentManager.Publish(replaceMedia.ContentItem);

                    statuses.Add(new {
                        id = replaceMedia.Id,
                        name = replaceMedia.Title,
                        type = replaceMedia.MimeType,
                        size = file.ContentLength,
                        progress = 1.0,
                        url = replaceMedia.FileName,
                    });
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Unexpected exception when uploading a media.");

                    statuses.Add(new {
                        error = T(ex.Message).Text,
                        progress = 1.0,
                    });
                }
            }

            return Json(statuses, JsonRequestBehavior.AllowGet);
        }
    }
}
