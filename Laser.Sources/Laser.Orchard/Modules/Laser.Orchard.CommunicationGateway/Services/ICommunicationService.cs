using Laser.Orchard.CommunicationGateway.Models;
using Laser.Orchard.ShortLinks.Services;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Fields;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Fields.Fields;
using Orchard.Localization;
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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.CommunicationGateway.Services {

    public interface ICommunicationService : IDependency {

        bool AdvertisingIsAvailable(Int32 id);

        string GetCampaignLink(string CampaignSource, ContentPart part);

        bool CampaignLinkExist(ContentPart part);

        void UserToContact(IUser UserContent);

        CommunicationContactPart GetContactFromUser(int iduser);

        void Synchronize();
    }

    public class CommunicationService : ICommunicationService {
        private readonly IOrchardServices _orchardServices;
        private readonly IShortLinksService _shortLinksService;
        private readonly IContentExtensionsServices _contentExtensionsServices;
        private readonly IModuleService _moduleService;
        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        private readonly IRepository<CommunicationEmailRecord> _repositoryCommunicationEmailRecord;

        public CommunicationService(IRepository<CommunicationEmailRecord> repositoryCommunicationEmailRecord, INotifier notifier, IModuleService moduleService, IOrchardServices orchardServices, IShortLinksService shortLinksService, IContentExtensionsServices contentExtensionsServices) {
            _orchardServices = orchardServices;
            _shortLinksService = shortLinksService;
            _contentExtensionsServices = contentExtensionsServices;
            _moduleService = moduleService;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _repositoryCommunicationEmailRecord = repositoryCommunicationEmailRecord;
        }


        public bool AdvertisingIsAvailable(Int32 id) {
            ContentItem ci = _orchardServices.ContentManager.Get(id);
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


        public void Synchronize() {

            #region Creazione di un Contact Master a cui agganciare tutte le parti che non hanno una profilazione

            if (_orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(y => y.Master).Count() == 0) {
                var Contact = _orchardServices.ContentManager.New("CommunicationContact");
                _orchardServices.ContentManager.Create(Contact);
                Contact.As<TitlePart>().Title = "Master Contact";
                Contact.As<CommunicationContactPart>().Master = true;
                _notifier.Add(NotifyType.Information, T("Master Contact Created"));
            }

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

        /// <summary>
        ///La parte sarebbe CommunicationAdvertisingPart ma nobn l'ho definita quindi passo una cosa generica (ContentPart)
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public string GetCampaignLink(string CampaignSource, ContentPart generalpart) {
            //string CampaignSource = "email";
            string shortlink = "";
            ContentPart part = (ContentPart)(((dynamic)generalpart).ContentItem.CommunicationAdvertisingPart);
            string CampaignTerm = string.Join("+", part.ContentItem.As<TagsPart>().CurrentTags.ToArray()).ToLower();
            string CampaignMedium = CampaignSource;
            string CampaignContent = part.ContentItem.As<TitlePart>().Title.ToLower();
            string CampaignName = "Flash";
            try {
                int idCampagna = ((int)((dynamic)part).CampaignId);
                CampaignName = _orchardServices.ContentManager.Get(idCampagna).As<TitlePart>().Title;
            } catch (Exception ex) {
                // cuomunicato non legato a campagna
            }
            string link = "";
            if (!string.IsNullOrEmpty(((dynamic)part).UrlLinked.Value)) {
                link = (string)(((dynamic)part).UrlLinked.Value);
            } else {
                var pickerField = ((dynamic)part).ContentLinked as ContentPickerField;
                if (pickerField != null) {
                    var firstItem = pickerField.ContentItems.FirstOrDefault();
                    if (firstItem != null) {
                        var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
                        link = urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(firstItem));
                    }
                } else
                    return "";
            }

            string linkelaborated = ElaborateLink(link, CampaignSource, CampaignMedium, CampaignTerm, CampaignContent, CampaignName);
            if (!string.IsNullOrEmpty(linkelaborated)) {
                shortlink = _shortLinksService.GetShortLink(linkelaborated);
                if (string.IsNullOrEmpty(shortlink)) {
                    throw new Exception("Url Creation Failed");
                }
            }
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
                    var firstItem = pickerField.ContentItems.FirstOrDefault();
                    if (firstItem != null) {
                        linkExist = true;
                    }
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

        public void UserToContact(IUser UserContent) {
            bool asProfilePart = true;
            try {
                var profpart = ((dynamic)UserContent).ProfilePart;
                asProfilePart = true;
            } catch { asProfilePart = false; }
            int iduser = UserContent.Id;
            var contactsUsers = _orchardServices.ContentManager.Query<CommunicationContactPart, CommunicationContactPartRecord>().Where(x => x.UserPartRecord_Id == iduser).List().FirstOrDefault();
            ContentItem Contact;
            if (contactsUsers == null) {
                Contact = _orchardServices.ContentManager.New("CommunicationContact");
                _orchardServices.ContentManager.Create(Contact);
            } else {
                Contact = contactsUsers.ContentItem;
            }
            if (!string.IsNullOrEmpty(UserContent.Email) && UserContent.ContentItem.As<UserPart>().RegistrationStatus == UserStatus.Approved) {
                CommunicationEmailRecord cmr = _repositoryCommunicationEmailRecord.Fetch(x => x.Email == UserContent.Email).FirstOrDefault();
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
            Contact.As<TitlePart>().Title = UserContent.Email + " " + UserContent.UserName;
            dynamic mypart = (((dynamic)Contact).CommunicationContactPart);
            mypart.GetType().GetProperty("UserIdentifier").SetValue(mypart, UserContent.Id, null);
            if (asProfilePart) {
                List<ContentPart> Lcp = new List<ContentPart>();
                Lcp.Add(((ContentPart)((dynamic)Contact).ProfilePart));
                foreach (dynamic cf in ((dynamic)UserContent).ProfilePart.Fields) {
                    object myval;
                    if (cf.FieldDefinition.Name == typeof(DateTimeField).Name)
                        myval = ((object)(((dynamic)cf).DateTime));
                    else
                        if (cf.FieldDefinition.Name == typeof(MediaLibraryPickerField).Name || cf.FieldDefinition.Name == typeof(ContentPickerField).Name)
                            myval = ((Int32[])cf.Ids).ToList().Select(x => (object)x).ToList();
                        else
                            if (cf.FieldDefinition.Name == typeof(TaxonomyField).Name) {
                                List<TaxoVM> second = new List<TaxoVM>();
                                foreach (TermPart tp in ((TaxonomyField)cf).Terms) {
                                    TaxoVM tv = new TaxoVM();
                                    tv.Id = tp.Id;
                                    tv.flag = true;
                                    second.Add(tv);
                                }
                                myval = ((object)(second.Select(x => (dynamic)x).ToList()));
                            } else
                                myval = ((object)(((dynamic)cf).Value));
                    _contentExtensionsServices.StoreInspectExpandoFields(Lcp, ((string)((dynamic)cf).Name), myval, Contact);
                }
            }
        }
    }
}