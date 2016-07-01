using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Laser.Orchard.CommunicationGateway.Utils;
using Laser.Orchard.ShortLinks.Services;
using Laser.Orchard.StartupConfig.Models;
using Laser.Orchard.StartupConfig.Services;
using NHibernate.Transform;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Fields.Fields;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.MediaLibrary.Fields;
using Orchard.Modules.Services;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.Security;
using Orchard.Tags.Models;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Orchard.Taxonomies.Services;
using Orchard.Core.Common.Models;
using Orchard.Localization.Services;

namespace Laser.Orchard.CommunicationGateway.Services {

    public interface ICommunicationService : IDependency {

        bool AdvertisingIsAvailable(Int32 id);
        string GetCampaignLink(string CampaignSource, ContentPart part);
        bool CampaignLinkExist(ContentPart part);
        void UserToContact(IUser UserContent);
        CommunicationContactPart GetContactFromUser(int iduser);
        List<ContentItem> GetContactsFromMail(string mail);

        List<ContentItem> GetContactsFromSms(string prefix, string sms);

        ContentItem GetContactFromName(string name);

        ContentItem GetContactFromId(int id);

        void Synchronize();
        void UnboundFromUser(UserPart userPart);
        CommunicationContactPart EnsureMasterContact();
        CommunicationContactPart TryEnsureContact(int userId);
    }

    public class CommunicationService : ICommunicationService {
        private readonly IOrchardServices _orchardServices;
        private readonly IShortLinksService _shortLinksService;
        private readonly IContentExtensionsServices _contentExtensionsServices;
        private readonly IModuleService _moduleService;
        private readonly INotifier _notifier;
        private readonly ISessionLocator _session;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ICultureManager _cultureManager;
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private readonly IRepository<CommunicationEmailRecord> _repositoryCommunicationEmailRecord;
        private readonly IRepository<CommunicationSmsRecord> _repositoryCommunicationSmsRecord;

        public CommunicationService(ITaxonomyService taxonomyService, IRepository<CommunicationEmailRecord> repositoryCommunicationEmailRecord, INotifier notifier, IModuleService moduleService, IOrchardServices orchardServices, IShortLinksService shortLinksService, IContentExtensionsServices contentExtensionsServices, ISessionLocator session, ICultureManager cultureManager, IRepository<CommunicationSmsRecord> repositoryCommunicationSmsRecord) {
            _orchardServices = orchardServices;
            _shortLinksService = shortLinksService;
            _contentExtensionsServices = contentExtensionsServices;
            _moduleService = moduleService;
            _notifier = notifier;
            _repositoryCommunicationEmailRecord = repositoryCommunicationEmailRecord;
            _repositoryCommunicationSmsRecord = repositoryCommunicationSmsRecord;
            _session = session;
            _taxonomyService = taxonomyService;
            _cultureManager = cultureManager;

            T = NullLocalizer.Instance;
        }

        public bool AdvertisingIsAvailable(Int32 id) {
            ContentItem ci = _orchardServices.ContentManager.Get(id, VersionOptions.DraftRequired);
            if (ci.ContentType != "CommunicationAdvertising") {
                return false;
            }
            if (ci.As<CommunicationAdvertisingPart>().CampaignId > 0) { // è legato ad una campagna
                ContentItem campaign = _orchardServices.ContentManager.Get(ci.As<CommunicationAdvertisingPart>().CampaignId, VersionOptions.Latest);
                DateTime from = ((DateTimeField)(((dynamic)campaign).CommunicationCampaignPart.FromDate)).DateTime;
                DateTime to = ((DateTimeField)(((dynamic)campaign).CommunicationCampaignPart.ToDate)).DateTime;
                if (from > DateTime.UtcNow || (to < DateTime.UtcNow && to != DateTime.MinValue))
                    return false;
            }
            return true;
        }

        public CommunicationContactPart EnsureMasterContact() {
            if (_orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(y => y.Master).Count() == 0) {
                var Contact = _orchardServices.ContentManager.Create("CommunicationContact");
                Contact.As<TitlePart>().Title = "Master Contact";
                Contact.As<CommunicationContactPart>().Master = true;
                _notifier.Add(NotifyType.Information, T("Master Contact Created"));
            }
            CommunicationContactPart master = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(y => y.Master).List().FirstOrDefault();
            return master;
        }

        public CommunicationContactPart TryEnsureContact(int userId) {
            CommunicationContactPart contact = null;
            var user = _orchardServices.ContentManager.Get(userId).As<IUser>();
            if (user != null) {
                UserToContact(user);
                contact = GetContactFromUser(userId);
            }
            return contact;
        }

        public void Synchronize() {

            #region Creazione di un Contact Master a cui agganciare tutte le parti che non hanno una profilazione

            //if (_orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(y => y.Master).Count() == 0) {
            //    var Contact = _orchardServices.ContentManager.New("CommunicationContact");
            //    _orchardServices.ContentManager.Create(Contact);
            //    Contact.As<TitlePart>().Title = "Master Contact";
            //    Contact.As<CommunicationContactPart>().Master = true;
            //    _notifier.Add(NotifyType.Information, T("Master Contact Created"));
            //}
            EnsureMasterContact();

            #endregion Creazione di un Contact Master a cui agganciare tutte le parti che non hanno una profilazione

            #region Import dei profili degli utenti

            List<Int32> contactsUsers = new List<int>();
            var users = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().List();
            if (_orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Count() > 0) {
                contactsUsers = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().List().Select(y => y.As<CommunicationContactPart>().UserIdentifier).ToList();
            }
            // var userWithNoConcat = users.Where(x => !contactsUsers.Contains(x.Id));
            //  foreach (var user in userWithNoConcat) {
            foreach (var user in users) {
                UserToContact(user);
            }
            _notifier.Add(NotifyType.Information, T("Syncronized {0} user's profiles", users.Count().ToString()));

            #endregion Import dei profili degli utenti


            #region Ricreo collegamento con parte mobile preesistente

            var features = _moduleService.GetAvailableFeatures().ToDictionary(m => m.Descriptor.Id, m => m);
            if (features.ContainsKey("Laser.Orchard.MobileCommunicationImport")) {
                if (features.ContainsKey("Laser.Orchard.Mobile") && features["Laser.Orchard.Mobile"].IsEnabled) {
                    if (features["Laser.Orchard.MobileCommunicationImport"].IsEnabled) {
                        _moduleService.DisableFeatures(new string[] { "Laser.Orchard.MobileCommunicationImport" });
                    }
                    _moduleService.EnableFeatures(new string[] { "Laser.Orchard.MobileCommunicationImport" }, true);
                }
            }

            #endregion Ricreo collegamento con parte mobile preesistente

            #region Ricreo collegamento con parte sms preesistente

            if (features.ContainsKey("Laser.Orchard.SmsCommunicationImport")) {
                if (features.ContainsKey("Laser.Orchard.Sms") && features["Laser.Orchard.Sms"].IsEnabled) {
                    if (features["Laser.Orchard.SmsCommunicationImport"].IsEnabled) {
                        _moduleService.DisableFeatures(new string[] { "Laser.Orchard.SmsCommunicationImport" });
                    }
                    _moduleService.EnableFeatures(new string[] { "Laser.Orchard.SmsCommunicationImport" }, true);
                }
            }

            #endregion Ricreo collegamento con parte sms preesistente

            // aggiungo 200.000 record
            //for (int i = 0; i < 100000; i++) {
            //    var email = Guid.NewGuid() + "@fake.it";
            //    ContentItem Contact;
            //    Contact = _orchardServices.ContentManager.New("CommunicationContact");
            //    _orchardServices.ContentManager.Create(Contact);
            //    CommunicationEmailRecord newrec = new CommunicationEmailRecord();
            //    newrec.Email = email;
            //    newrec.CommunicationContactPartRecord_Id = Contact.Id;
            //    _repositoryCommunicationEmailRecord.Create(newrec);
            //    _repositoryCommunicationEmailRecord.Flush();
            //    Contact.As<TitlePart>().Title = email + " progr:" + i.ToString();
            //    _orchardServices.TransactionManager.RequireNew();
            //}
        }

        public CommunicationContactPart GetContactFromUser(int iduser) {
            return _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(x => x.UserPartRecord_Id == iduser).List().FirstOrDefault();
        }

        public List<ContentItem> GetContactsFromMail(string mail) {
            string hql = @"SELECT cir.Id as Id
                FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr
                join civr.ContentItemRecord as cir
                join cir.EmailContactPartRecord as EmailPart
                join EmailPart.EmailRecord as EmailRecord 
                WHERE civr.Published=1 AND EmailRecord.Validated AND EmailRecord.Email = :mail";

            var elencoId = _session.For(null)
                .CreateQuery(hql)
                .SetParameter("mail", mail)
                .List();
            var contentQuery = _orchardServices.ContentManager.Query(VersionOptions.Latest)
                .ForType("CommunicationContact")
                .Where<CommunicationContactPartRecord>(x => elencoId.Contains(x.Id)).List();
            return contentQuery.ToList();
        }

        public List<ContentItem> GetContactsFromSms(string prefix, string sms) {
            string hql = @"SELECT cir.Id as Id
                FROM Orchard.ContentManagement.Records.ContentItemVersionRecord as civr
                join civr.ContentItemRecord as cir
                join cir.SmsContactPartRecord as SmsPart
                join SmsPart.SmsRecord as SmsRecord 
                WHERE civr.Published=1 AND SmsRecord.Prefix = :prefix AND SmsRecord.Sms = :sms";
            var elencoId = _session.For(null)
                .CreateQuery(hql)
                .SetParameter("prefix", prefix)
                .SetParameter("sms", sms)
                .List();
            var contentQuery = _orchardServices.ContentManager.Query(VersionOptions.Latest)
                .ForType("CommunicationContact")
                .Where<CommunicationContactPartRecord>(x => elencoId.Contains(x.Id)).List();
            return contentQuery.ToList();
        }

        public ContentItem GetContactFromName(string name) {
            var query = _orchardServices.ContentManager.Query(new string[] { "CommunicationContact" })
                .Where<TitlePartRecord>(x => x.Title == name);
            return query.List().FirstOrDefault();
        }

        public ContentItem GetContactFromId(int id) {
            var query = _orchardServices.ContentManager.Query(new string[] { "CommunicationContact" })
                .Where<CommunicationContactPartRecord>(x => x.Id == id);
            return query.List().FirstOrDefault();
        }

        /// <summary>
        ///La parte sarebbe CommunicationAdvertisingPart ma non l'ho definita quindi passo una cosa generica (ContentPart)
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public string GetCampaignLink(string CampaignSource, ContentPart generalpart) {
            //string CampaignSource = "email";
            //Logger.Error("GetCampaignLink: 01");
            string shortlink = "";
            ContentPart part = (ContentPart)(((dynamic)generalpart).ContentItem.CommunicationAdvertisingPart);
            //Logger.Error("GetCampaignLink: 01.01");
            string CampaignTerm = "";
            var tagPart = part.ContentItem.As<TagsPart>();
            if (tagPart != null) {
                CampaignTerm = string.Join("+", tagPart.CurrentTags.ToArray()).ToLower();
            }
            //Logger.Error("GetCampaignLink: 01.02");
            string CampaignMedium = CampaignSource;
            string CampaignContent = part.ContentItem.As<TitlePart>().Title.ToLower();
            //Logger.Error("GetCampaignLink: 01.03");
            string CampaignName = "Flash";
            //Logger.Error("GetCampaignLink: 02");
            try {
                int idCampagna = ((int)((dynamic)part).CampaignId);
                CampaignName = _orchardServices.ContentManager.Get(idCampagna).As<TitlePart>().Title;
            } catch (Exception ex) {
                // cuomunicato non legato a campagna
            }
            //Logger.Error("GetCampaignLink: 03");
            string link = "";
            if (!string.IsNullOrEmpty(((dynamic)part).UrlLinked.Value)) {
                //Logger.Error("GetCampaignLink: 03.01");
                link = (string)(((dynamic)part).UrlLinked.Value);
            } else {
                //Logger.Error("GetCampaignLink: 03.02");
                var pickerField = ((dynamic)part).ContentLinked as ContentPickerField;
                //Logger.Error("GetCampaignLink: 03.03");
                if (pickerField != null && pickerField.ContentItems != null) {
                    //Logger.Error("GetCampaignLink: 03.04");
                    var firstItem = pickerField.ContentItems.FirstOrDefault();
                    //Logger.Error("GetCampaignLink: 03.05");
                    if (firstItem != null) {
                        //Logger.Error("GetCampaignLink: 03.06");
                        var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                        //Logger.Error("GetCampaignLink: 03.07");
                        link = urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(firstItem));
                        //Logger.Error("GetCampaignLink: 03.08");
                    } else {
                        return "";
                    }
                } else {
                    return "";
                }
            }
            //Logger.Error("GetCampaignLink: 04");

            string linkelaborated = ElaborateLink(link, CampaignSource, CampaignMedium, CampaignTerm, CampaignContent, CampaignName);
            //Logger.Error("GetCampaignLink: 04.01");
            if (!string.IsNullOrEmpty(linkelaborated)) {
                //Logger.Error("GetCampaignLink: 04.02");
                shortlink = _shortLinksService.GetShortLink(linkelaborated);
                //Logger.Error("GetCampaignLink: 04.03");
                if (string.IsNullOrEmpty(shortlink)) {
                    throw new Exception("Url Creation Failed");
                }
            }
            //Logger.Error("GetCampaignLink: 05");
            return shortlink;
        }

        public bool CampaignLinkExist(ContentPart generalpart) {
            bool linkExist = false;

            ContentPart part = (ContentPart)(((dynamic)generalpart).ContentItem.CommunicationAdvertisingPart);

            if (!string.IsNullOrEmpty(((dynamic)part).UrlLinked.Value)) {
                linkExist = true;
            } else {
                var pickerField = ((dynamic)part).ContentLinked as ContentPickerField;

                if (pickerField != null) {
                    try {
                        var firstItem = pickerField.ContentItems.FirstOrDefault();
                        if (firstItem != null) {
                            linkExist = true;
                        }
                    } catch { }
                }
            }

            return linkExist;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="CampaignSource"></param>
        /// <param name="CampaignMedium"></param>
        /// <param name="CampaignTerm">Tassonomia legata al contenuto cliccato</param>
        /// <param name="CampaignContent">Used for A/B testing</param>
        /// <param name="CampaignName"></param>
        /// <returns></returns>
        private string ElaborateLink(string link, string CampaignSource = "newsletter", string CampaignMedium = "email", string CampaignTerm = "", string CampaignContent = "", string CampaignName = "") {
            var uriBuilder = new UriBuilder(link);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["utm_source"] = "Krake";
            //query["referrer"] = string.Format("utm_source%3D{0}", CampaignSource);
            query["utm_medium"] = CampaignMedium;
            query["utm_term"] = CampaignTerm;
            query["utm_content"] = CampaignContent;
            query["utm_campaign"] = CampaignName;
            uriBuilder.Query = query.ToString();
            link = uriBuilder.ToString();
            return link;
        }

        public void UnboundFromUser(UserPart userPart) {
            var contacts = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(x => x.UserPartRecord_Id == userPart.Id).List();
            foreach(var contact in contacts) {
                contact.UserIdentifier = 0;
            }
        }

        public void UserToContact(IUser UserContent) {
            if (UserContent.Id == 0) {
                // non crea il contatto se lo user non è ancora stato salvato
                return;
            }
            bool asProfilePart = true;
            try {
                var profpart = ((dynamic)UserContent).ProfilePart;
                asProfilePart = true;
            } catch { asProfilePart = false; }
            int iduser = UserContent.Id;
            var contactsUsers = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(x => x.UserPartRecord_Id == iduser).List().FirstOrDefault();
            ContentItem Contact = null;
            if (contactsUsers == null) {
                // cerca un eventuale contatto con la stessa mail ma non ancora legato a uno user
                var contactEmailList = GetContactsFromMail(UserContent.Email);
                foreach (var contactEmail in contactEmailList) {
                    if ((contactEmail != null) && (contactEmail.ContentType == "CommunicationContact")) {
                        if ((contactEmail.As<CommunicationContactPart>().Record.UserPartRecord_Id == 0) && (contactEmail.As<CommunicationContactPart>().Master == false)) {
                            Contact = contactEmail;
                            Contact.As<CommunicationContactPart>().Logs += string.Format(T("This contact has been bound to its user on {0:yyyy-MM-dd HH:mm} by contact synchronize function.").Text, DateTime.Now);
                            break; // associa solo il primo contatto che trova
                        }
                    }
                }

                if (Contact == null) {
                    Contact = _orchardServices.ContentManager.New("CommunicationContact");
                    _orchardServices.ContentManager.Create(Contact);
                    Contact = _orchardServices.ContentManager.Get(Contact.Id);
                    Contact.As<CommunicationContactPart>().Master = false;
                }
            } else {
                Contact = contactsUsers.ContentItem;
            }

            //if (UserContent.ContentItem.User.PushCategories != null) {
            //    dynamic mypart = (((dynamic)Contact).CommunicationContactPart);
            //    mypart.GetType().GetProperty("UserIdentifier").SetValue(mypart, UserContent.Id, null);
            //}
            try {
                if (((dynamic)UserContent.ContentItem).User.Pushcategories != null && (((dynamic)Contact).CommunicationContactPart).Pushcategories != null) {
                    //List<TermPart> ListTermPartToAdd = ((TaxonomyField)((dynamic)UserContent.ContentItem).User.Pushcategories).Terms.ToList();
                    List<TermPart> ListTermPartToAdd = _taxonomyService.GetTermsForContentItem(UserContent.Id, "Pushcategories").ToList();
                    _taxonomyService.UpdateTerms(Contact, ListTermPartToAdd, "Pushcategories");
                }
            } catch { // non ci sono le Pushcategories
            }
            try {
                if ((UserContent.ContentItem.As<FavoriteCulturePart>() != null) && (Contact.As<FavoriteCulturePart>() != null)) {
                    if (UserContent.ContentItem.As<FavoriteCulturePart>().Culture_Id != 0) {
                        if (UserContent.ContentItem.As<FavoriteCulturePart>().Culture_Id != Contact.As<FavoriteCulturePart>().Culture_Id) {
                            Contact.As<FavoriteCulturePart>().Culture_Id = UserContent.ContentItem.As<FavoriteCulturePart>().Culture_Id;
                        }
                    }
                    else {
                        // imposta la culture di default
                        var defaultCultureId = _cultureManager.GetCultureByName(_cultureManager.GetSiteCulture()).Id;
                        Contact.As<FavoriteCulturePart>().Culture_Id = defaultCultureId;
                        UserContent.ContentItem.As<FavoriteCulturePart>().Culture_Id = defaultCultureId;
                    }
                }
            } catch { // non si ha l'estensione per favorite culture
            }

            var contact = Contact; //GetContactFromUser(UserContent.Id);

            // email
            if (!string.IsNullOrEmpty(UserContent.Email) && UserContent.ContentItem.As<UserPart>().RegistrationStatus == UserStatus.Approved) {
                CommunicationEmailRecord cmr = null;
                if (contact != null) {
                    cmr = _repositoryCommunicationEmailRecord.Fetch(x => x.Email == UserContent.Email && x.EmailContactPartRecord_Id == contact.Id).FirstOrDefault();
                }
                if (cmr != null) {
                    if (cmr.EmailContactPartRecord_Id != Contact.Id) {
                        cmr.EmailContactPartRecord_Id = Contact.Id;
                        cmr.DataModifica = DateTime.Now;
                        _repositoryCommunicationEmailRecord.Update(cmr);
                        _repositoryCommunicationEmailRecord.Flush();
                    }
                } else {
                    CommunicationEmailRecord newrec = new CommunicationEmailRecord();
                    newrec.Email = UserContent.Email;
                    newrec.EmailContactPartRecord_Id = Contact.Id;
                    newrec.Id = 0;
                    newrec.Validated = true;
                    newrec.DataInserimento = DateTime.Now;
                    newrec.DataModifica = DateTime.Now;
                    newrec.Produzione = true;
                    _repositoryCommunicationEmailRecord.Create(newrec);
                    _repositoryCommunicationEmailRecord.Flush();
                }
            }

            // sms
            try {
                dynamic userPwdRecoveryPart = ((dynamic)UserContent.ContentItem).UserPwdRecoveryPart;
                if (userPwdRecoveryPart != null) {
                    string pref = userPwdRecoveryPart.InternationalPrefix;
                    string num = userPwdRecoveryPart.PhoneNumber;
                    CommunicationContactPart ciCommunication = contact.As<CommunicationContactPart>();
                    if (ciCommunication != null) {
                        CommunicationSmsRecord csr = _repositoryCommunicationSmsRecord.Fetch(x => x.SmsContactPartRecord_Id == ciCommunication.ContentItem.Id).FirstOrDefault();
                        if (csr == null) {
                            CommunicationSmsRecord newsms = new CommunicationSmsRecord();
                            newsms.Prefix = pref;
                            newsms.Sms = num;
                            newsms.SmsContactPartRecord_Id = ciCommunication.ContentItem.Id;
                            newsms.Id = 0;
                            newsms.Validated = true;
                            newsms.DataInserimento = DateTime.Now;
                            newsms.DataModifica = DateTime.Now;
                            newsms.Produzione = true;
                            _repositoryCommunicationSmsRecord.Create(newsms);
                            _repositoryCommunicationSmsRecord.Flush();
                        }
                        else {
                            csr.Prefix = pref;
                            csr.Sms = num;
                            csr.SmsContactPartRecord_Id = ciCommunication.ContentItem.Id;
                            csr.DataModifica = DateTime.Now;
                            _repositoryCommunicationSmsRecord.Update(csr);
                            _repositoryCommunicationSmsRecord.Flush();
                        }
                    }
                }
            }
            catch { 
                // non è abilitato il modulo Laser.Mobile.SMS, quindi non allineo il telefono
            }

            if (string.IsNullOrWhiteSpace(UserContent.UserName) == false) {
                Contact.As<TitlePart>().Title = UserContent.UserName;
            }
            else if (string.IsNullOrWhiteSpace(UserContent.Email) == false) {
                Contact.As<TitlePart>().Title = UserContent.Email;
            }
            else {
                Contact.As<TitlePart>().Title = string.Format("User with ID {0}", UserContent.Id);
            }
            //Contact.As<TitlePart>().Title = UserContent.Email + " " + UserContent.UserName;
            if (Contact.Has<CommonPart>()) {
                Contact.As<CommonPart>().ModifiedUtc = DateTime.Now;
                Contact.As<CommonPart>().Owner = UserContent;
            }
            dynamic mypart = (((dynamic)Contact).CommunicationContactPart);
            mypart.GetType().GetProperty("UserIdentifier").SetValue(mypart, UserContent.Id, null);
            if (asProfilePart) {
                List<ContentPart> Lcp = new List<ContentPart>();
                Lcp.Add(((ContentPart)((dynamic)Contact).ProfilePart));
                foreach (dynamic cf in ((dynamic)UserContent).ProfilePart.Fields) {
                    object myval;
                    if (cf.FieldDefinition.Name == typeof(DateTimeField).Name)
                        myval = ((object)(((dynamic)cf).DateTime));
                    else if (cf.FieldDefinition.Name == typeof(MediaLibraryPickerField).Name || cf.FieldDefinition.Name == typeof(ContentPickerField).Name)
                        myval = ((Int32[])cf.Ids).ToList().Select(x => (object)x).ToList();
                    else if (cf.FieldDefinition.Name == typeof(TaxonomyField).Name) {
                        List<TaxoVM> second = new List<TaxoVM>();
                        var selectedTerms = _taxonomyService.GetTermsForContentItem(UserContent.Id, ((ContentField)cf).Name);
                        foreach (TermPart tp in selectedTerms) {
                            TaxoVM tv = new TaxoVM();
                            tv.Id = tp.Id;
                            tv.flag = true;
                            second.Add(tv);
                        }
                        myval = ((object)(second.Select(x => (dynamic)x).ToList()));
                    }
                    else {
                        myval = ((object)(((dynamic)cf).Value));
                    }
                    _contentExtensionsServices.StoreInspectExpandoFields(Lcp, ((string)((dynamic)cf).Name), myval, Contact);
                }
            }
        }
    }
}