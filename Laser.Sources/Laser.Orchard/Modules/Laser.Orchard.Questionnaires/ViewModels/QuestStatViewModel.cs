using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class QuestStatViewModel {
        public string QuestionnaireTitle { get; set; }
        public int QuestionnairePart_Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public int Count { get; set; }
    }
}