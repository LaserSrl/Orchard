using AutoMapper;
using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.ViewModels;
using Laser.Orchard.StartupConfig.Localization;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.ContentManagement.Handlers;
using Orchard.Tasks.Scheduling;
using System;
using System.Globalization;

namespace Laser.Orchard.Questionnaires.Drivers {

    public class GamePartDriver : ContentPartDriver<GamePart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IDateLocalization _dateLocalization;
        private readonly IScheduledTaskManager _taskManager;

        public Localizer T { get; set; }

        protected override string Prefix {
            get { return "Laser.Mobile.Questionnaires.Game"; }
        }

        public GamePartDriver(IOrchardServices orchardServices, IDateLocalization dateLocalization,
            IScheduledTaskManager taskManager) {
            _orchardServices = orchardServices;
            _dateLocalization = dateLocalization;
            _taskManager = taskManager;
        }

        protected override DriverResult Editor(GamePart part, dynamic shapeHelper) {
            var viewModel = new GamePartVM();

            DateTime? tmpGameDate = _dateLocalization.ReadDateLocalized(part.GameDate);
            Mapper.CreateMap<GamePart, GamePartVM>()
                .ForMember(dest => dest.GameDate, opt => opt.Ignore());
            Mapper.Map(part, viewModel);
            viewModel.GameDate = _dateLocalization.WriteDateLocalized(tmpGameDate);
            return ContentShape("Parts_GamePart_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts/GamePart_Edit", Model: viewModel, Prefix: Prefix));
        }

        protected override DriverResult Editor(GamePart part, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new GamePartVM();
            if (updater.TryUpdateModel(viewModel, Prefix, null, null)) {
                Mapper.CreateMap<GamePartVM, GamePart>()
                    .ForMember(dest => dest.GameDate, opt => opt.Ignore());
                Mapper.Map(viewModel, part);
                if (!String.IsNullOrWhiteSpace(viewModel.GameDate)) {
                    part.GameDate = _dateLocalization.StringToDatetime(viewModel.GameDate, "") ?? DateTime.Now;
                }
            }
            //Schedule a task to send an email at the end of the game
            DateTime timeGameEnd = ((dynamic)part.ContentItem).ActivityPart.DateTimeEnd;
            //do we need to check whther timeGameEnd > DateTime.Now?
            Int32 thisGameID = part.Record.Id;
            //Check whether we already have a task for this game
            string taskTypeStr = Laser.Orchard.Questionnaires.Handlers.ScheduledTaskHandler.TaskType + " " + thisGameID.ToString();
            var tasks = _taskManager.GetTasks(taskTypeStr);
            foreach (var ta in tasks) {
                //if we are here, it means the task ta exists with the same game id as the current game
                //hence we should update the task.
                _taskManager.DeleteTasks(ta.ContentItem); //maybe
            }
            DateTime taskDate = timeGameEnd.AddMinutes(5);
            _taskManager.CreateTask(taskTypeStr, taskDate, null);

            return Editor(part, shapeHelper);
        }

        protected override void Importing(GamePart part, ImportContentContext context) {
            var root = context.Data.Element(part.PartDefinition.Name);
            part.AbstractText = root.Attribute("AbstractText").Value;
            part.AnswerPoint = Decimal.Parse(root.Attribute("AnswerPoint").Value, CultureInfo.InvariantCulture);
            part.AnswerTime = Decimal.Parse(root.Attribute("AnswerTime").Value, CultureInfo.InvariantCulture);
            part.GameDate = DateTime.Parse(root.Attribute("GameDate").Value, CultureInfo.InvariantCulture);
            part.GameType = ((GameType)Enum.Parse(typeof(GameType), root.Attribute("GameType").Value));
            part.MyOrder = Int32.Parse(root.Attribute("MyOrder").Value);
            part.QuestionsSortedRandomlyNumber = Int32.Parse(root.Attribute("QuestionsSortedRandomlyNumber").Value);
            part.RandomResponse = Boolean.Parse(root.Attribute("RandomResponse").Value);
            part.RankingAndroidIdentifier = root.Attribute("RankingAndroidIdentifier").Value;
            part.RankingIOSIdentifier = root.Attribute("RankingIOSIdentifier").Value;
        }

        protected override void Exporting(GamePart part, ExportContentContext context) {
            var root = context.Element(part.PartDefinition.Name);
            root.SetAttributeValue("AbstractText", part.AbstractText);
            root.SetAttributeValue("AnswerPoint", part.AnswerPoint.ToString(CultureInfo.InvariantCulture));
            root.SetAttributeValue("AnswerTime", part.AnswerTime.ToString(CultureInfo.InvariantCulture));
            root.SetAttributeValue("GameDate", part.GameDate.ToString(CultureInfo.InvariantCulture));
            root.SetAttributeValue("GameType", part.GameType);
            root.SetAttributeValue("MyOrder", part.MyOrder);
            root.SetAttributeValue("QuestionsSortedRandomlyNumber", part.QuestionsSortedRandomlyNumber);
            root.SetAttributeValue("RandomResponse", part.RandomResponse);
            root.SetAttributeValue("RankingAndroidIdentifier", part.RankingAndroidIdentifier);
            root.SetAttributeValue("RankingIOSIdentifier",      part.RankingIOSIdentifier);
        }
    }
}