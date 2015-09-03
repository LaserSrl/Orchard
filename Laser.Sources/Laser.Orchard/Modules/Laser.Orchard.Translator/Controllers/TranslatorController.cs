using Laser.Orchard.Translator.Models;
using Laser.Orchard.Translator.Services;
using Laser.Orchard.Translator.ViewModels;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Themes;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.Translator.Controllers
{
    public class TranslatorController : Controller
    {
        private readonly ITranslatorServices _translatorServices;

        public Localizer T { get; set; }

        public TranslatorController(ITranslatorServices translatorServices)
        {
            _translatorServices = translatorServices;
            T = NullLocalizer.Instance;
        }

        [Admin]
        public ActionResult Index(string language, string folderName, string folderType)
        {
            TranslationDetailViewModel translationDetailVM = new TranslationDetailViewModel();
            List<StringSummaryViewModel> messages = new List<StringSummaryViewModel>();

            var localizedMessageRecords = _translatorServices.GetTranslations().Where(m => m.Language == language
                                                                                        && m.ContainerName == folderName
                                                                                        && m.ContainerType == folderType
                                                                                        && !String.IsNullOrWhiteSpace(m.TranslatedMessage));
            foreach (var message in localizedMessageRecords)
            {
                messages.Add(new StringSummaryViewModel { id = message.Id, message = message.Message, localized = true });
            }

            var unlocalizedMessageRecords = _translatorServices.GetTranslations().Where(m => m.Language == language
                                                                                        && m.ContainerName == folderName
                                                                                        && m.ContainerType == folderType
                                                                                        && String.IsNullOrWhiteSpace(m.TranslatedMessage));

            foreach (var message in unlocalizedMessageRecords)
            {
                messages.Add(new StringSummaryViewModel { id = message.Id, message = message.Message, localized = false });
            }

            translationDetailVM.containerName = folderName;
            translationDetailVM.language = language;
            translationDetailVM.messages = messages.OrderBy(m => m.message).ToList();

            return View(translationDetailVM);
        }

        [Themed(false)]
        public ActionResult TranslatorForm(int id)
        {
            TranslationRecord messageRecord = _translatorServices.GetTranslations().Where(m => m.Id == id).FirstOrDefault();

            if (messageRecord != null)
                return View(messageRecord);
            else
                return View(new TranslationRecord());
           
        }

        [HttpPost]
        [ActionName("TranslatorForm")]
        [FormValueRequired("saveTranslation")]
        public ActionResult SaveTranslation(TranslationRecord translation)
        {
            bool success = _translatorServices.TryAddOrUpdateTranslation(translation);

            if (!success)
            {
                ModelState.AddModelError("SaveTranslationError", T("An error occurred while saving the translation. Please reload the page and retry.").ToString());
                ViewBag.SaveSuccess = false;
            }
            else
                ViewBag.SaveSuccess = true;

            return View(translation);
        }

        [HttpPost]
        [ActionName("TranslatorForm")]
        [FormValueRequired("deleteTranslation")]
        public ActionResult DeleteTranslation(int id)
        {
            TranslationRecord messageRecord = _translatorServices.GetTranslations().Where(m => m.Id == id).FirstOrDefault();

            bool success = _translatorServices.DeleteTranslation(messageRecord);

            if (!success)
            {
                ModelState.AddModelError("DeleteTranslationError", T("Unable to delete the translation.").ToString());
                ViewBag.DeleteSuccess = false;
                return View(messageRecord);
            }
            else
            {
                ViewBag.DeleteSuccess = true;
                return View(new TranslationRecord { Id = 0 });
            }
        }
    }
}