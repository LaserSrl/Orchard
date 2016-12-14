using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization;
using OProjections = Orchard.Projections;
using Orchard;
using Orchard.Data;
using Laser.Orchard.Questionnaires.Models;
using Orchard.ContentManagement;

namespace Laser.Orchard.Questionnaires.Handlers {
    public class QuestionnaireNotAnsweredFilter : OProjections.Services.IFilterProvider {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<UserAnswersRecord> _userAnswersRecord;
        public Localizer T { get; set; }

        public QuestionnaireNotAnsweredFilter(IOrchardServices orchardServices, IRepository<UserAnswersRecord> userAnswersRecord) {
            _orchardServices = orchardServices;
            _userAnswersRecord = userAnswersRecord;
            T = NullLocalizer.Instance;
        }
        public void Describe(OProjections.Descriptors.Filter.DescribeFilterContext describe) {
            describe.For("Questionnaire", T("Questionnaire"), T("Questionnaire"))
                .Element("Questionnaire Not Answered Filter", T("Specific questionnaire without response for current User"), T("Filter for specific questionnaire without response for current User."),
                    ApplyFilter,
                    DisplayFilter,
                    "QuestionnaireNotAnsweredFilterForm"
                );
        }
        public LocalizedString DisplayFilter(OProjections.Descriptors.Filter.FilterContext context) {
            return T("Only unanswered Questionnaire for current user");
        }
        public void ApplyFilter(OProjections.Descriptors.Filter.FilterContext context) {
            // recupera il context da un campo tokenized
            var aux = (string)(context.State.QuestionnaireId);
            var questionnaireId = string.IsNullOrWhiteSpace(aux) ? 0 : Convert.ToInt32(aux);
            var currentUser = _orchardServices.WorkContext.CurrentUser;
            if (currentUser != null) {
                List<Int32> elencoIdQuestionnaires = _userAnswersRecord.Fetch(x => x.User_Id == currentUser.Id && x.QuestionnairePartRecord_Id == questionnaireId).Select(y => y.QuestionnairePartRecord_Id).Distinct().ToList();
                if (elencoIdQuestionnaires.Count == 0) {
                    elencoIdQuestionnaires.Add(0);
                }
                Action<IAliasFactory> selector =
                    alias => alias.ContentPartRecord<QuestionnairePartRecord>();
                if (questionnaireId != 0) {
                    Action<IHqlExpressionFactory> filter = x => x.Eq("Id", questionnaireId);
                    context.Query.Where(selector, filter);
                }
                foreach (var id in elencoIdQuestionnaires) {
                    var termId = id;
                    Action<IHqlExpressionFactory> filter = x => x.NotEqProperty("Id", id.ToString());
                    context.Query.Where(selector, filter);
                }
            }
            else {
                // non estrae nulla: la condizione seguente è sempre falsa
                Action<IAliasFactory> selector =
                       alias => alias.ContentPartRecord<QuestionnairePartRecord>();
                Action<IHqlExpressionFactory> filter = x => x.Eq("Id", -100);
                context.Query.Where(selector, filter);
            }
        }
    }
}