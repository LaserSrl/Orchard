using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using Laser.Orchard.StartupConfig.Services;
using Orchard;
using Orchard.Captcha.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using Orchard.Security;
using Laser.Orchard.Questionnaires.Settings;
using Orchard.Core.Title.Models;
using Orchard.OutputCache.Filters;

namespace Laser.Orchard.Questionnaires.Drivers {
    public class QuestionnairePartDriver : ContentPartDriver<QuestionnairePart> {
        private readonly IQuestionnairesServices _questServices;
        private readonly IControllerContextAccessor _controllerContextAccessor;
        private readonly IOrchardServices _orchardServices;
        private readonly ICaptchaService _capthcaServices;
        private readonly ICurrentContentAccessor _currentContentAccessor;
        
        public QuestionnairePartDriver(IQuestionnairesServices questServices,
            IOrchardServices orchardServices,
            IControllerContextAccessor controllerContextAccessor,
            ICaptchaService capthcaServices,
            ICurrentContentAccessor currentContentAccessor) {
            _questServices = questServices;
            _orchardServices = orchardServices;
            _controllerContextAccessor = controllerContextAccessor;
            T = NullLocalizer.Instance;
            _capthcaServices = capthcaServices;
            _currentContentAccessor = currentContentAccessor;
        }

        public Localizer T { get; set; }
        protected override string Prefix {
            get {
                return "Questionnaire";
            }
        }
        protected override DriverResult Display(QuestionnairePart part, string displayType, dynamic shapeHelper) {
            if (displayType == "Summary")
                return ContentShape("Parts_Questionnaire_Summary",
                    () => shapeHelper.Parts_Questionnaire_Summary(
                        QuestionsCount: part.Questions.Count(c => c.Published)
                        ));
            if (displayType == "SummaryAdmin")
                return ContentShape("Parts_Questionnaire_SummaryAdmin",
                    () => shapeHelper.Parts_Questionnaire_SummaryAdmin(
                        QuestionsCount: part.Questions.Count(c => c.Published),
                        QuestionsTotalCount: part.Questions.Count()
                        ));
            var isAuthorized = (_orchardServices.Authorizer.Authorize(Permissions.SubmitQuestionnaire));
            if (isAuthorized) {
                var viewModel = _questServices.BuildViewModelWithResultsForQuestionnairePart(part); //Modello mappato senza risposte
                if (_controllerContextAccessor.Context != null) {
                    // valorizza il context
                    var currentCi = _currentContentAccessor.CurrentContentItem;
                    if ((currentCi != null) && currentCi.Has<TitlePart>()) {
                        viewModel.Context = currentCi.Get<TitlePart>().Title;
                    }
                    else {
                        viewModel.Context = _controllerContextAccessor.Context.HttpContext.Request.RawUrl;
                    }
                    // limita la lunghezza del context a 255 chars
                    if (viewModel.Context.Length > 255) {
                        viewModel.Context = viewModel.Context.Substring(0, 255);
                    }
                    // valorizza le altre proprietà del viewModel
                    var fullModelWithAnswers = _controllerContextAccessor.Context.Controller.TempData["QuestUpdatedEditModel"];
                    var hasAcceptedTerms = _controllerContextAccessor.Context.Controller.TempData["HasAcceptedTerms"];

                    if (fullModelWithAnswers != null) { // Mappo le risposte
                        var risposteModel = (QuestionnaireWithResultsViewModel)fullModelWithAnswers;
                        //Mappo l'oggetto principale per evitare che mi richieda di accettare le condizioni
                        viewModel.MustAcceptTerms = risposteModel.MustAcceptTerms;
                        viewModel.HasAcceptedTerms = risposteModel.HasAcceptedTerms;

                        for (var i = 0; i < viewModel.QuestionsWithResults.Count(); i++) {
                            switch (viewModel.QuestionsWithResults[i].QuestionType) {
                                case QuestionType.OpenAnswer:
                                    viewModel.QuestionsWithResults[i].OpenAnswerAnswerText = risposteModel.QuestionsWithResults[i].OpenAnswerAnswerText;
                                    break;
                                case QuestionType.SingleChoice:
                                    viewModel.QuestionsWithResults[i].SingleChoiceAnswer = risposteModel.QuestionsWithResults[i].SingleChoiceAnswer;
                                    break;
                                case QuestionType.MultiChoice:
                                    for (var j = 0; j < viewModel.QuestionsWithResults[i].AnswersWithResult.Count(); j++) {
                                        viewModel.QuestionsWithResults[i].AnswersWithResult[j].Answered = risposteModel.QuestionsWithResults[i].AnswersWithResult[j].Answered;
                                    }
                                    break;
                            }

                        }
                    }
                    else if (hasAcceptedTerms != null) { // l'utente ha appena accettato le condizionoi
                        viewModel.HasAcceptedTerms = (bool)_controllerContextAccessor.Context.Controller.TempData["HasAcceptedTerms"];
                    }
                }
                if (viewModel.UseRecaptcha) { // se è previsto un recaptcha creo l'html e il js del recaptcha
                    viewModel.CaptchaHtmlWidget = _capthcaServices.GenerateCaptcha();
                }

                //return ContentShape("Parts_Questionnaire_FrontEnd_Edit",
                //    () => shapeHelper.Parts_Questionnaire_FrontEnd_Edit(
                //            Questionnaire: viewModel
                //        ));

                return ContentShape("Parts_Questionnaire_FrontEnd_Edit",
                                 () => shapeHelper.Parts_Questionnaire_FrontEnd_Edit(
                                     Questionnaire: viewModel,
                                     Prefix: Prefix));
            }
            else {
                throw new OrchardSecurityException(T("You have to be logged in, before answering a questionnaire!"));
            }
            //return ContentShape("Parts_Questionnaire",
            //    () => shapeHelper.Parts_Questionnaire(
            //            Questions: viewModel.QuestionsWithResults
            //            ));
        }

        protected override DriverResult Editor(QuestionnairePart part, dynamic shapeHelper) {
            Int32 QuestionsLimitsNumber = part.Settings.GetModel<QuestionnairesPartSettingVM>().QuestionsLimitsNumber;
            Int32 QuestionsSortedRandomlyNumber = part.Settings.GetModel<QuestionnairesPartSettingVM>().QuestionsSortedRandomlyNumber;
            bool ShowCorrectResponseFlag = part.Settings.GetModel<QuestionnairesPartSettingVM>().ShowCorrectResponseFlag;
            _controllerContextAccessor.Context.Controller.TempData["ShowCorrectResponseFlag"] = ShowCorrectResponseFlag;
            _controllerContextAccessor.Context.Controller.TempData["QuestionsLimitsNumber"] = QuestionsLimitsNumber;
            _controllerContextAccessor.Context.Controller.TempData["AnswersLimitsNumber"] = part.Settings.GetModel<QuestionnairesPartSettingVM>().QuestionsResponseLimitsNumber;
            _controllerContextAccessor.Context.Controller.TempData["EnableQuestionImage"] = part.Settings.GetModel<QuestionnairesPartSettingVM>().EnableQuestionImage;
            _controllerContextAccessor.Context.Controller.TempData["EnableAnswerImage"] = part.Settings.GetModel<QuestionnairesPartSettingVM>().EnableAnswerImage;
            _controllerContextAccessor.Context.Controller.TempData["QuestionImageLimitNumber"] = part.Settings.GetModel<QuestionnairesPartSettingVM>().QuestionImageLimitNumber;
            _controllerContextAccessor.Context.Controller.TempData["AnswerImageLimitNumber"] = part.Settings.GetModel<QuestionnairesPartSettingVM>().AnswerImageLimitNumber;
            _controllerContextAccessor.Context.Controller.ViewBag.QuestionnairesPartSettings = part.Settings.GetModel<QuestionnairesPartSettingVM>();
            QuestionnaireEditModel modelForEdit;
            if (_controllerContextAccessor.Context.Controller.TempData["ModelWithErrors"] != null) {
                modelForEdit = (QuestionnaireEditModel)_controllerContextAccessor.Context.Controller.TempData["ModelWithErrors"];
            }
            else {
                modelForEdit = _questServices.BuildEditModelForQuestionnairePart(part);
            }
  
            _controllerContextAccessor.Context.Controller.TempData[Prefix + "ModelWithErrors"] = null;
            return ContentShape("Parts_Questionnaire_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/Questionnaire_Edit",
                                 Model: modelForEdit,
                                 Prefix: Prefix));
        }

        protected override DriverResult Editor(QuestionnairePart part, IUpdateModel updater, dynamic shapeHelper) {
            QuestionnaireEditModel editModel = new QuestionnaireEditModel();
            editModel = _questServices.BuildEditModelForQuestionnairePart(part);
            try {
                if (updater.TryUpdateModel(editModel, Prefix, null, null)) {
                    if (part.ContentItem.Id != 0) {
                        // se per caso part.Id è diversa dall'Id registrato nei record relazionati, arrivo da una traduzione, quindi devo trattare tutto come se fosse questions e answers nuove
                        foreach (var q in editModel.Questions) {

                            if (part.Id != q.QuestionnairePartRecord_Id) {
                                q.QuestionnairePartRecord_Id = part.Id;
                                q.Id = 0;
                            }

                            foreach (var a in q.Answers) {
                                if (q.Id == 0) {
                                    a.Id = 0;
                                }
                            }
                        }
                        try {
                            _questServices.UpdateForContentItem(
                                part.ContentItem, editModel);
                        }
                        catch (Exception ex) {
                            updater.AddModelError("QuestionnaireUpdateError", T("Cannot update questionnaire. " + ex.Message));
                            _controllerContextAccessor.Context.Controller.TempData[Prefix + "ModelWithErrors"] = editModel;
                        }
                    }

                }
                else {
                    updater.AddModelError("QuestionnaireUpdateError", T("Cannot update questionnaire"));
                    _controllerContextAccessor.Context.Controller.TempData[Prefix + "ModelWithErrors"] = editModel;
                }
            }
            catch (Exception ex) {
                updater.AddModelError("QuestionnaireUpdateError", T("Cannot update questionnaire....... " + ex.Message + ex.StackTrace));
                _controllerContextAccessor.Context.Controller.TempData[Prefix + "ModelWithErrors"] = editModel;
            }
            return Editor(part, shapeHelper);
        }


        #region [ Import/Export ]
        protected override void Exporting(QuestionnairePart part, ExportContentContext context) {

            var root = context.Element(part.PartDefinition.Name);
            XElement termsText = new XElement("TermsText");
            root.SetAttributeValue("MustAcceptTerms", part.MustAcceptTerms);
            root.SetAttributeValue("UseRecaptcha", part.UseRecaptcha);
            termsText.SetValue(part.TermsText ?? "");
            root.Add(termsText);
            foreach (var q in part.Questions) {
                XElement question = new XElement("Question");
                question.SetAttributeValue("OriginalId", q.Id);
                question.SetAttributeValue("Position", q.Position);
                question.SetAttributeValue("Published", q.Published);
                question.SetAttributeValue("Question", q.Question);
                question.SetAttributeValue("QuestionType", q.QuestionType);
                question.SetAttributeValue("AnswerType", q.AnswerType);
                question.SetAttributeValue("Section", q.Section);
                question.SetAttributeValue("IsRequired", q.IsRequired);
                question.SetAttributeValue("Condition", q.Condition);
                question.SetAttributeValue("ConditionType", q.ConditionType);
                question.SetAttributeValue("AllFiles", q.AllFiles);
                foreach (var a in q.Answers) {
                    XElement answer = new XElement("Answer");
                    answer.SetAttributeValue("OriginalId", a.Id);
                    answer.SetAttributeValue("Position", a.Position);
                    answer.SetAttributeValue("Published", a.Published);
                    answer.SetAttributeValue("Answer", a.Answer);
                    answer.SetAttributeValue("AllFiles", a.AllFiles);
                    answer.SetAttributeValue("CorrectResponse", a.CorrectResponse);
                    question.Add(answer);
                }
                root.Add(question);
            }
        }

        protected override void Importing(QuestionnairePart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            var questions = context.Data.Element(part.PartDefinition.Name).Elements("Question");
            var editModel = _questServices.BuildEditModelForQuestionnairePart(part);
            editModel.MustAcceptTerms = bool.Parse(root.Attribute("MustAcceptTerms").Value);
            editModel.UseRecaptcha = bool.Parse(root.Attribute("UseRecaptcha").Value);
            editModel.TermsText = root.Element("TermsText").Value;

            var questionModelList = new List<QuestionEditModel>();
            foreach (var q in questions) { // recupero le questions
                var answers = q.Elements("Answer");
                var answerModelList = new List<AnswerEditModel>();
                foreach (var a in answers) { // recupero le answers
                    var answerEditModel = new AnswerEditModel {
                        Position = int.Parse(a.Attribute("Position").Value),
                        Published = bool.Parse(a.Attribute("Published").Value),
                        Answer = a.Attribute("Answer").Value,
                        OriginalId = int.Parse(a.Attribute("OriginalId").Value),
                        CorrectResponse = bool.Parse(a.Attribute("CorrectResponse").Value),
                        AllFiles = a.Attribute("AllFiles")!=null?a.Attribute("AllFiles").Value:null,
                    };
                    answerModelList.Add(answerEditModel);
                }
                var questionEditModel = new QuestionEditModel {
                    Position = int.Parse(q.Attribute("Position").Value),
                    Published = bool.Parse(q.Attribute("Published").Value),
                    Question = q.Attribute("Question").Value,
                    Section = q.Attribute("Section") != null ? q.Attribute("Section").Value : null,
                    QuestionType = (QuestionType)Enum.Parse(typeof(QuestionType), q.Attribute("QuestionType").Value),
                    AnswerType = (AnswerType)Enum.Parse(typeof(AnswerType), q.Attribute("AnswerType").Value),
                    IsRequired = bool.Parse(q.Attribute("IsRequired").Value),
                    QuestionnairePartRecord_Id = part.Id,
                    Answers = answerModelList,
                    Condition = q.Attribute("Condition") == null ? null : q.Attribute("Condition").Value,
                    ConditionType = (ConditionType)Enum.Parse(typeof(ConditionType), q.Attribute("ConditionType").Value),
                    OriginalId = int.Parse(q.Attribute("OriginalId").Value),
                    AllFiles = q.Attribute("AllFiles") != null ? q.Attribute("AllFiles").Value : null,
                };
                questionModelList.Add(questionEditModel);
            }
            editModel.Questions = questionModelList; // metto tutto nel model 
            _questServices.UpdateForContentItem(
                    part.ContentItem, editModel); //aggiorno
        }
        #endregion

        ///// <summary>
        ///// Se il ContentItem corrente contiene una QuestionnairePart, invalida di fatto la cache.
        ///// </summary>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public System.Text.StringBuilder InflatingCacheKey(System.Text.StringBuilder key) {
        //    var part = _currentContentAccessor.CurrentContentItem.As<QuestionnairePart>();
        //    if (part != null) {
        //        key.AppendFormat("sid={0};rnd={1};", HttpContext.Current.Session.SessionID, new Random().Next(1000000));
        //    }
        //    return key;
        //}
    }
}