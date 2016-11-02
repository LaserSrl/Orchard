using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Laser.Orchard.NewsLetters.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Laser.Orchard.NewsLetters.Drivers {
    public class AnnouncementPartDriver : ContentPartDriver<AnnouncementPart> {
        public AnnouncementPartDriver() {
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "NewsLetters"; }
        }

        protected override DriverResult Display(AnnouncementPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_Announcement_SummaryAdmin",
                        () => shapeHelper.Parts_NewsletterDefinition_SummaryAdmin());
            }
            if (displayType == "Summary") {
                return ContentShape("Parts_Announcement_Summary",
                        () => shapeHelper.Parts_NewsletterDefinition_Summary());
            }
            if (displayType == "Detail") {
                return ContentShape("Parts_Announcement",
                        () => shapeHelper.Parts_NewsletterDefinition());
            }
            return null;
        }
        protected override DriverResult Editor(AnnouncementPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Announcement_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/Announcement_Edit",
                                 Model: part,
                                 Prefix: Prefix));
        }

        protected override DriverResult Editor(AnnouncementPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (!updater.TryUpdateModel(part, Prefix, null, null)) {
                updater.AddModelError("AnnouncementPartError", T("Announcement Error"));
            }
            var selectedNews = HttpContext.Current.Request.Form[Prefix + ".AnnouncementPart_Newsletters_Ids"];
            selectedNews = String.Join(",", selectedNews.Split(',').Where(w => w != "false"));
            part.AttachToNextNewsletterIds = selectedNews;
            return Editor(part, shapeHelper);
        }


        #region [ Import/Export ]
        protected override void Exporting(AnnouncementPart part, ExportContentContext context) {

            //foreach (var q in part.Questions) {
            //    XElement question = new XElement("Question");
            //    question.SetAttributeValue("Position", q.Position);
            //    question.SetAttributeValue("Published", q.Published);
            //    question.SetAttributeValue("Question", q.Question);
            //    question.SetAttributeValue("QuestionType", q.QuestionType);
            //    foreach (var a in q.Answers) {
            //        XElement answer = new XElement("Answer");
            //        answer.SetAttributeValue("Position", a.Position);
            //        answer.SetAttributeValue("Published", a.Published);
            //        answer.SetAttributeValue("Answer", a.Answer);
            //        question.Add(answer);
            //    }
            //    context.Element(part.PartDefinition.Name).Add(question);
            //}
        }

        protected override void Importing(AnnouncementPart part, ImportContentContext context) {
            //var questions = context.Data.Element(part.PartDefinition.Name).Elements("Question");
            //var editModel = _questServices.BuildEditModelForQuestionnairePart(part);
            //var questionModelList = new List<QuestionEditModel>();
            //foreach (var q in questions) { // recupero le questions
            //    var answers = q.Elements("Answer");
            //    var answerModelList = new List<AnswerEditModel>();
            //    foreach (var a in answers) { // recupero le answers
            //        var answerEditModel = new AnswerEditModel {
            //            Position = int.Parse(a.Attribute("Position").Value),
            //            Published = bool.Parse(a.Attribute("Published").Value),
            //            Answer = a.Attribute("Answer").Value,
            //        };
            //        answerModelList.Add(answerEditModel);
            //    }
            //    var questionEditModel = new QuestionEditModel {
            //        Position = int.Parse(q.Attribute("Position").Value),
            //        Published = bool.Parse(q.Attribute("Published").Value),
            //        Question = q.Attribute("Question").Value,
            //        QuestionType = (QuestionType)Enum.Parse(typeof(QuestionType), q.Attribute("QuestionType").Value),
            //        QuestionnairePartRecord_Id = part.Id,
            //        Answers = answerModelList
            //    };
            //    questionModelList.Add(questionEditModel);
            //}
            //editModel.Questions = questionModelList; // metto tutto nel model 
            //_questServices.UpdateForContentItem(
            //        part.ContentItem, editModel); //aggiorno
        }
        #endregion

    }
}