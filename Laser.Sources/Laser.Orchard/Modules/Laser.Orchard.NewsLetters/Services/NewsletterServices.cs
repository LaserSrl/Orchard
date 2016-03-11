using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using Laser.Orchard.Commons.Extensions;
using Laser.Orchard.NewsLetters.Extensions;
using Laser.Orchard.NewsLetters.Models;
using Laser.Orchard.NewsLetters.ViewModels;
using Laser.Orchard.StartupConfig.Extensions;
using Laser.Orchard.TemplateManagement.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Email.Models;
using Orchard.Email.Services;
using Orchard.Localization;
using Orchard.Messaging.Services;
using Orchard.Mvc.Html;
using Orchard.UI.Notify;
using Orchard.Events;
using System.Diagnostics;

namespace Laser.Orchard.NewsLetters.Services {
    public interface IJobsQueueService : IEventHandler {
        void Enqueue(string message, object parameters, int priority);
    }
    public class NewsletterServices : Laser.Orchard.NewsLetters.Services.INewsletterServices {
        private readonly IJobsQueueService _jobsQueueService;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IRepository<SubscriberRecord> _repositorySubscribers;
        private readonly IRepository<NewsletterDefinitionPartRecord> _repositoryNewsletterDefinition;
        private readonly IRepository<NewsletterEditionPartRecord> _repositoryNewsletterEdition;
        private readonly ITemplateService _templateService;
        private readonly IMessageService _messageService;
        private readonly INotifier _notifier;
        public NewsletterServices(

            IJobsQueueService jobsQueueService,
            IContentManager contentManager,
            IOrchardServices orchardServices,
            ITemplateService templateService,
             IMessageService messageService,
            IRepository<SubscriberRecord> repositorySubscribers,
            IRepository<NewsletterDefinitionPartRecord> repositoryNewsletterDefinition,
            IRepository<NewsletterEditionPartRecord> repositoryNewsletterEdition,
             INotifier notifier) {
            _notifier = notifier;
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _messageService = messageService;
            _repositorySubscribers = repositorySubscribers;
            _repositoryNewsletterDefinition = repositoryNewsletterDefinition;
            T = NullLocalizer.Instance;
            _templateService = templateService;
            _repositoryNewsletterEdition = repositoryNewsletterEdition;
            _jobsQueueService = jobsQueueService;
        }

        public Localizer T { get; set; }


        #region [ NewsletterDefinition ]
        public ContentItem GetNewsletterDefinition(int id, VersionOptions versionOptions) {
            var NewsletterDefinitionPart = _contentManager.Get<NewsletterDefinitionPart>(id, versionOptions);
            return NewsletterDefinitionPart == null ? null : NewsletterDefinitionPart.ContentItem;
        }

        public IEnumerable<NewsletterDefinitionPart> GetNewsletterDefinition() {
            return GetNewsletterDefinition(VersionOptions.Published);
        }

        public IEnumerable<NewsletterDefinitionPart> GetNewsletterDefinition(VersionOptions versionOptions) {
            return _contentManager.Query<NewsletterDefinitionPart, NewsletterDefinitionPartRecord>(versionOptions)
                .Join<TitlePartRecord>()
                .OrderBy(br => br.Title)
                .List();
        }

        public IEnumerable<NewsletterDefinitionPart> GetNewsletterDefinition(string ids, VersionOptions versionOptions) {
            var newsIds = ids.Split(',').Select(s => Convert.ToInt32(s)).ToList();
            return _contentManager.Query<NewsletterDefinitionPart, NewsletterDefinitionPartRecord>(versionOptions)
                .Where(w => newsIds.Contains(w.Id))
                .Join<TitlePartRecord>()
                .OrderBy(br => br.Title)
                .List();
        }

        public void DeleteNewsletterDefinition(ContentItem newsletterDefinition) {
            _contentManager.Remove(newsletterDefinition);
        }
        #endregion


        #region [ NewsletterEdition | Item]

        public ContentItem GetNewsletterEdition(int id, VersionOptions versionOptions) {
            var newsletterEditionPart = _contentManager.Get<NewsletterEditionPart>(id, versionOptions);
            return newsletterEditionPart == null ? null : newsletterEditionPart.ContentItem;
        }

        public IEnumerable<NewsletterEditionPart> GetNewsletterEditions(int newsletterId) {
            return GetNewsletterEditions(newsletterId, VersionOptions.Published);
        }

        public IEnumerable<NewsletterEditionPart> GetNewsletterEditions(int newsletterId, VersionOptions versionOptions) {
            return _contentManager.Query<NewsletterEditionPart, NewsletterEditionPartRecord>(versionOptions)
                .Where(w => w.NewsletterDefinitionPartRecord_Id == newsletterId)
                .Join<TitlePartRecord>()
                .OrderBy(br => br.Title)
                .List();
        }

        public void DeleteNewsletterEdition(ContentItem newsletterDefinition) {
            _contentManager.Remove(newsletterDefinition);
        }

        public void SendNewsletterEdition(ref NewsletterEditionPart newsletterEdition, bool isTest = false, string testEmail = "") {
            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);
            var subscribers = GetSubscribers(newsletterEdition.NewsletterDefinitionPartRecord_Id).Where(w => w.Confirmed);
            int[] selectedAnnIds;
            IList<AnnouncementPart> items = null;
            IEnumerable<ExpandoObject> fullyItems;
            if (!String.IsNullOrWhiteSpace(newsletterEdition.AnnouncementIds)) {
                selectedAnnIds = !String.IsNullOrWhiteSpace(newsletterEdition.AnnouncementIds) ? newsletterEdition.AnnouncementIds.Split(',').Select(s => Convert.ToInt32(s)).ToArray() : null;
                items = GetAnnouncements(selectedAnnIds);
                fullyItems = items.Select(
                    s => new {
                        AnnouncementPart = s,
                        DisplayUrl = urlHelper.ItemDisplayUrl(s)
                    }.ToExpando());
            }
            else {
                fullyItems = null;
            }
            var model = new {
                NewsletterEdition = newsletterEdition,
                ContentItems = fullyItems
            }.ToExpando();
            if (!isTest) {
                var subscribersEmails = subscribers.Select(s => s.Email);
                if (SendEmail((dynamic)model,
                    GetNewsletterDefinition(newsletterEdition.NewsletterDefinitionPartRecord_Id,
                        VersionOptions.Published).As<NewsletterDefinitionPart>().TemplateRecord_Id,
                        null, subscribersEmails)) {

                    // Aggiorno la newsletter edition, e rimuovo la relazione tra Newletter e Announcement 
                    newsletterEdition.Dispatched = true;
                    newsletterEdition.DispatchDate = DateTime.Now;
                    newsletterEdition.Number = GetNextNumber(newsletterEdition.NewsletterDefinitionPartRecord_Id); ;
                    foreach (var item in items) {
                        var ids = ("," + item.AttachToNextNewsletterIds + ",").Replace("," + newsletterEdition.NewsletterDefinitionPartRecord_Id + ",", "");
                        item.AttachToNextNewsletterIds = ids;
                    }
                }
            }
            else if (!String.IsNullOrWhiteSpace(testEmail)) {
                if (SendEmail((dynamic)model,
                    GetNewsletterDefinition(newsletterEdition.NewsletterDefinitionPartRecord_Id,
                        VersionOptions.Published).As<NewsletterDefinitionPart>().TemplateRecord_Id,
                        new List<string> { testEmail }, null))
                    _orchardServices.Notifier.Information(T("Newsletter edition sent to a test email!"));
            }
            else if (String.IsNullOrWhiteSpace(testEmail)) {
                _orchardServices.Notifier.Error(T("Enter a test email!"));
            }
        }


        public IList<AnnouncementLookupViewModel> GetNextAnnouncements(int newsltterId, int[] selectedIds) {
            var list = _contentManager.Query<AnnouncementPart, AnnouncementPartRecord>(VersionOptions.Published)
                .Where(w => w.AttachToNextNewsletterIds.Contains(newsltterId.ToString()))
                .OrderBy(br => br.AnnouncementTitle)
                .List()
                .Select(s => new AnnouncementLookupViewModel {
                    Id = s.Id,
                    Title = String.IsNullOrWhiteSpace(s.AnnouncementTitle) ? s.ContentItem.As<TitlePart>().Title : s.AnnouncementTitle,
                    Selected = selectedIds != null && selectedIds.Contains(s.Id)
                });
            return list.ToList();
        }

        public IList<AnnouncementPart> GetAnnouncements(int[] selectedIds) {
            var list = _contentManager.Query<AnnouncementPart, AnnouncementPartRecord>(VersionOptions.Published)
                .Where(w => selectedIds.Contains(w.Id))
                .OrderBy(br => br.AnnouncementTitle)
                .List();
            return list.ToList();

        }

        public int GetNextNumber(int newsltterId) {
            var maxNumber = _repositoryNewsletterEdition.Table
                 .Where(w => w.NewsletterDefinitionPartRecord_Id == newsltterId)
                 .Select(s => s.Number)
                 .Max();
            return (maxNumber.HasValue ? maxNumber.Value + 1 : 1);
        }
        #endregion


        #region [ Subscribers ]

        public SubscriberRecord GetSubscriber(int id) {
            var subscriber = _repositorySubscribers
                .Get(id);
            return subscriber;
        }

        public IEnumerable<SubscriberRecord> GetSubscribers(int newsletterId) {
            var subscriber = _repositorySubscribers
                .Fetch(w => w.NewsletterDefinition.Id == newsletterId);
            return subscriber;
        }

        public SubscriberRecord TryRegisterSubscriber(SubscriberViewModel subscriber) {
            SubscriberRecord returnValue = null;
            try {
                var subs = _repositorySubscribers.Table.Where(w => w.Email == subscriber.Email &&
                    w.NewsletterDefinition.Id == subscriber.NewsletterDefinition_Id).SingleOrDefault();
                if (subs == null) {
                    subs = new SubscriberRecord {
                        Email = subscriber.Email,
                        Confirmed = false,
                        SubscriptionDate = DateTime.Now,
                        Name = subscriber.Name,
                        NewsletterDefinition = _repositoryNewsletterDefinition.Get(subscriber.NewsletterDefinition_Id)
                    };
                    try {
                        _repositorySubscribers.Create(subs);
                        returnValue = subs;
                    }
                    catch (Exception ex) {
                        _orchardServices.Notifier.Error(T(ex.Message));
                        returnValue = null;
                    }

                }
                else if (!subs.Confirmed) {
                    subs.SubscriptionDate = DateTime.Now;
                    subs.Name = subscriber.Name;
                    subs.Guid = Guid.NewGuid().ToString();
                    try {
                        _repositorySubscribers.Update(subs);
                        returnValue = subs;
                    }
                    catch (Exception ex) {
                        _orchardServices.Notifier.Error(T(ex.Message));
                        returnValue = null;
                    }
                }
                else {
                    _orchardServices.Notifier.Information(T("Email already registered!"));
                }
                if (returnValue != null) {
                    dynamic viewModel = new SubscriberViewModel {
                        Email = subs.Email,
                        Name = subs.Name,
                        Guid = subs.Guid,
                        NewsletterDefinition_Id = subs.NewsletterDefinition.Id,
                        NewsletterDefinition = _contentManager.Get(subs.NewsletterDefinition.Id)
                    };
                    SendEmail(viewModel, subs.NewsletterDefinition.ConfirmSubscrptionTemplateRecord_Id, new List<string> { subs.Email }, null);

                }
            }
            catch {
                returnValue = null;
            }

            return returnValue;
        }

        public SubscriberRecord TryRegisterConfirmSubscriber(SubscriberViewModel subscriber) {
            try {
                var subs = _repositorySubscribers.Table.Where(w => w.Email == subscriber.Email &&
                w.Guid == subscriber.Guid).SingleOrDefault();
                if (subs != null && !subs.Confirmed) {
                    subs.ConfirmationDate = DateTime.Now;
                    subs.Confirmed = true;
                    try {
                        _repositorySubscribers.Update(subs);
                        return subs;
                    }
                    catch (Exception ex) {
                        _orchardServices.Notifier.Error(T(ex.Message));
                        return null;
                    }
                }
                else if (subs != null && subs.Confirmed) {
                    _orchardServices.Notifier.Information(T("Email already registered!"));
                    return null;
                }
            }
            catch {
            }

            return null;
        }

        public SubscriberRecord TryUnregisterSubscriber(SubscriberViewModel subscriber) {
            SubscriberRecord returnValue = null;
            try {
                var subs = _repositorySubscribers.Table.Where(w => w.Email == subscriber.Email &&
                    w.NewsletterDefinition.Id == subscriber.NewsletterDefinition_Id).SingleOrDefault();
                if (subs == null) {
                    _orchardServices.Notifier.Information(T("Email not found!"));
                    return null;
                }
                else if (subs.Confirmed) {
                    returnValue = subs;
                }
                else {
                    _orchardServices.Notifier.Information(T("Email not found!"));
                    return null;
                }
                if (returnValue != null) {
                    dynamic viewModel = new SubscriberViewModel {
                        Email = subs.Email,
                        Name = subs.Name,
                        Guid = subs.Guid,
                        NewsletterDefinition_Id = subs.NewsletterDefinition.Id,
                        NewsletterDefinition = _contentManager.Get(subs.NewsletterDefinition.Id)
                    };
                    SendEmail(viewModel, subs.NewsletterDefinition.DeleteSubscrptionTemplateRecord_Id, new List<string> { subs.Email }, null);

                }
            }
            catch {
                returnValue = null;
            }

            return returnValue;
        }

        public SubscriberRecord TryUnregisterConfirmSubscriber(SubscriberViewModel subscriber) {
            try {
                var subs = _repositorySubscribers.Table.Where(w => w.Email == subscriber.Email &&
                    w.Guid == subscriber.Guid).SingleOrDefault();
                if (subs != null && subs.Confirmed) {
                    subs.UnsubscriptionDate = DateTime.Now;
                    subs.Confirmed = false;
                    try {
                        _repositorySubscribers.Update(subs);
                        return subs;
                    }
                    catch (Exception ex) {
                        _orchardServices.Notifier.Error(T(ex.Message));
                        return null;
                    }
                }
                else if (subs != null && !subs.Confirmed) {
                    _orchardServices.Notifier.Information(T("Email not yet registered!"));
                    return null;
                }
            }
            catch {

            }
            return null;
        }
        #endregion

        private bool SendEmail(dynamic contentModel, int templateId, IEnumerable<string> sendTo, IEnumerable<string> bcc, bool queued = true) {
            ParseTemplateContext templatectx = new ParseTemplateContext();
            var template = _templateService.GetTemplate(templateId);
            var urlHelper = new UrlHelper(_orchardServices.WorkContext.HttpContext.Request.RequestContext);

            // Creo un model che ha Content (il contentModel), Urls con alcuni oggetti utili per il template
            // Nel template pertanto Model, diventa Model.Content
            var host = string.Format("{0}://{1}{2}",
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Scheme,
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Host,
                                    _orchardServices.WorkContext.HttpContext.Request.Url.Port == 80
                                        ? string.Empty
                                        : ":" + _orchardServices.WorkContext.HttpContext.Request.Url.Port);
            var dynamicModel = new {
                Content = contentModel,
                Urls = new {
                    SubscriptionSubscribe = urlHelper.SubscriptionSubscribe(),
                    SubscriptionUnsubscribe = urlHelper.SubscriptionUnsubscribe(),
                    SubscriptionConfirmSubscribe = urlHelper.SubscriptionConfirmSubscribe(),
                    SubscriptionConfirmUnsubscribe = urlHelper.SubscriptionConfirmUnsubscribe(),
                    BaseUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl,
                    MediaUrl = urlHelper.MediaExtensionsImageUrl(),
                    Domain = host,

                }.ToExpando()
            };
            templatectx.Model = dynamicModel;

            var body = _templateService.ParseTemplate(template, templatectx);
            if (body.StartsWith("Error On Template")) {
                _notifier.Add(NotifyType.Error, T("Error on template, mail not sended"));
                return false;
            }
            var data = new Dictionary<string, object>();
            var smtp = _orchardServices.WorkContext.CurrentSite.As<SmtpSettingsPart>();
            var recipient = sendTo != null ? sendTo : new List<string> { smtp.Address };
            data.Add("Subject", template.Subject);
            data.Add("Body", body);
            data.Add("Recipients", String.Join(",", recipient));
            if (bcc != null) {
                data.Add("Bcc", String.Join(",", bcc));
            }
            //var watch = Stopwatch.StartNew();
            //int msgsent = 0;

            //for(int i=0;i<20;i++) {
            //    msgsent++;
            //    data["Subject"] = msgsent.ToString();
            //    data["Bcc"] = "lorenzo.frediani@laser-group.com";
            //    _messageService.Send(SmtpMessageChannel.MessageType, data);
            //}
            //watch.Stop();
            //_notifier.Add(NotifyType.Information, T("Sent " + msgsent.ToString()+" email in Milliseconds:" + watch.ElapsedMilliseconds.ToString()));            
            if (!queued) {
                _messageService.Send(SmtpMessageChannel.MessageType, data);
            }
            else {
                var priority = 0;//normal 50 to hight -50 to low

                _jobsQueueService.Enqueue("IMessageService.Send", new { type = SmtpMessageChannel.MessageType, parameters = data }, priority);
            }

            return true;
        }

    }
}