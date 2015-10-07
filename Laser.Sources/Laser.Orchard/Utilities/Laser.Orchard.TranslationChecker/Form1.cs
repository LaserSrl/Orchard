using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Laser.Orchard.TranslationChecker.Models;

namespace Laser.Orchard.TranslationChecker {

    public partial class Form1 : Form {
        const string PO_MODULE_FILENAME = "orchard.module.po";
        const string PO_THEME_FILENAME = "orchard.theme.po";

        const string T_REGEX = "(\\s|\\@)+T\\((\"[^\"]*\")[^\\)]*\\)";
        const string NAMESPACE_REGEX = @"namespace\s+([^{\r\:\s]+)";
        const string CLASSNAME_REGEX = @"\s+class\s+([^{\r\:\s]+)";
        const string PO_REGEX = @"msgctxt\s+{0}[\r|\n]+msgid\s+{1}";
        const string PO_SUBSTRINGS_REGEX = @"[\\\.\(\[\)\]\{\}\*\?\!\<\>]{1}"; // these chars will be escaped in the match regex \.()[]{}*?!<>

        private string _folder, _baseFolderModules, _baseFolderThemes;
        private string[] _modulesFolders, _themesFolders;
        private List<TranslationMessage> _translationMessages;

        public Form1() {
            InitializeComponent();
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        private void Form1_Load(object sender, EventArgs e) {
            InitializeData();
        }
        private void btnCheckTraduzioni_Click(object sender, EventArgs e) {
            ResetData();
            CheckMissingTranslations(this.cmbLingue.SelectedValue.ToString());
        }

        private void InitializeData() {
            var bookmarkPath = @"Laser.Sources\Laser.Orchard";
            _folder = Application.StartupPath.Substring(0, Application.StartupPath.IndexOf(bookmarkPath) + bookmarkPath.Length);
            _baseFolderModules = Path.Combine(_folder, "Modules");
            _baseFolderThemes = Path.Combine(_folder, "Themes");
            _modulesFolders = Directory.GetDirectories(_baseFolderModules);
            _themesFolders = Directory.GetDirectories(_baseFolderThemes);
            _translationMessages = new List<TranslationMessage>();

            var languages = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            this.cmbLingue.DataSource = languages;
            this.cmbLingue.ValueMember = "Name";
            this.cmbLingue.DisplayMember = "DisplayName";
            this.panel1.Visible = false;
            this.lblTranslatorWsUrl.Text = ConfigurationManager.AppSettings["RemoteTranslationBaseUrl"];

        }

        private void ResetData() {
            _translationMessages.Clear();
            this.chkTranslations.Items.Clear();
            this.panel1.Visible = false;
        }
        private void CheckMissingTranslations(string language) {
            //Modules check
            this.txtLogOperations.AppendText("===============================================\r\n");
            this.txtLogOperations.AppendText(String.Concat("MODULES", "\r\n"));
            this.txtLogOperations.AppendText("===============================================\r\n");
            foreach (var moduleFolder in _modulesFolders) {
                ParseT(moduleFolder, language, TranslationArea.Modules);
            }
            //Themes check
            this.txtLogOperations.AppendText("===============================================\r\n");
            this.txtLogOperations.AppendText(String.Concat("THEMES", "\r\n"));
            this.txtLogOperations.AppendText("===============================================\r\n");
            foreach (var themeFolder in _themesFolders) {
                ParseT(themeFolder, language, TranslationArea.Themes);
            }

            foreach (var item in _translationMessages.Select(x => x.ContainerName).Distinct()) {
                this.chkTranslations.Items.Add(item, true);
            }
            this.panel1.Visible = true;
        }

        private void ParseT(string folderPath, string language, TranslationArea translationArea) {
            this.txtLogOperations.AppendText("===============================================\r\n");
            this.txtLogOperations.AppendText(String.Concat(folderPath, "\r\n"));
            var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".cs") || s.EndsWith(".cshtml"));
            foreach (var file in files) {
                string matchContext;
                if (file.EndsWith(".cshtml")) {
                    matchContext = file.Replace(_folder, "~").Replace(@"\", "/");
                } else {
                    matchContext = ""; // reset value if file is a .cs
                }
                var fileContent = File.ReadAllText(file).Replace("\\\"", "-------");
                var matches = Regex.Matches(fileContent, T_REGEX);
                foreach (Match match in matches) {
                    var matchingString = match.Groups[2].Value.Replace("-------", "\\\"");
                    if (matchContext == "") {
                        matchContext = GetClassContext(fileContent, match.Groups[1].Index);
                    }
                    if (!matchContext.StartsWith("\"")) {
                        matchContext = String.Concat("\"", matchContext);
                    }
                    if (!matchContext.EndsWith("\"")) {
                        matchContext = String.Concat(matchContext, "\"");
                    }
                    matchContext = matchContext.Replace("-------", "\\\"");
                    var translationMessage = new TranslationMessage {
                        ContainerType = translationArea,
                        ContainerName = Path.GetFileName(folderPath),
                        MessageContext = matchContext,
                        MessageId = matchingString,
                        MessageLanguage = language
                    };
                    if (!_translationMessages.Any(x =>
                        x.MessageContext.Equals(translationMessage.MessageContext) &&
                        x.MessageId.Equals(translationMessage.MessageId) &&
                        x.ContainerName.Equals(translationMessage.ContainerName) &&
                        x.ContainerType.Equals(translationMessage.ContainerType) &&
                        x.ContainerName.Equals(translationMessage.ContainerName) &&
                        x.MessageLanguage.Equals(translationMessage.MessageLanguage))) {
                        if (!TranslationExists(folderPath, language, matchContext, matchingString, translationArea)) {
                            this.txtLogOperations.AppendText(String.Concat("msgctxt ", matchContext, "\r\n"));
                            this.txtLogOperations.AppendText(String.Concat("msgid ", matchingString, "\r\n"));
                            _translationMessages.Add(translationMessage);
                        }
                    }
                }
            }

        }

        private string GetClassContext(string fileContent, int index) {
            var returnValue = "";
            var contentBeforeMatch = fileContent.Substring(0, index);
            var namespaceMatches = Regex.Matches(contentBeforeMatch, NAMESPACE_REGEX, RegexOptions.IgnoreCase);
            var namespaceString = namespaceMatches[namespaceMatches.Count - 1].Groups[1].Value;
            var classMatches = Regex.Matches(contentBeforeMatch, CLASSNAME_REGEX, RegexOptions.IgnoreCase);
            var classNameString = classMatches[classMatches.Count - 1].Groups[1].Value;
            returnValue = String.Concat(namespaceString, ".", classNameString);
            return returnValue;
        }

        private bool TranslationExists(string folderPath, string language, string messageContext, string messageId, TranslationArea translationArea) {
            var localizationFolder = Path.Combine(folderPath, @"App_Data\Localization", language);
            string localizationFile;
            if (translationArea == TranslationArea.Modules) {
                localizationFile = Path.Combine(localizationFolder, PO_MODULE_FILENAME);
            } else {
                localizationFile = Path.Combine(localizationFolder, PO_THEME_FILENAME);
            }


            if (!File.Exists(localizationFile)) return false;
            var poContent = File.ReadAllText(localizationFile);
            return Regex.IsMatch(poContent, String.Format(PO_REGEX, Regex.Replace(messageContext, PO_SUBSTRINGS_REGEX, @"\$&"), Regex.Replace(messageId, PO_SUBSTRINGS_REGEX, @"\$&")));
        }

        private void btnCheckUncheckAll_Click(object sender, EventArgs e) {
            var check = (this.chkTranslations.CheckedItems.Count < this.chkTranslations.Items.Count);
            for (int i = 0; i < this.chkTranslations.Items.Count; i++) {
                this.chkTranslations.SetItemChecked(i, check);
            }
        }

        private void btnAskForTranslations_Click(object sender, EventArgs e) {
            this.txtLogOperations.AppendText(String.Concat("Sending translations ", "\r\n"));
            this.txtLogOperations.AppendText(String.Concat("==============================================", "\r\n"));
            foreach (var containerString in this.chkTranslations.CheckedItems) {
                var collection = _translationMessages.Where(x => x.ContainerName.Equals(containerString.ToString())).Distinct();
                //TODO: chiamata al ws di traduzione
                AskForTranslations(collection);
                this.txtLogOperations.AppendText(String.Concat("Translations sent for: ", containerString.ToString(), "\r\n"));

            }
        }

        private void btnCopyToClipboard_Click(object sender, EventArgs e) {
            List<string[]> toCopy = new List<string[]>();
            foreach (var containerString in this.chkTranslations.CheckedItems) {
                var collection = _translationMessages.Where(x => x.ContainerName.Equals(containerString.ToString())).Distinct();
                toCopy.AddRange(collection.Select(x => new string[] {
                    "",
                    String.Concat("msgctxt ",x.MessageContext,""), 
                    String.Concat("msgid ",x.MessageId,""),
                    String.Concat("msgstr ",x.MessageId," ***"),
                }));
            }
            Clipboard.SetText(String.Concat(toCopy.Select(x => String.Join("\r\n", x))));
        }

        private void AskForTranslations(IEnumerable<TranslationMessage> messages) {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var URI = String.Concat(ConfigurationManager.AppSettings["RemoteTranslationBaseUrl"], "/TranslatorAPI/AddRecords");
            var listRecords = messages.Select(x => new TranslationRecord {
                Id = 0,
                ContainerName = x.ContainerName,
                ContainerType = x.TinyContainerType,
                Context = x.MessageContext.Substring(1, x.MessageContext.Length - 2),
                Message = x.MessageId.Substring(1, x.MessageId.Length - 2),
                Language = x.MessageLanguage
            }).ToList();
            try {
                using (var client = new HttpClient()) {
                    var serializedMessages = serializer.Serialize(listRecords);
                    var content = new StringContent(serializedMessages, Encoding.UTF8, "application/json");
                    var httpResult = client.PostAsync(URI, content);
                    if (!httpResult.Result.IsSuccessStatusCode) {
                        this.txtLogOperations.AppendText(String.Concat("Error: ", httpResult.Result.ReasonPhrase, "\r\n"));
                    }
                    //return responseBody;
                }

            } catch (HttpRequestException e) {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                //return null;
            }


        }


    }
}
