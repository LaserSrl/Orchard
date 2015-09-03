using Laser.Orchard.StartupConfig.Services;
using Laser.Orchard.Translator.Models;
using Laser.Orchard.Translator.Services;
using Laser.Orchard.Translator.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Laser.Orchard.Translator.Controllers
{
    [Admin]
    public class TranslatorTreeController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly ITranslatorServices _translatorServices;
        private readonly IUtilsServices _utilsServices;

        public Localizer T { get; set; }

        public TranslatorTreeController(IOrchardServices orchardServices, ITranslatorServices translatorServices, IUtilsServices utilsServices)
        {
            _orchardServices = orchardServices;
            _translatorServices = translatorServices;
            _utilsServices = utilsServices;
            T = NullLocalizer.Instance;
        }

        public ActionResult Index()
        {
            TranslatorViewModel treeVM = new TranslatorViewModel();

            treeVM.CultureList = _translatorServices.GetCultureList();
            treeVM.ShowAdvancedOperations = _orchardServices.Authorizer.Authorize(Permissions.TranslatorPermission.ManageTranslations);

            return View(treeVM);
        }

        public JsonResult CreateJsonForTree(string language)
        {
            List<TranslationTreeNodeViewModel> tree = new List<TranslationTreeNodeViewModel>();

            if (!String.IsNullOrWhiteSpace(language))
            {
                tree.Add(new TranslationTreeNodeViewModel
                {
                    text = T("Modules").ToString(),
                    children = CreateListForTree(language, Path.Combine(_utilsServices.TenantPath, "Modules"), ElementToTranslate.Module),
                    data = new Dictionary<string, string>() { { "type", "M" } }
                });

                tree.Add(new TranslationTreeNodeViewModel
                {
                    text = T("Themes").ToString(),
                    children = CreateListForTree(language, Path.Combine(_utilsServices.TenantPath, "Themes"), ElementToTranslate.Theme),
                    data = new Dictionary<string, string>() { { "type", "T" } }
                });
            }

            return Json(tree, JsonRequestBehavior.AllowGet);
        }

        private List<TranslationTreeNodeViewModel> CreateListForTree(string language, string parentFolder, ElementToTranslate elementType)
        {
            var translatorSettings = _orchardServices.WorkContext.CurrentSite.As<TranslatorSettingsPart>();

            List<string> elementsToTranslate = new List<string>();
            if (elementType == ElementToTranslate.Module)
                elementsToTranslate = translatorSettings.ModulesToTranslate.Replace(" ", "").Split(',').ToList();
            else if (elementType == ElementToTranslate.Theme)
                elementsToTranslate = translatorSettings.ThemesToTranslate.Replace(" ", "").Split(',').ToList();

            var list = new List<string>(Directory.GetDirectories(parentFolder));
            list = list.Select(dir => dir.Remove(0, dir.LastIndexOf(Path.DirectorySeparatorChar) + 1)).ToList();
            list = list.Where(dir => elementsToTranslate.Any(x => x == dir)).ToList();
            list.Sort((x, y) => string.Compare(x, y));

            List<TranslationTreeNodeViewModel> treeList = new List<TranslationTreeNodeViewModel>();
            foreach (var item in list)
            {
                Dictionary<string, string> additionalData = new Dictionary<string, string>();

                additionalData.Add("percent", GetCompletionPercent(language, Path.Combine(parentFolder, item)).ToString() + "%");

                if (elementType == ElementToTranslate.Module)
                    additionalData.Add("type", "M");
                else if (elementType == ElementToTranslate.Theme)
                    additionalData.Add("type", "T");

                treeList.Add(new TranslationTreeNodeViewModel { text = item, data = additionalData });
            }

            return treeList;
        }

        private int GetCompletionPercent(string language, string folder)
        {
            if (!Directory.Exists(Path.Combine(folder, "App_Data", "Localization", language)))
                return 0;
            else
                return 100;
        }
    }
}