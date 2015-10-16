using IXMSDLibrary;
using Laser.Orchard.IXMSD.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Configuration;
using Orchard.MediaLibrary.Models;
using System;
using System.Threading;
using System.Web.Hosting;

namespace Laser.Orchard.IXMSD.Handlers {

    public class IXMSDPartHandler : ContentHandler {
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;

        public IXMSDPartHandler(IOrchardServices orchardServices, ShellSettings shellSettings) {
            _orchardServices = orchardServices;
            _shellSettings = shellSettings;

            OnLoaded<IXMSDPart>((context, part) => {
                if (!String.IsNullOrEmpty(part.ExternalMediaUrl)) {
                    part.ContentItem.As<MediaPart>()._publicUrl.Loader(x => part.ExternalMediaUrl);
                }
            });

            OnPublished<IXMSDPart>((context, IXMSDPart) => {
                var getpart = _orchardServices.WorkContext.CurrentSite.As<IXMSDSettingsPart>();
                if (getpart != null) {
                   // if (!string.IsNullOrEmpty(getpart.URL)) {
                        if (string.IsNullOrEmpty(IXMSDPart.ExternalMediaUrl) || IXMSDPart.ExternalMediaUrl.ToLower().IndexOf(IXMSDPart.ContentItem.As<MediaPart>().FileName.ToLower()) < 0) {
                            string extensionfile = IXMSDPart.ContentItem.As<MediaPart>().FileName.Substring(IXMSDPart.ContentItem.As<MediaPart>().FileName.LastIndexOf('.'));
                            string newFileName = (_shellSettings.Name + "-" + IXMSDPart.ContentItem.Id.ToString() + extensionfile).Replace("_", "-").Replace(" ", "-");  //IXMSDPart.ContentItem.As<MediaPart>().FileName).Replace("_", "-").Replace(" ", "-");
                            UploadFiles UploadFiles = new UploadFiles();
                            UploadInfo UploadInfo = new UploadInfo();
                            UploadInfo.FinalPath = HostingEnvironment.MapPath(@"~\Media\" + _shellSettings.Name + @"\" + IXMSDPart.ContentItem.As<MediaPart>().FolderPath + @"\" + IXMSDPart.ContentItem.As<MediaPart>().FileName);
                            UploadInfo.Nome = newFileName;// (_shellSettings.Name + "-" + ((IXMSDPart.ContentItem.As<MediaPart>().MediaUrl).Split('/').Last())).Replace("_", "-");
                            Thread oThread = new Thread(() => UploadFiles.FtpUpload(UploadInfo, getpart.ftpRemoteHost, getpart.ftpUser, getpart.ftpPwd, HostingEnvironment.MapPath("~/") + @"App_Data\Logs\"));
                            oThread.Start();
                            IXMSDPart.ContentItem.As<MediaPart>().FileName = newFileName;
                        }
              //      }
                }
            });
        }
    }
}