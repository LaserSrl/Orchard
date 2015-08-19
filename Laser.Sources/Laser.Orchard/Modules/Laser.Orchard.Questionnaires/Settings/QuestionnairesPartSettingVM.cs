using System;

namespace Laser.Orchard.Questionnaires.Settings {

    public class QuestionnairesPartSettingVM {

        public Int32 QuestionsLimitsNumber { get; set; }

        public Int32 QuestionsSortedRandomlyNumber { get; set; }

        public Int32 QuestionsResponseLimitsNumber { get; set; }

        public bool ShowCorrectResponseFlag { get; set; }

        public bool EnableQuestionImage { get; set; }

        public bool EnableAnswerImage { get; set; }

        public Int32 QuestionImageLimitNumber { get; set; }

        public Int32 AnswerImageLimitNumber { get; set; }

        public bool RandomResponse { get; set; }

        public QuestionnairesPartSettingVM() {
            this.QuestionsLimitsNumber = 0;
            this.QuestionsSortedRandomlyNumber = 0;
            this.QuestionsResponseLimitsNumber = 0;
            this.ShowCorrectResponseFlag = false;
            this.RandomResponse = false;
            this.EnableQuestionImage = false;
            this.EnableAnswerImage = false;
            this.QuestionsResponseLimitsNumber = 0;
            this.AnswerImageLimitNumber = 0;
        }
    }
}