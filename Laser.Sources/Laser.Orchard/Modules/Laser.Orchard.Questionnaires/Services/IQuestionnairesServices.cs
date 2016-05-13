using System;
using System.Collections.Generic;
using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Tasks.Scheduling;
namespace Laser.Orchard.Questionnaires.Services {
    public interface IQuestionnairesServices : IDependency {
        void UpdateForContentItem(ContentItem item, QuestionnaireEditModel partEditModel);
        QuestionnaireEditModel BuildEditModelForQuestionnairePart(QuestionnairePart part);
        QuestionnaireViewModel BuildViewModelForQuestionnairePart(QuestionnairePart part);
        QuestionnaireWithResultsViewModel BuildViewModelWithResultsForQuestionnairePart(QuestionnairePart part);
        void CreateUserAnswers(UserAnswersRecord answerRecord);
        AnswerRecord GetAnswer(int id);
        List<QuestionnaireStatsViewModel> GetStats(int questionnaireId);
        List<QuestStatViewModel> GetStats(QuestionType type);
        void Save(QuestionnaireWithResultsViewModel editModel, IUser currentUser,string SessionID);
        //bool SendTemplatedEmailRanking(bool multipleRankings);
        bool SendTemplatedEmailRanking();
        bool SendTemplatedEmailRanking(Int32 gameID);
    }
}
