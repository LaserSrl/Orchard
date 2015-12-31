using Laser.Orchard.CommunicationGateway.Helpers;
using Laser.Orchard.CommunicationGateway.Services;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Laser.Orchard.CommunicationGateway.Controllers {

    public class ContactsAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly string contentType = "CommunicationContact";
        private readonly dynamic TestPermission = Permissions.ManageContact;
        private readonly ICommunicationService _communicationService;
        private readonly INotifier _notifier;
        private Localizer T { get; set; }

        public ContactsAdminController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager,
             ICommunicationService communicationService) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _communicationService = communicationService;
        }

        [Admin]
        public ActionResult Synchronize() {
            if (_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner)) {
                _communicationService.Synchronize();
            }
            return RedirectToAction("Index", "ContactsAdmin");
        }

        [Admin]
        public ActionResult Edit(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            object model;
            if (id == 0) {
                var newContent = _contentManager.New(contentType);
                //if (idCampaign > 0) {
                //    List<int> lids = new List<int>();
                //    lids.Add(idCampaign);
                //    ((dynamic)newContent).CommunicationAdvertisingPart.Campaign.Ids = lids.ToArray();
                //}
                //  model = _contentManager.BuildEditor(newContent);
                //   _contentManager.Create(newContent);
                model = _contentManager.BuildEditor(newContent);
            } else
                model = _contentManager.BuildEditor(_contentManager.Get(id, VersionOptions.Latest));
            return View((object)model);
        }

        [HttpPost, ActionName("Edit"), Admin]
        public ActionResult EditPOST(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            ContentItem content;
            if (id == 0) {
                var newContent = _contentManager.New(contentType);
                _contentManager.Create(newContent, VersionOptions.Draft);
                content = newContent;
            } else
                content = _contentManager.Get(id, VersionOptions.DraftRequired);
            var model = _contentManager.UpdateEditor(content, this);
            if (!ModelState.IsValid) {
                foreach (string key in ModelState.Keys) {
                    if (ModelState[key].Errors.Count > 0)
                        foreach (var error in ModelState[key].Errors)
                            _notifier.Add(NotifyType.Error, T(error.ErrorMessage));
                }
                _orchardServices.TransactionManager.Cancel();
                return View(model);
            } else {
                _contentManager.Publish(content);
            }
            _notifier.Add(NotifyType.Information, T("Contact saved"));
            return RedirectToAction("Edit", new { id = content.Id });
        }
        [HttpPost]
        [Admin]
        public ActionResult Remove(Int32 id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            ContentItem content = _contentManager.Get(id);
            _contentManager.Remove(content);
            return RedirectToAction("Index", "ContactsAdmin");
        }

        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, SearchVM search) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, search);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, SearchVM search) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            dynamic Options = new System.Dynamic.ExpandoObject();
            var expression = search.Expression;
            IContentQuery<ContentItem> contentQuery = _contentManager.Query(VersionOptions.Latest).ForType(contentType);//.OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc); //Performance issues on heavy ContentItems numbers #6247
            if (!string.IsNullOrEmpty(search.Expression))
                contentQuery = contentQuery.Where<TitlePartRecord>(w => w.Title.Contains(expression));
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(contentQuery.Count());
            var pageOfContentItems = contentQuery.Slice(pager.GetStartIndex(), pager.PageSize)
                .Select(p => new ContentIndexVM {
                    Id = p.Id,
                    Title = ((dynamic)p).TitlePart.Title,
                    ModifiedUtc = ((dynamic)p).CommonPart.ModifiedUtc,
                    UserName = ((dynamic)p).CommonPart.Owner != null ? ((dynamic)p).CommonPart.Owner.UserName : "Anonymous",
                }).ToList();

            if (pageOfContentItems == null) {
                pageOfContentItems = new List<ContentIndexVM>();
            }
            _orchardServices.New.List();
            var model = new SearchIndexVM(pageOfContentItems, search, pagerShape, Options);
            return View((object)model);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.Text);
        }
    }
}