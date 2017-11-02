using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Laser.Orchard.StartupConfig.WebApiProtection.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.AppData;
using Orchard.Utility.Extensions;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IApiKeySettingService : ISingletonDependency {
        string EncryptionKeys(string key);
        void Refresh();
    }
    public class ApiKeySettingService : IApiKeySettingService {
        private Dictionary<string, string> _encryptionKeys;
        private readonly IOrchardServices _orchardServices;
        private readonly ShellSettings _shellSettings;
        private readonly IAppDataFolder _appDataFolder;
        public ApiKeySettingService(IOrchardServices orchardServices, ShellSettings shellSettings, IAppDataFolder appDataFolder) {
            _shellSettings = shellSettings;
            _appDataFolder = appDataFolder;
            _orchardServices = orchardServices;
            ReadFileSetting();
        }
        private string ReadFileSetting() {
            var filePath = Path.Combine(Path.Combine("Sites", _shellSettings.Name), "ApiSetting.txt");
            var content = "";
            _encryptionKeys = new Dictionary<string, string>();
            if (!_appDataFolder.FileExists(filePath)) {
                var key = "";
                content = "TheDefaultChannel" + ":" + key + Environment.NewLine;
                _appDataFolder.CreateFile(filePath, content);
            }
            else {
                var filecontent = _appDataFolder.ReadFile(filePath);
                var lines = filecontent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines) {
                    var keyval = line.Split(':');
                    if (keyval.Length > 1 && (!string.IsNullOrEmpty(line.Substring(keyval[0].Length + 1).Trim())))
                        _encryptionKeys.Add(keyval[0], line.Substring(keyval[0].Length + 1).Trim());
                }
                content = filecontent;
            }
            return content;
        }
        public void Refresh() {
            var filePath = Path.Combine(Path.Combine("Sites", _shellSettings.Name), "ApiSetting.txt");
            var content = ReadFileSetting();
            var savefile = false;
            var settings = _orchardServices.WorkContext.CurrentSite.As<ProtectionSettingsPart>();
            var builder = new System.Text.StringBuilder();
            builder.Append(content);
            foreach (var set in settings.ExternalApplicationList.ExternalApplications) {
                if (!_encryptionKeys.Keys.Contains(set.Name)) {
                    savefile = true;
                    var key = "";
                    builder.Append(set.Name + ":" + key + Environment.NewLine);
                }
            }
            content = builder.ToString();
            if (savefile)
                _appDataFolder.CreateFile(filePath, content);
        }
        public string EncryptionKeys(string key) {
            if (!string.IsNullOrEmpty(key) && _encryptionKeys.Keys.Contains(key))
                return _encryptionKeys[key];
            if (_encryptionKeys.Keys.Contains("TheDefaultChannel"))
                return _encryptionKeys["TheDefaultChannel"];
            else
                return _shellSettings.EncryptionKey;
        }
    }

}