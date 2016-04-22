using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Laser.Orchard.CommunicationGateway.ViewModels {
    public class ExportedFilesListVM {
        public List<FileSystemInfo> FileInfos { get; set; }
        public string ExportedFilePath { get; set; }

        public ExportedFilesListVM() {
            FileInfos = new List<FileSystemInfo>();
            ExportedFilePath = "";
        }
    }
}