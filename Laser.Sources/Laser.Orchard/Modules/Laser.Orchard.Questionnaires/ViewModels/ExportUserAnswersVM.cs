using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class ExportUserAnswersVM {
        public string UserName { get; set; }
        public DateTime AnswerDate { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}