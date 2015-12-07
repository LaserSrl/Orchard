using Laser.Orchard.CommunicationGateway.Helpers;
using Laser.Orchard.CommunicationGateway.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.MediaLibrary.Models;


namespace Laser.Orchard.CommunicationGateway.Controllers {

    public class CampaignAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly string contentType = "CommunicationCampaign";
        private readonly dynamic TestPermission = Permissions.ManageCampaigns;
        private readonly INotifier _notifier;
        private Localizer T { get; set; }

        public CampaignAdminController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        [Admin]
        public ActionResult Edit(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            object model;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                //  model = _orchardServices.ContentManager.BuildEditor(newContent);
                //   _contentManager.Create(newContent);
                model = _contentManager.BuildEditor(newContent);
            }
            else
                model = _contentManager.BuildEditor(_orchardServices.ContentManager.Get(id));
            return View((object)model);
        }

        [HttpPost, ActionName("Edit"), Admin]
        public ActionResult EditPOST(int id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            ContentItem content;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                _orchardServices.ContentManager.Create(newContent);
                content = newContent;
            }
            else
                content = _orchardServices.ContentManager.Get(id);
            var model = _orchardServices.ContentManager.UpdateEditor(content, this);

            if (!ModelState.IsValid) {
                foreach (string key in ModelState.Keys) {
                    if (ModelState[key].Errors.Count > 0)
                        foreach (var error in ModelState[key].Errors)
                            _notifier.Add(NotifyType.Error, T(error.ErrorMessage));
                }
                _orchardServices.TransactionManager.Cancel();
                return View(model);
            }
            _notifier.Add(NotifyType.Information, T("Campaign saved"));
            return RedirectToAction("Index", "CampaignAdmin");
        }

        [HttpPost]
        [Admin]
        public ActionResult Remove(Int32 id) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            ContentItem content = _orchardServices.ContentManager.Get(id);
            IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query().ForType("CommunicationAdvertising");

            IEnumerable<ContentItem> ListContent = contentQuery.List().Where(x => ((int[])((dynamic)x).CommunicationAdvertisingPart.Campaign.Ids).Contains(id));
            if (ListContent.Count() == 0)
                _orchardServices.ContentManager.Remove(content);
            else
                _notifier.Add(NotifyType.Warning, T("Can't remove campaign with advertise"));

            return RedirectToAction("Index", "CampaignAdmin");
        }


        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, SearchVM search, bool ShowVideo = false) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, search, ShowVideo);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, SearchVM search, bool ShowVideo = false) {
            dynamic Options = new System.Dynamic.ExpandoObject();
            Options.ShowVideo = false;
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            var expression = search.Expression;
            IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query().ForType(contentType).OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc);
            IEnumerable<ContentItem> ListContent = contentQuery.List();
            if (ListContent.Count() == 0 || ShowVideo) {
                if (_orchardServices.ContentManager.Query<TitlePart, TitlePartRecord>("Video").Where(x => x.Title == "HowTo" + contentType).List().Count() > 0) {
                    Options.ShowVideo = true;
                    Options.VideoContent = _orchardServices.ContentManager.Query<TitlePart, TitlePartRecord>("Video").Where(x => x.Title == "HowTo" + contentType).List().Where(y => y.ContentItem.IsPublished()).FirstOrDefault().ContentItem;
                }

            }
            if (!string.IsNullOrEmpty(search.Expression))
                ListContent = from content in ListContent
                              where
                              ((content.As<TitlePart>().Title ?? "").Contains(expression, StringComparison.InvariantCultureIgnoreCase))
                              select content;
            IEnumerable<ContentIndexVM> listVM = ListContent.Select(p => new ContentIndexVM {
                Id = p.Id,
                Title = p.As<TitlePart>().Title,
                ModifiedUtc = p.As<CommonPart>().ModifiedUtc,
                UserName = p.As<CommonPart>().Owner.UserName
            });
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            dynamic pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(listVM.Count());
            var list = listVM.Skip(pager.GetStartIndex())
                                .Take(pager.PageSize);
            //_orchardServices.New.List();
            //list.AddRange(listVM.Skip(pager.GetStartIndex())
            //                    .Take(pager.PageSize)
            //                    );
            var model = new SearchIndexVM(list, search, pagerShape, Options);
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