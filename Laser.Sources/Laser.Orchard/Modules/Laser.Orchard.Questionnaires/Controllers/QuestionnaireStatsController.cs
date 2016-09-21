using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System;
using System.Globalization;
using Orchard.Tasks.Scheduling;

namespace Laser.Orchard.Questionnaires.Controllers {
    public class QuestionnaireStatsController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly IQuestionnairesServices _questionnairesServices;
        private readonly IScheduledTaskManager _taskManager;

        public QuestionnaireStatsController(IOrchardServices orchardServices, IQuestionnairesServices questionnairesServices, IScheduledTaskManager taskManager) {
            _orchardServices = orchardServices;
            _questionnairesServices = questionnairesServices;
            _taskManager = taskManager;
        }

        [HttpGet]
        [Admin]
        public ActionResult QuestionDetail(int idQuestionario, int idDomanda, int? page, int? pageSize, string from = null, string to = null) {
            DateTime fromDate, toDate;
            CultureInfo provider = CultureInfo.GetCultureInfo(_orchardServices.WorkContext.CurrentCulture);
            DateTime.TryParse(from, provider, DateTimeStyles.None, out fromDate);
            DateTime.TryParse(to, provider, DateTimeStyles.None, out toDate);

            var stats = _questionnairesServices.GetStats(idQuestionario, (DateTime?)fromDate, (DateTime?)toDate).Where(x => x.QuestionId == idDomanda).FirstOrDefault();

            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, new PagerParameters { Page = page, PageSize = pageSize });
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(stats.Answers.Count());
            var pageOfAnswers = stats.Answers.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            stats.Answers = pageOfAnswers.OrderByDescending(o => o.Count).ThenBy(o => o.Answer).ToList();

            QuestionnaireStatsDetailViewModel model = new QuestionnaireStatsDetailViewModel();
            model.AnswersStats = stats;
            model.Pager = pagerShape;

            return View((object)model);
        }

        [HttpGet]
        [Admin]
        public ActionResult Detail(int idQuestionario, string from = null, string to = null, bool export = false) {
            DateTime fromDate, toDate;
            CultureInfo provider = CultureInfo.GetCultureInfo(_orchardServices.WorkContext.CurrentCulture);
            DateTime.TryParse(from, provider, DateTimeStyles.None, out fromDate);
            DateTime.TryParse(to, provider, DateTimeStyles.None, out toDate);
            var model = _questionnairesServices.GetStats(idQuestionario, (DateTime?)fromDate, (DateTime?)toDate);
            if (export == true) {
                ViewBag.Msg = "Export in corso...";
            }
            return View((object)model);
        }

        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, string searchExpression) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessStatistics))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, searchExpression);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, string searchExpression) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessStatistics))
                return new HttpUnauthorizedResult();

            IContentQuery<ContentItem> contentQuery = _orchardServices.ContentManager.Query()
                                                                                     .ForType("Questionnaire")
                                                                                     .OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc);
            if (!string.IsNullOrEmpty(searchExpression))
                contentQuery = contentQuery.Where<TitlePartRecord>(w => w.Title.Contains(searchExpression));

            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(contentQuery.Count());
            var pageOfContentItems = contentQuery.Slice(pager.GetStartIndex(), pager.PageSize);

            var model = new QuestionnaireSearchViewModel();
            model.Pager = pagerShape;
            model.Questionnaires = pageOfContentItems;

            return View((object)model);
        }
    }
}