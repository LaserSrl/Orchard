using Laser.Orchard.Translator.Models;
using Laser.Orchard.Translator.Services;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Laser.Orchard.Translator.Controllers
{
    public class TranslatorAPIController : ApiController
    {
        private readonly ITransactionManager _transactionManager;
        private readonly ITranslatorServices _translatorServices;

        public TranslatorAPIController(ITransactionManager transactionManager, ITranslatorServices translatorServices)
        {
            _transactionManager = transactionManager;
            _translatorServices = translatorServices;
        }

        [System.Web.Mvc.HttpPost]
        public bool AddRecords([FromBody] List<TranslationRecord> records)
        {
            try
            {
                if (records == null) return false;

                if (records.Where(r => String.IsNullOrWhiteSpace(r.Message)
                                    || String.IsNullOrWhiteSpace(r.Language)
                                    || String.IsNullOrWhiteSpace(r.ContainerName)
                                    || String.IsNullOrWhiteSpace(r.ContainerType)).Any()) return false;

                foreach (var record in records)
                {
                    var alreadyExistingRecords = _translatorServices.GetTranslations().Where(r => r.ContainerName == record.ContainerName
                                                                                              && r.ContainerType == record.ContainerType
                                                                                              && r.Context.ToString() == record.Context
                                                                                              && r.Message.ToString() == record.Message
                                                                                              && r.Language == record.Language);

                    if (!alreadyExistingRecords.Any())
                    {
                        bool success = _translatorServices.TryAddOrUpdateTranslation(record);
                        if (!success)
                        {
                            _transactionManager.Cancel();
                            return false;
                        }
                    }

                    var folderList = records.GroupBy(g => new { g.ContainerName, g.ContainerType })
                                            .Select(g => new { g.Key.ContainerName, g.Key.ContainerType });

                    foreach (var folder in folderList)
                    {
                        if (folder.ContainerType == "M")
                            _translatorServices.EnableFolderTranslation(folder.ContainerName, ElementToTranslate.Module);
                        else if (folder.ContainerType == "T")
                            _translatorServices.EnableFolderTranslation(folder.ContainerName, ElementToTranslate.Theme);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                _transactionManager.Cancel();
                return false;
            }
        }
    }
}