using Laser.Orchard.Questionnaires.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;


namespace Laser.Orchard.Questionnaires.ViewModels {

    public class GamePartVM {

        public GamePartVM() {
            MyOrder = GetMaxOrderNumber();
            EmailToSendTemplatedRanking = GetDefaultEmail();
        }

        [Required]
        public string AbstractText { get; set; }

        [Required]
        public string GameDate { get; set; }

        [Required]
        public string RankingIOSIdentifier { get; set; }

        [Required]
        public string RankingAndroidIdentifier { get; set; }

        [Required]
        public Int32 MyOrder { get; set; }

        public string EmailToSendTemplatedRanking { get; set; }

        private int GetMaxOrderNumber() {
            //TODO
            return 1;
        }

        private string GetDefaultEmail() {
            //TODO
            return "a@a.it";
        }

        public bool workflowfired { get; set; }

        public Int32 QuestionsSortedRandomlyNumber { get; set; }

        public bool RandomResponse { get; set; }

        public Decimal AnswerPoint { get; set; }

        public Decimal AnswerTime { get; set; }

        public int State { get; set; }

        public GameType GameType { get; set; }

        public SelectList ListOfGameType {
            get {
                SelectList enumToList = new SelectList(Enum.GetValues(typeof(GameType)).Cast<GameType>().Select(v => new SelectListItem {
                    Text = v.ToString(),
                    Value = v.ToString()
                }).ToList(), "Value", "Text");
                List<SelectListItem> _list = enumToList.ToList();
              //  _list.Insert(0, new SelectListItem() { Value = "All", Text = "All" });
                return new SelectList((IEnumerable<SelectListItem>)_list, "Value", "Text");
            }
        }
    }

    //matteo.piovanelli : 2016/05/10 : add class used in creating an Index of games
    public class GamePartSearchViewModel {
        public dynamic Pager { get; set; }
        public string SearchExpression { get; set; }
        public IEnumerable<ContentItem> GameParts { get; set; }
    }
}