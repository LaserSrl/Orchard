using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Laser.Orchard.Questionnaires.ViewModels {

    public class QuestStatViewModel {
        public string QuestionnaireTitle { get; set; }
        public int QuestionnairePart_Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public int Count { get; set; }
    }

    public class AnswerStatsViewModel {
        public string Answer { get; set; }
        public int Count { get; set; }
    }

    public class QuestionnaireStatsViewModel {
        public int QuestionnairePart_Id { get; set; }
        public string QuestionnaireTitle { get; set; }
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public QuestionType QuestionType { get; set; }
        public List<AnswerStatsViewModel> Answers { get; set; }
    }

    public class QuestionnaireStatsDetailViewModel {
        public dynamic Pager { get; set; }
        public QuestionnaireStatsViewModel AnswersStats { get; set; }
    }

    public class QuestionnaireSearchViewModel {
        public dynamic Pager { get; set; }
        public string SearchExpression { get; set; }
        public IEnumerable<ContentItem> Questionnaires { get; set; }
    }
}