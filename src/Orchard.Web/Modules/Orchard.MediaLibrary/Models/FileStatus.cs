using System.IO;

namespace Orchard.MediaLibrary.Models {
    public class FileStatus {
        public const string HandlerPath = "/";

        public string Group { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Size { get; set; }
        public string Progress { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        public string DeleteUrl { get; set; }
        public string DeleteType { get; set; }
        public string Error { get; set; }

        public FileStatus() {
        }

        public FileStatus(FileInfo fileInfo) {
            SetValues(fileInfo.Name, (int)fileInfo.Length, fileInfo.FullName);
        }

        public FileStatus(string fileName, int fileLength, string fullPath) {
            SetValues(fileName, fileLength, fullPath);
        }

        private void SetValues(string fileName, int fileLength, string fullPath) {
            Name = fileName;
            Type = "image/png";
            Size = fileLength;
            Progress = "1.0";
            Url = HandlerPath + "/file/upload?f=" + fileName;
            DeleteUrl = HandlerPath + "/file/delete?f=" + fileName;
            DeleteType = "DELETE";
            ThumbnailUrl = "/Content/img/generalFile.png";
        }
    }
}