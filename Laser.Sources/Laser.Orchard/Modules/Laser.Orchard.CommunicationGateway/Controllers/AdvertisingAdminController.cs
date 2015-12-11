
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.MediaLibrary.Models;
using Laser.Orchard.CommunicationGateway.Models;
using System.Net;
using System.IO;
using System.Dynamic;

namespace Laser.Orchard.CommunicationGateway.Controllers {
    public class AdvertisingAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly string contentType = "CommunicationAdvertising";
        private readonly dynamic TestPermission = Permissions.ManageCommunicationAdv;
        private readonly INotifier _notifier;
        private Localizer T { get; set; }

        public AdvertisingAdminController(
            IOrchardServices orchardServices,
            INotifier notifier,
            IContentManager contentManager) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }




        [Admin]
        public ActionResult Edit(int id, int idCampaign = 0) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            object model;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                if (idCampaign > 0) {
                    List<int> lids = new List<int>();
                    lids.Add(idCampaign);
                    ((dynamic)newContent).CommunicationAdvertisingPart.Campaign.Ids = lids.ToArray();
                }
                //  model = _orchardServices.ContentManager.BuildEditor(newContent);
                //   _contentManager.Create(newContent);
                model = _contentManager.BuildEditor(newContent);
            } else
                model = _contentManager.BuildEditor(_orchardServices.ContentManager.Get(id, VersionOptions.Latest));
            return View((object)model);
        }


        [HttpPost, ActionName("Edit"), Admin]
        public ActionResult EditPOST(int id, int idCampaign = 0) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();

            ContentItem content;
            if (id == 0) {
                var newContent = _orchardServices.ContentManager.New(contentType);
                _orchardServices.ContentManager.Create(newContent, VersionOptions.Draft);
                content = newContent;
            } else
                content = _orchardServices.ContentManager.Get(id, VersionOptions.Latest);
            var model = _orchardServices.ContentManager.UpdateEditor(content, this);
            if (idCampaign > 0) {
                List<int> lids = new List<int>();
                lids.Add(idCampaign);
                ((dynamic)content).CommunicationAdvertisingPart.Campaign.Ids = lids.ToArray();
            }
            if (!ModelState.IsValid) {
                foreach (string key in ModelState.Keys) {
                    if (ModelState[key].Errors.Count > 0)
                        foreach (var error in ModelState[key].Errors)
                            _notifier.Add(NotifyType.Error, T(error.ErrorMessage));
                }
                _orchardServices.TransactionManager.Cancel();
                return View(model);
            }
            _contentManager.Unpublish(content);
            _notifier.Add(NotifyType.Information, T("Advertising saved"));
            if (Request.Form["submit.Publish"] == "submit.Publish") {
                // _contentManager.Unpublish(content);
                _contentManager.Publish(content);
                //  _contentManager.Unpublish(content); // inserito per permettere il publishlater

            }
            return RedirectToAction("Index", "AdvertisingAdmin", new { id = idCampaign });
        }


        [HttpPost]
        [Admin]
        public ActionResult Remove(Int32 id, int idCampaign = 0) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            ContentItem content = _orchardServices.ContentManager.Get(id);
            _orchardServices.ContentManager.Remove(content);
            return RedirectToAction("Index", "AdvertisingAdmin", new { id = idCampaign });
        }

        [HttpGet]
        [Admin]
        public ActionResult Index(int id, int? page, int? pageSize, SearchVM search) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, search, id);
        }



        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, SearchVM search, int id = 0) {
            if (!_orchardServices.Authorizer.Authorize(TestPermission))
                return new HttpUnauthorizedResult();
            dynamic Options = new System.Dynamic.ExpandoObject();
            if (id >= 0)
                Options.Campaign = _orchardServices.ContentManager.Get(id);
            else {
                // Options.Campaign = ""; // devo inserire la proprietà Campaign altrimenti index va in exception
                Options.Campaign = new System.Dynamic.ExpandoObject();
                Options.Campaign.Id = id;
            }
            var expression = search.Expression;
            IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query(VersionOptions.Latest).ForType(contentType).OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc);
            IEnumerable<ContentItem> ListContent;
            /*Nel caso di flash advertising la campagna è -10, quindi il filtro è sempre valido.*/
            if (id > 0)
                ListContent = contentQuery.List().Where(x => ((int[])((dynamic)x).CommunicationAdvertisingPart.Campaign.Ids).Contains(id));
            else
                ListContent = contentQuery.List().Where(x => ((dynamic)x).CommunicationAdvertisingPart.Campaign.Ids.Length == 0);
            if (!string.IsNullOrEmpty(search.Expression))
                ListContent = from content in ListContent
                              where
                              ((content.As<TitlePart>().Title ?? "").Contains(expression, StringComparison.InvariantCultureIgnoreCase))
                              select content;
            IEnumerable<ContentIndexVM> listVM = ListContent.Select(p => new ContentIndexVM {
                Id = p.Id,
                Title = p.As<TitlePart>().Title,
                ModifiedUtc = p.As<CommonPart>().ModifiedUtc,
                UserName = p.As<CommonPart>().Owner.UserName,
                //        Option = p.As<FacebookPostPart>().FacebookMessageSent
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