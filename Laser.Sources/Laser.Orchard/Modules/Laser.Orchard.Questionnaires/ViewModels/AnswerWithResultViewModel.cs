using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.ViewModels {
    public class AnswerWithResultViewModel : AnswerViewModel {
        public bool Answered { get; set; }
        public string AnswerText { get; set; }
    }
}