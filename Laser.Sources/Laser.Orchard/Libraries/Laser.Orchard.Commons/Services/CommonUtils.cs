using System.IO;

namespace Laser.Orchard.Commons.Services {
    public class CommonUtils {
        public string NormalizeFileName(string fileName, string defaultFileName, char replacementChar = '_') {
            var normalizedFileName = defaultFileName;
            var tempFileName = fileName;
            foreach (var badChar in Path.GetInvalidFileNameChars()) {
                tempFileName.Replace(badChar, replacementChar);
            }
            if (string.IsNullOrWhiteSpace(tempFileName.Trim()) == false) {
                normalizedFileName = tempFileName;
            }
            return normalizedFileName;
        }
    }
}
