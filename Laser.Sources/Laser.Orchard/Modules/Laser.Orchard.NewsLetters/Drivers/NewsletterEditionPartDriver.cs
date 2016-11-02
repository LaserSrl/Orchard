using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Laser.Orchard.NewsLetters.Models;
using Laser.Orchard.NewsLetters.Services;
using Laser.Orchard.NewsLetters.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Title.Models;
using Orchard.Localization;

namespace Laser.Orchard.NewsLetters.Drivers {
    public class NewsletterEditionPartDriver : ContentPartDriver<NewsletterEditionPart> {
        private readonly INewsletterServices _newslServices;
        private readonly IOrchardServices _orchardServices;
        private readonly RequestContext _requestContext;

        public NewsletterEditionPartDriver(IOrchardServices orchardServices, RequestContext requestContext, INewsletterServices newsletterDefinitionServices) {
            _newslServices = newsletterDefinitionServices;
            _orchardServices = orchardServices;
            _requestContext = requestContext;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "NewsLetters"; }
        }

        protected override DriverResult Display(NewsletterEditionPart part, string displayType, dynamic shapeHelper) {
            if (displayType == "SummaryAdmin") {
                return ContentShape("Parts_NewsletterEdition_SummaryAdmin",
                        () => shapeHelper.Parts_NewsletterDefinition_SummaryAdmin());
            }
            if (displayType == "Summary") {
                return ContentShape("Parts_NewsletterEdition_Summary",
                        () => shapeHelper.Parts_NewsletterDefinition_Summary());
            }
            if (displayType == "Detail") {
                return ContentShape("Parts_NewsletterEdition",
                        () => shapeHelper.Parts_NewsletterDefinition());
            }
            return null;
        }
        protected override DriverResult Editor(NewsletterEditionPart part, dynamic shapeHelper) {
            int newsId = 0;
            int.TryParse(_requestContext.HttpContext.Request.RequestContext.RouteData.Values["newsletterId"].ToString(), out newsId);
            int[] selectedAnnouncementsIds = !String.IsNullOrWhiteSpace(part.AnnouncementIds) ? part.AnnouncementIds.Split(',').Select(s => Convert.ToInt32(s)).ToArray() : null;

            part.NewsletterDefinitionPartRecord_Id = newsId;
            var model = new NewsletterEditionViewModel {
                NewsletterEditionPart = part,
                AnnouncementToAttach = part.Dispatched ?
                _newslServices.GetAnnouncements(selectedAnnouncementsIds).Select(s => new AnnouncementLookupViewModel {
                    Title = !String.IsNullOrWhiteSpace(s.AnnouncementTitle) ? s.AnnouncementTitle : s.ContentItem.As<TitlePart>().Title,
                    Id = s.Id,
                    Selected = true,
                }).ToList() :
                _newslServices.GetNextAnnouncements((int)newsId, selectedAnnouncementsIds)
            };
            var shapes = new List<DriverResult>();
            shapes.Add(ContentShape("Parts_NewsletterEdition_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/NewsletterEdition_Edit",
                                 Model: model,
                                 Prefix: Prefix)));
            shapes.Add(ContentShape("Parts_SendNewsletterButton",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts/SendNewsletterButton",
                                 Model: part,
                                 Prefix: Prefix)));

            return new CombinedResult(shapes);
        }

        protected override DriverResult Editor(NewsletterEditionPart part, IUpdateModel updater, dynamic shapeHelper) {
            int newsId = 0;
            int.TryParse(_requestContext.HttpContext.Request.RequestContext.RouteData.Values["newsletterId"].ToString(), out newsId);
            int[] selectedAnnouncementsIds = !String.IsNullOrWhiteSpace(part.AnnouncementIds) ? part.AnnouncementIds.Split(',').Select(s => Convert.ToInt32(s)).ToArray() : null;
            var model = new NewsletterEditionViewModel {
                NewsletterEditionPart = part,
                AnnouncementToAttach = part.Dispatched ? null : _newslServices.GetNextAnnouncements((int)newsId, selectedAnnouncementsIds)
            };
            if (!updater.TryUpdateModel(model, Prefix, null, null)) {
                updater.AddModelError("NewsletterEditionPartError", T("NewsletterEdition Error"));
            }
            if (!part.Dispatched) {
                part.AnnouncementIds = String.Join(",", model.AnnouncementToAttach.Where(w => w.Selected).Select(s => s.Id));
            } else {
                updater.AddModelError("NewsletterEditionPartError", T("Dispatched newsletter can't be updated!"));
            }

            return Editor(part, shapeHelper);
        }


        #region [ Import/Export ]
        protected override void Exporting(NewsletterEditionPart part, ExportContentContext context) {

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

        protected override void Importing(NewsletterEditionPart part, ImportContentContext context) {
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