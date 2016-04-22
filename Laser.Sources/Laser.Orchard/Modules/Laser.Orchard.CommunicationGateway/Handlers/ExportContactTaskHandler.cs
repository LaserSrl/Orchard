using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.Services;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Tasks.Scheduling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace Laser.Orchard.CommunicationGateway.Handlers {

    public class ExportContactTaskHandler : IScheduledTaskHandler {

        private readonly IExportContactService _exportContactService;
        private readonly ShellSettings _shellSettings;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        private const string TaskType = "Laser.Orchard.CommunicationGateway.ExportContact.Task";

        public ExportContactTaskHandler(IExportContactService exportContactService, ShellSettings shellSettings) {
            _exportContactService = exportContactService;
            _shellSettings = shellSettings;
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context) {
            if (context.Task.TaskType != TaskType) {
                return;
            }

            try {
                // Parametri search
                string[] parametri = (((dynamic)context.Task.ContentItem).ExportTaskParametersPart.Parameters.Value).Split(';');
                string[] parExpression = parametri[0].Split('=');
                string[] parField = parametri[1].Split('=');

                SearchVM search = new SearchVM();
                search.Expression = parExpression[1].ToString();
                if (parField[1].ToString() == "Name") search.Field = ViewModels.SearchFieldEnum.Name;
                if (parField[1].ToString() == "Mail") search.Field = ViewModels.SearchFieldEnum.Mail;
                if (parField[1].ToString() == "Phone") search.Field = ViewModels.SearchFieldEnum.Phone;

                IEnumerable<ContentItem> contentItems = _exportContactService.GetContactList(search);
                List<ContactExport> listaContatti = new List<ContactExport>();

                foreach (ContentItem contenuto in contentItems) {
                    // Contact Master non viene esportato
                    if (!contenuto.As<CommunicationContactPart>().Master) {
                        listaContatti.Add(_exportContactService.GetInfoContactExport(contenuto));
                    }
                }

                // Export CSV
                StringBuilder strBuilder = new StringBuilder();
                string Separator = ";";
                bool isColumnExist = false;

                foreach (ContactExport contatto in listaContatti) {

                    if (!isColumnExist) {
                        #region column
                        strBuilder.Append("Id" + Separator);
                        strBuilder.Append("TitlePart.Title" + Separator);
                        foreach (Hashtable fieldColumn in contatto.Fields) {
                            foreach (DictionaryEntry nameCol in fieldColumn) {
                                strBuilder.Append(nameCol.Key + Separator);
                            }
                        }
                        strBuilder.Append("ContactPart.Sms" + Separator);
                        strBuilder.Append("ContactPart.Email" + Separator);
                        strBuilder.Append(Environment.NewLine);
                        #endregion

                        isColumnExist = true;
                    }

                    #region row
                    strBuilder.Append(contatto.Id.ToString() + Separator);
                    strBuilder.Append(contatto.Title + Separator);
                    foreach (Hashtable fieldRow in contatto.Fields) {
                        foreach (DictionaryEntry valueRow in fieldRow) {
                            strBuilder.Append(valueRow.Value + Separator);
                        }
                    }
                    strBuilder.Append(string.Join(",", contatto.Sms) + Separator);
                    strBuilder.Append(string.Join(",", contatto.Mail) + Separator);
                    strBuilder.Append(Environment.NewLine);
                    #endregion
                }

                // Save File
                string fileName = String.Format("contacts_{0}_{1:yyyyMMddHHmmss}.csv", _shellSettings.Name, DateTime.Now);
                string filePath = HostingEnvironment.MapPath("~/") + @"App_Data\Sites\" + _shellSettings.Name + @"\Export\Contacts\" + fileName;

                if (!File.Exists(filePath)) {

                    // Creo la directory
                    FileInfo fi = new FileInfo(filePath);
                    if (!fi.Directory.Exists) {
                        System.IO.Directory.CreateDirectory(fi.DirectoryName);
                    }

                    // Write File
                    using (StreamWriter sw = new StreamWriter(filePath)) {
                        sw.Write(strBuilder.ToString());
                    }
                } else
                    Logger.Debug(T("File {0} is already exist").Text, fileName);
            } 
            catch (Exception ex) {
                Logger.Error(T("Export Contacts - Error Message: " + ex.Message).Text);
            }
        }

    }
}