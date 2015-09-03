using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.Translator.Models;
using Laser.Orchard.Translator.Services;
using Orchard;
using Orchard.ContentManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Laser.Orchard.Translator.Controllers
{
    public class ImportExportController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly ITranslatorServices _translatorServices;
        private readonly IUtilsServices _utilsServices;

        private const string pattern = "msgctxt\\s+\"([^\\s]+)\"\\s+msgid\\s+\"([^\"]*)\"\\s+msgstr\\s+\"([^\"]*)\"";

        public ImportExportController(IOrchardServices orchardServices, ITranslatorServices translatorServices, IUtilsServices utilsServices)
        {
            _orchardServices = orchardServices;
            _translatorServices = translatorServices;
            _utilsServices = utilsServices;
        }

        public ActionResult ImportTranslations()
        {
            _translatorServices.DeleteAllTranslations();

            var translatorSettings = _orchardServices.WorkContext.CurrentSite.As<TranslatorSettingsPart>();

            List<string> modulesToTranslate = translatorSettings.ModulesToTranslate.Replace(" ", "").Split(',').ToList();
            List<string> themesToTranslate = translatorSettings.ThemesToTranslate.Replace(" ", "").Split(',').ToList();

            if (modulesToTranslate.Any())
                ImportFromPO(modulesToTranslate, ElementToTranslate.Module);

            if (themesToTranslate.Any())
                ImportFromPO(themesToTranslate, ElementToTranslate.Theme);

            string returnUrl = this.Request.UrlReferrer.AbsolutePath;
            return Redirect(returnUrl);
        }

        private void ImportFromPO(List<string> foldersToImport, ElementToTranslate type)
        {
            string parentFolder = "";
            string fileName = "";

            if (type == ElementToTranslate.Module)
            {
                parentFolder = "Modules";
                fileName = "orchard.module.po";
            }
            else if (type == ElementToTranslate.Theme)
            {
                parentFolder = "Themes";
                fileName = "orchard.theme.po";
            }
            else
                return;

            foreach (var folder in foldersToImport)
            {
                var path = Path.Combine(_utilsServices.TenantPath, parentFolder, folder, "App_Data", "Localization");
                if (Directory.Exists(path))
                {
                    var languages = Directory.GetDirectories(path).Select(d => new DirectoryInfo(d).Name);
                    foreach (var language in languages)
                    {
                        var filePath = Path.Combine(path, language, fileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            string fileContent = System.IO.File.ReadAllText(filePath);
                            foreach (Match match in Regex.Matches(fileContent, pattern, RegexOptions.IgnoreCase))
                            {
                                TranslationRecord translation = new TranslationRecord();

                                translation.ContainerName = folder;

                                if (type == ElementToTranslate.Module)
                                    translation.ContainerType = "M";
                                else if (type == ElementToTranslate.Theme)
                                    translation.ContainerType = "T";

                                translation.Context = match.Groups[1].Value;
                                translation.Message = match.Groups[2].Value;
                                translation.TranslatedMessage = match.Groups[3].Value;
                                translation.Language = language;

                                _translatorServices.TryAddOrUpdateTranslation(translation);
                            }
                        }
                    }
                }
            }
        }
    }
}