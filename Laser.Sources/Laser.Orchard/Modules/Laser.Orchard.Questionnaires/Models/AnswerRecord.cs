using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laser.Orchard.Questionnaires.Models {
    public class AnswerRecord {
        public virtual int Id { get; set; }
        public virtual string Answer { get; set; }
        public virtual bool Published { get; set; }
        public virtual int Position { get; set; }
        // foreign models
        public virtual int QuestionRecord_Id { get; set; }

        public virtual bool CorrectResponse { get; set; }
        public virtual string AllFiles { get; set; }
    }
}