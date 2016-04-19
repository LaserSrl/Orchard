using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Fields.Fields;
using Orchard.MediaLibrary.Fields;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.CommunicationGateway.Services {

    #region Support Classes

    public class ContactExport {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<Hashtable> Fields { get; set; }
        public List<string> Sms { get; set; }
        public List<string> Mail { get; set; }
    }

    #endregion

    public interface IExportContactService : IDependency {

        IEnumerable<ContentItem> GetContactList(SearchVM search);
        ContactExport GetInfoContactExport(ContentItem contenuto);
    }

    public class ExportContactService : IExportContactService {

        private readonly IContentManager _contentManager;
        private readonly ISessionLocator _session;
        private readonly ITaxonomyService _taxonomyService;

        public ExportContactService(IContentManager contentManager, ISessionLocator session, ITaxonomyService taxonomyService) {
            _contentManager = contentManager;
            _session = session;
            _taxonomyService = taxonomyService;
        }

        public IEnumerable<ContentItem> GetContactList(SearchVM search) {
            IEnumerable<ContentItem> contentItems = null;
            List<int> arr = null;

            IContentQuery<ContentItem> contentQuery = _contentManager.Query(VersionOptions.Latest).ForType("CommunicationContact");

            if (!string.IsNullOrEmpty(search.Expression)) {
                switch (search.Field) {
                    case SearchFieldEnum.Name:
                        contentItems = contentQuery.Where<TitlePartRecord>(w => w.Title.Contains(search.Expression)).List();
                        break;

                    case SearchFieldEnum.Mail:
                        string myQueryMail = @"select cir.Id
                                    from Orchard.ContentManagement.Records.ContentItemVersionRecord as civr
                                    join civr.ContentItemRecord as cir
                                    join cir.EmailContactPartRecord as EmailPart
                                    join EmailPart.EmailRecord as EmailRecord
                                    where EmailRecord.Email like '%' + :mail + '%'
                                    order by cir.Id";

                        var elencoIdMail = _session.For(null)
                            .CreateQuery(myQueryMail)
                            .SetParameter("mail", search.Expression)
                            .List<int>();

                        arr = new List<int>(elencoIdMail);
                        contentItems = contentQuery.Where<CommunicationContactPartRecord>(x => arr.Contains(x.Id)).List();
                        break;

                    case SearchFieldEnum.Phone:
                        string myQuerySms = @"select cir.Id
                                    from Orchard.ContentManagement.Records.ContentItemVersionRecord as civr
                                    join civr.ContentItemRecord as cir
                                    join cir.SmsContactPartRecord as SmsPart
                                    join SmsPart.SmsRecord as SmsRecord
                                    where SmsRecord.Sms like '%' + :sms + '%'
                                    order by cir.Id";

                        var elencoIdSms = _session.For(null)
                            .CreateQuery(myQuerySms)
                            .SetParameter("sms", search.Expression)
                            .List<int>();

                        arr = new List<int>(elencoIdSms);
                        contentItems = contentQuery.Where<CommunicationContactPartRecord>(x => arr.Contains(x.Id)).List();
                        break;
                }
            } else {
                contentItems = contentQuery.List();
            };

            return contentItems;
        }

        public ContactExport GetInfoContactExport(ContentItem content) {
            ContactExport contact = new ContactExport();

            // Id
            contact.Id = content.Id;

            // Title
            contact.Title = content.As<TitlePart>().Title;

            // Fields
            List<Hashtable> listaField = new List<Hashtable>();

            bool ExistProfilePart = true;
            dynamic fields = null;

            try {
                fields = ((dynamic)content).ProfilePart.Fields;
            } catch {
                ExistProfilePart = false;
            }

            if (ExistProfilePart) {
                foreach (dynamic cf in fields) {

                    if (cf.FieldDefinition.Name != typeof(ContentPickerField).Name && cf.FieldDefinition.Name != typeof(MediaLibraryPickerField).Name) {
                        string keyField = "ProfilePart." + ((object)cf.DisplayName).ToString();
                        string valueField = "";

                        if (cf.FieldDefinition.Name == typeof(DateTimeField).Name) {
                            if (cf.DateTime != null && !cf.DateTime.Equals(DateTime.MinValue))
                                valueField = ((object)cf.DateTime).ToString();
                        }
                        else if (cf.FieldDefinition.Name == typeof(TaxonomyField).Name) {
                            if (((TaxonomyField)cf).Terms != null) {
                                foreach (TermPart term in ((TaxonomyField)cf).Terms) {
                                    // Più termini selezionati
                                    if (valueField != "")
                                        valueField = "," + valueField;

                                    if (term.FullPath == "/" + term.Id) {
                                        // Taxonomy ad un livello
                                        valueField = valueField + term.Name;
                                    } 
                                    else {
                                        // Taxonomy su più livelli
                                        GetValueCompletoTerms(term, ref valueField);
                                    }
                                }
                            }
                        } else {
                            if (cf.Value != null)
                                valueField = ((object)cf.Value).ToString();
                        }

                        Hashtable hs = new Hashtable();
                        hs.Add(keyField, valueField);
                        listaField.Add(hs);
                    }
                }
            }
            contact.Fields = listaField;

            // Sms
            List<string> listaSms = new List<string>();

            SmsContactPart smspart = content.As<SmsContactPart>();
            foreach (CommunicationSmsRecord sms in smspart.Record.SmsRecord) {
                // Rimuovo il carattere '+' perchè Excel lo considera come una formula
                if (sms.Prefix.StartsWith("+"))
                    sms.Prefix = sms.Prefix.Substring(1);

                listaSms.Add(sms.Prefix + "/" + sms.Sms);
            }
            contact.Sms = listaSms;

            // Mail
            List<string> listaMail = new List<string>();

            EmailContactPart mailpart = content.As<EmailContactPart>();
            foreach (CommunicationEmailRecord mail in mailpart.Record.EmailRecord) {
                listaMail.Add(mail.Email);
            }
            contact.Mail = listaMail;

            return contact;
        }


        private void GetValueCompletoTerms(TermPart term, ref string valueTerm) {

            valueTerm = "/" + term.Name.Replace('/', '\\') + valueTerm;

            string padre = term.FullPath.Split('/')[term.FullPath.Split('/').Length - 2];

            if (padre != "") {
                // Metodo ricorsivo
                TermPart termFather = _taxonomyService.GetTerm(Convert.ToInt32(padre));
                GetValueCompletoTerms(termFather, ref valueTerm);
            } 
            else {
                // Rimuovo primo carattere '/'
                valueTerm = valueTerm.Substring(1);
            }
        }

    }
}