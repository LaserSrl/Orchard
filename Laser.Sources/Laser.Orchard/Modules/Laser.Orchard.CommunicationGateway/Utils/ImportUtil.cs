using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using Orchard.ContentManagement;
using Orchard;
using Laser.Orchard.CommunicationGateway.Services;
using Orchard.Core.Title.Models;
using Laser.Orchard.CommunicationGateway.Models;
using Orchard.Data;
using Orchard.Taxonomies.Fields;
using Orchard.ContentPicker.Fields;
using Orchard.MediaLibrary.Fields;
using Orchard.Taxonomies.Services;
using Orchard.Taxonomies.Models;

namespace Laser.Orchard.CommunicationGateway.Utils {
    public class ImportUtil {
        private class PartialRecord {
            public int Id { get; set; }
            public string Name { get; set; }
            public Dictionary<string, string> Profile { get; set; }
            public PartialRecord() {
                Id = 0;
                Name = "";
                Profile = new Dictionary<string, string>();
            }
        }

        public List<string> Errors { get; set; }
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly ICommunicationService _communicationService;
        private readonly IRepository<CommunicationEmailRecord> _repositoryCommunicationEmailRecord;
        private readonly IRepository<CommunicationSmsRecord> _repositoryCommunicationSmsRecord;
        private readonly ITaxonomyService _taxonomyService;
        private const char fieldSeparator = ';';
        private const char smsSeparator = '/';
        private int idxContactId = 0;
        private int idxContactEmail = 0;
        private int idxContactSms = 0;
        private int idxContactName = 0;

        public ImportUtil(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
            _contentManager = _orchardServices.ContentManager;
            _communicationService = _orchardServices.WorkContext.Resolve<ICommunicationService>();
            _repositoryCommunicationEmailRecord = _orchardServices.WorkContext.Resolve<IRepository<CommunicationEmailRecord>>();
            _repositoryCommunicationSmsRecord = _orchardServices.WorkContext.Resolve<IRepository<CommunicationSmsRecord>>();
            _taxonomyService = _orchardServices.WorkContext.Resolve<ITaxonomyService>();
        }

        public void ImportCsv(byte[] buffer) {
            Errors = new List<string>();
            string[] lines = null;
            List<string> intestazione = null;
            List<string> record = null;

            try {
                // converte il buffer in un testo
                string[] lineSeparator = { Environment.NewLine };
                string file = Encoding.ASCII.GetString(buffer);
                lines = file.Split(lineSeparator, StringSplitOptions.None);
            }
            catch (Exception ex) {
                Errors.Add("File format not valid. " + ex.Message);
            }

            if (Errors.Count == 0) {
                if (lines.Count() > 1) {
                    // legge l'intestazione
                    intestazione = GetFields(lines[0]);
                    // normalizza l'intestazione nel caso di ProfilePart
                    string header = null;
                    for (int i = 0; i < intestazione.Count; i++ ) {
                        header = intestazione[i];
                        if (header.StartsWith("ProfilePart.") && header.EndsWith(".Value")) {
                            intestazione[i] = header.Substring(12, header.Length - 18); //12: lunghezza di "ProfilePart.", 18 lunghezza di "ProfilePart." + ".Value"
                        }
                    }

                    // controlla l'esistenza e la posizione dei campi cardine (ID, Name, mail, sms)
                    bool check = CheckForMainFields(intestazione);
                    if (check) {
                        //scandisce le righe del file
                        for (int i = 1; i < lines.Length; i++) {
                            if (string.IsNullOrWhiteSpace(lines[i]) == false) {
                                record = GetFields(lines[i]);
                                if (record.Count > 0) {
                                    ImportRecord(intestazione, record);
                                }
                            }
                        }
                    }
                    else {
                        Errors.Add("At least a main field is missing. Main fields are: ID, TitlePart.Title, ContactPart.Email, ContactPart.Sms");
                    }
                }
            }
        }

        private void ImportRecord(List<string> intestazioni, List<string> campi) {
            string[] elencoMail = null;
            string[] elencoSms = null;
            string campo = null;

            // popola le strutture dati di appoggio
            PartialRecord partialRecord = new PartialRecord();
            for (int i = 0; i < campi.Count; i++) {
                campo = campi[i];
                if (i == idxContactId) {
                    if (string.IsNullOrWhiteSpace(campo) == false) {
                        partialRecord.Id = Convert.ToInt32(campo);
                    }
                }
                else if (i == idxContactName) {
                    partialRecord.Name = campo;
                }
                else if (i == idxContactEmail) {
                    elencoMail = campo.Split(fieldSeparator);
                }
                else if (i == idxContactSms) {
                    elencoSms = campo.Split(fieldSeparator);
                }
                else {
                    partialRecord.Profile.Add(intestazioni[i], campo);
                }
            }

            // gestisce le mail
            foreach (var mail in elencoMail) {
                ImportMail(mail, partialRecord);
            }

            // gestisce gli sms
            string[] smsComponents = null; // prefix and sms
            foreach (var prefixAndSms in elencoSms) {
                smsComponents = prefixAndSms.Split(smsSeparator);
                if (smsComponents.Count() > 1) {
                    ImportSms(smsComponents[0], smsComponents[1], partialRecord);
                }
            }
        }

        private void ImportMail(string mail, PartialRecord partialRecord) {
            var elencoItems = _communicationService.GetContactsFromMail(mail);
            if (elencoItems.Count > 0) {
                foreach (var item in elencoItems) {
                    if (CheckSameContact(item, partialRecord) == true) {
                        UpdateContactInfo(item, partialRecord);
                    }
                    else {
                        var contact = item.As<CommunicationContactPart>();
                        if (contact.Master) {
                            // stacca la mail dal master contact, crea un nuovo contatto e glie la associa
                            var a1 = item.As<EmailContactPart>().EmailRecord.Where(x => x.Email == mail).First();
                            var ci = CreateContact(partialRecord);
                            a1.EmailContactPartRecord_Id = ci.Id;
                        }
                        else {
                            Errors.Add(string.Format("Mail {0} already assigned to contact id {1}.", mail, item.Id));
                        }
                    }
                }
            }
            else {
                CreateContactForMail(partialRecord, mail);
            }
        }

        private void ImportSms(string prefix, string sms, PartialRecord partialRecord) {
            var elencoItems = _communicationService.GetContactsFromSms(prefix, sms);
            if (elencoItems.Count > 0) {
                foreach (var item in elencoItems) {
                    if (CheckSameContact(item, partialRecord) == true) {
                        UpdateContactInfo(item, partialRecord);
                    }
                    else {
                        var contact = item.As<CommunicationContactPart>();
                        if (contact.Master) {
                            // stacca l'sms dal master contact, crea un nuovo contatto e glie lo associa
                            var a1 = item.As<SmsContactPart>().SmsRecord.Where(x => x.Prefix == prefix && x.Sms == sms).First();
                            var ci = CreateContact(partialRecord);
                            a1.SmsContactPartRecord_Id = ci.Id;
                        }
                        else {
                            Errors.Add(string.Format("Sms phone number {0}/{1} already assigned to contact id {2}.", prefix, sms, item.Id));
                        }
                    }
                }
            }
            else {
                CreateContactForSms(partialRecord, prefix, sms);
            }
        }

        /// <summary>
        /// Aggiorna le altre info del contatto: nome e profile part.
        /// </summary>
        private void UpdateContactInfo(ContentItem ci, PartialRecord partialRecord) {
            ci.As<TitlePart>().Title = partialRecord.Name;
            ContentPart profile = ((dynamic)ci).ProfilePart;
            foreach (var prop in partialRecord.Profile) {
                var cf = profile.Fields.Where(x => x.Name == prop.Key).FirstOrDefault();
                if (cf != null) {
                    if (cf.GetType() == typeof(DateTime)) {
                        ((dynamic)cf).DateTime = prop.Value;
                    }
                    else if (cf.GetType() == typeof(TaxonomyField)) {
                        var taxoSettings = ((TaxonomyField)cf).PartFieldDefinition.Settings;
                        string taxoName = taxoSettings.Where(x => x.Key == "TaxonomyFieldSettings.Taxonomy").Select(x => x.Value).FirstOrDefault();
                        List<TermPart> termlist = new List<TermPart>();
                        TermPart term = null;
                        foreach (string locValue in prop.Value.Split(fieldSeparator)) {
                            term = _taxonomyService.GetTermByPath((taxoName + "/" + locValue).ToLower());
                            if(term != null){
                                termlist.Add(term);
                            }
                        }
                        _taxonomyService.UpdateTerms(ci, termlist, cf.Name);
                    }
                    else if ((cf.GetType() == typeof(MediaLibraryPickerField)) || (cf.GetType() == typeof(ContentPickerField))) {
                        Errors.Add(string.Format("Property {0} of type {1} cannot be updated by CSV import.", prop.Key, cf.GetType().Name));
                    }
                    else {
                        ((dynamic)cf).Value = prop.Value;
                    }
                }
            }
        }

        private void CreateContactForMail(PartialRecord partialRecord, string mail) {
            var ci = CreateContact(partialRecord);
            CommunicationEmailRecord cer = new CommunicationEmailRecord();
            cer.Email = mail;
            cer.DataInserimento = DateTime.Now;
            cer.DataModifica = DateTime.Now;
            cer.Produzione = true;
            cer.Validated = true;
            cer.EmailContactPartRecord_Id = ci.Id;
            _repositoryCommunicationEmailRecord.Create(cer);
        }

        private void CreateContactForSms(PartialRecord partialRecord, string prefix, string sms) {
            var ci = CreateContact(partialRecord);
            CommunicationSmsRecord csr = new CommunicationSmsRecord();
            csr.Prefix = prefix;
            csr.Sms = sms;
            csr.DataInserimento = DateTime.Now;
            csr.DataModifica = DateTime.Now;
            csr.Produzione = true;
            csr.Validated = true;
            csr.SmsContactPartRecord_Id = ci.Id;
            _repositoryCommunicationSmsRecord.Create(csr);
        }

        private ContentItem CreateContact(PartialRecord partialRecord) {
            ContentItem ci = _contentManager.Create("CommunicationContact");
            ci.As<TitlePart>().Title = partialRecord.Name;
            UpdateContactInfo(ci, partialRecord);
            return ci;
        }

        /// <summary>
        /// Controlla se il content item appartiene allo stesso contatto: verifica prima per id e poi per nome.
        /// </summary>
        /// <param name="ci"></param>
        /// <param name="partialRecord"></param>
        /// <returns></returns>
        private bool CheckSameContact(ContentItem ci, PartialRecord partialRecord) {
            bool result = false;
            if (partialRecord.Id != 0) {
                if (partialRecord.Id == ci.Id) {
                    result = true;
                }
            }
            else {
                if (partialRecord.Name == ci.As<TitlePart>().Title) {
                    result = true;
                }
            }
            return result;
        }

        private bool CheckForMainFields(List<string> intestazioni) {
            bool result = true;
            idxContactId = intestazioni.IndexOf("ID");
            if (idxContactId < 0) {
                result = false;
            }
            idxContactName = intestazioni.IndexOf("TitlePart.Title");
            if (idxContactName < 0) {
                result = false;
            }
            idxContactEmail = intestazioni.IndexOf("ContactPart.Email");
            if (idxContactEmail < 0) {
                result = false;
            }
            idxContactSms = intestazioni.IndexOf("ContactPart.Sms");
            if (idxContactSms < 0) {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Suddivide una string adi testo in campi separati da semicolon (;).
        /// Se in un campo sono presenti i caratteri semicolon o doppio apice, esso sarà delimitato da doppi apici (es. "Libro/Giallo;Film/Comico").
        /// Se in un campo è prensete un doppio apice, questo viene raddoppiato (es. "John disse: ""Geronimo!"" e si tuffò.").
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private List<string> GetFields(string line) {
            List<string> result = new List<string>();
            const char stringDelimiter = '"';
            const char doppiApici = '\0';
            bool insideAString = false;
            StringBuilder sb = new StringBuilder();

            // sostituisce eventuali coppie di doppi apici consecutivi con un carattere di comodo
            string row = line.Replace("\"\"", "" + doppiApici);
            for (int i = 0; i < row.Length; i++) {
                switch (row[i]) {
                    case  fieldSeparator:
                        if (insideAString) {
                            sb.Append(row[i]);
                        }
                        else {
                            result.Add(sb.ToString());
                            sb.Clear();
                        }
                        break;
                    case stringDelimiter:
                        if (insideAString) {
                            // fine della stringa
                            insideAString = false;
                        }
                        else {
                            // inizio della stringa
                            insideAString = true;
                        }
                        break;
                    case doppiApici:
                        sb.Append("\"");
                        break;
                    default:
                        sb.Append(row[i]);
                        break;
                }
            }
            // aggiunge l'ultimo campo
            result.Add(sb.ToString());
            return result;
        }
    }
}