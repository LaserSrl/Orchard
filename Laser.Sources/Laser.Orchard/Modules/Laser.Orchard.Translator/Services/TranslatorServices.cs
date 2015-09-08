using Laser.Orchard.Translator.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Laser.Orchard.Translator.ViewModels;

namespace Laser.Orchard.Translator.Services {
    public interface ITranslatorServices : IDependency {
        IEnumerable<string> GetCultureList();
        IQueryable<TranslationRecord> GetTranslations();
        IList<string> GetSuggestedTranslations(string message, string language);
        bool TryAddOrUpdateTranslation(TranslationRecord translation);
        void EnableFolderTranslation(string folderName, ElementToTranslate folderType);
        bool DeleteTranslation(TranslationRecord record);
        void DeleteAllTranslations();
    }

    public class TranslatorServices : ITranslatorServices {
        private readonly ICultureManager _cultureManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<TranslationRecord> _translationRecordRepository;

        public Localizer T { get; set; }

        public TranslatorServices(ICultureManager cultureManager, IOrchardServices orchardServices, IRepository<TranslationRecord> translationRecordRepository) {
            _cultureManager = cultureManager;
            _orchardServices = orchardServices;
            _translationRecordRepository = translationRecordRepository;
            T = NullLocalizer.Instance;
        }

        public IEnumerable<string> GetCultureList() {
            //Lista completa da usare in produzione
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(c => c.Name);

            //Lista ridotta a scopo di test
            //return _cultureManager.ListCultures();
        }

        public IQueryable<TranslationRecord> GetTranslations() {
            return _translationRecordRepository.Table;
        }

        public bool TryAddOrUpdateTranslation(TranslationRecord translation) {
            try {
                AddOrUpdateTranslation(translation);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        private void AddOrUpdateTranslation(TranslationRecord translation) {
            List<TranslationRecord> existingTranslations = new List<TranslationRecord>();
            bool searchById = translation.Id != 0;

            if (searchById) {
                existingTranslations = GetTranslations().Where(t => t.Id == translation.Id).ToList();
            } else {
                existingTranslations = GetTranslations().Where(t => t.Language == translation.Language
                                                                    && t.ContainerName == translation.ContainerName
                                                                    && t.ContainerType == translation.ContainerType
                                                                    && t.Context.ToString() == translation.Context
                                                                    && t.Message.ToString() == translation.Message).ToList();
            }

            if (existingTranslations.Any()) {
                TranslationRecord existingTranslation = existingTranslations.FirstOrDefault();

                existingTranslation.Context = translation.Context;
                existingTranslation.TranslatedMessage = translation.TranslatedMessage;

                _translationRecordRepository.Update(existingTranslation);
                _translationRecordRepository.Flush();
            } else {
                if (searchById) {
                    throw new Exception(T("The required translation does not exists.").ToString());
                } else {
                    _translationRecordRepository.Create(translation);
                    _translationRecordRepository.Flush();
                }
            }
        }

        public bool DeleteTranslation(TranslationRecord record) {
            try {
                _translationRecordRepository.Delete(record);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public void DeleteAllTranslations() {
            List<TranslationRecord> translations = GetTranslations().ToList();

            foreach (TranslationRecord translation in translations) {
                if (!String.IsNullOrEmpty(translation.Language))
                    _translationRecordRepository.Delete(translation);
            }
        }

        public void EnableFolderTranslation(string folderName, ElementToTranslate folderType)
        {
            var translatorSettings = _orchardServices.WorkContext.CurrentSite.As<TranslatorSettingsPart>();

            List<string> enabledFolders = new List<string>();
            if (folderType == ElementToTranslate.Module)
                enabledFolders = translatorSettings.ModulesToTranslate.Replace(" ", "").Split(',').ToList();
            else if (folderType == ElementToTranslate.Theme)
                enabledFolders = translatorSettings.ThemesToTranslate.Replace(" ", "").Split(',').ToList();

            if (!enabledFolders.Contains(folderName))
            {
                if (folderType == ElementToTranslate.Module)
                {
                    if (!String.IsNullOrWhiteSpace(translatorSettings.ModulesToTranslate))
                        translatorSettings.ModulesToTranslate += ",";

                    translatorSettings.ModulesToTranslate += folderName;
                }
                else if (folderType == ElementToTranslate.Theme)
                {
                    if (!String.IsNullOrWhiteSpace(translatorSettings.ThemesToTranslate))
                        translatorSettings.ThemesToTranslate += ",";

                    translatorSettings.ThemesToTranslate += folderName;
                }
            }
        }

        public IList<string> GetSuggestedTranslations(string message, string language) {
            return GetTranslations().Where(w => w.Message.ToString() == message && w.Language == language && w.TranslatedMessage.ToString() != "").Take(5)
                .Select(x => x.TranslatedMessage).AsParallel().Distinct().ToList();
        }
    }
}