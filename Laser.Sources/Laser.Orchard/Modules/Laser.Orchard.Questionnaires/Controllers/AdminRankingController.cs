using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace Laser.Orchard.Questionnaires.Controllers {
    public class AdminRankingController : Controller {
        private readonly IOrchardServices _orchardServices;
        //metti come parametro del costruttore IQuestionnairesServices questionnairesServices e
        //orchard si occupa di andare ad iniettarlo in maniera corretta, quindi posso poi fare
        //un bottone per mandarmi una mail
        private readonly IQuestionnairesServices _questionnairesServices;
        private readonly IRepository<RankingPartRecord> _repoRanking;
        private readonly ISessionLocator _sessionLocator;
        public AdminRankingController(IOrchardServices orchardServices, IQuestionnairesServices questionnairesServices, 
            IRepository<RankingPartRecord> repoRanking, ISessionLocator sessionLocator) {
            _orchardServices = orchardServices;
            _questionnairesServices = questionnairesServices;
            _repoRanking = repoRanking;
            _sessionLocator = sessionLocator;
        }

        [Admin]
        public ActionResult TestEmail(Int32 ID) {
            _questionnairesServices.SendTemplatedEmailRanking(ID);
            return RedirectToAction("Index");
        }
      
        [Admin]
        public ActionResult GetList() {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                return new HttpUnauthorizedResult();
            var query = _orchardServices.ContentManager.Query();
            var list = query.ForPart<GamePart>().List();
            var listranking = _orchardServices.ContentManager.Query().ForPart<RankingPart>().List();
            List<DisplaRankingTemplateVM> listaAllRank = new List<DisplaRankingTemplateVM>();
            foreach (GamePart gp in list) {

                ContentItem Ci = gp.ContentItem;
                string titolo = Ci.As<TitlePart>().Title;
                var listordered= listranking.Where(z => z.As<RankingPart>().ContentIdentifier == Ci.Id && z.As<RankingPart>().Device==TipoDispositivo.Apple ).OrderByDescending(y => y.Point);
                List<RankingTemplateVM> rkt = new List<RankingTemplateVM>();
                foreach (RankingPart cirkt in listordered) {
                    RankingTemplateVM tmp = new RankingTemplateVM();
                    tmp.Point = cirkt.Point;
                    tmp.ContentIdentifier = cirkt.ContentIdentifier;
                    tmp.Device = cirkt.Device;
                    tmp.Identifier = cirkt.Identifier;
                    tmp.name = getusername(cirkt.User_Id);
                    tmp.UsernameGameCenter = cirkt.UsernameGameCenter;
                    tmp.AccessSecured = cirkt.AccessSecured;
                    tmp.RegistrationDate = cirkt.RegistrationDate;
                    rkt.Add(tmp);
                }
                listaAllRank.Add(new DisplaRankingTemplateVM { Title = titolo + " Apple", ListRank = rkt });
               
                listordered = listranking.Where(z => z.As<RankingPart>().ContentIdentifier == Ci.Id && z.As<RankingPart>().Device == TipoDispositivo.Android).OrderByDescending(y => y.Point);
                rkt = new List<RankingTemplateVM>();
                foreach (RankingPart cirkt in listordered) {
                    RankingTemplateVM tmp = new RankingTemplateVM();
                    tmp.Point = cirkt.Point;
                    tmp.ContentIdentifier = cirkt.ContentIdentifier;
                    tmp.Device = cirkt.Device;
                    tmp.Identifier = cirkt.Identifier;
                    tmp.name = getusername(cirkt.User_Id);
                    tmp.UsernameGameCenter = cirkt.UsernameGameCenter;
                    tmp.AccessSecured = cirkt.AccessSecured;
                    tmp.RegistrationDate = cirkt.RegistrationDate;
                    rkt.Add(tmp);
                }
                listaAllRank.Add(new DisplaRankingTemplateVM { Title = titolo + " Android", ListRank = rkt });

                listordered = listranking.Where(z => z.As<RankingPart>().ContentIdentifier == Ci.Id && z.As<RankingPart>().Device == TipoDispositivo.WindowsMobile).OrderByDescending(y => y.Point);
                rkt = new List<RankingTemplateVM>();
                foreach (RankingPart cirkt in listordered) {
                    RankingTemplateVM tmp = new RankingTemplateVM();
                    tmp.Point = cirkt.Point;
                    tmp.ContentIdentifier = cirkt.ContentIdentifier;
                    tmp.Device = cirkt.Device;
                    tmp.Identifier = cirkt.Identifier;
                    tmp.name = getusername(cirkt.User_Id);
                    tmp.UsernameGameCenter = cirkt.UsernameGameCenter;
                    tmp.AccessSecured = cirkt.AccessSecured;
                    tmp.RegistrationDate = cirkt.RegistrationDate;
                    rkt.Add(tmp);
                }
                listaAllRank.Add(new DisplaRankingTemplateVM { Title = titolo+" Windows Mobile", ListRank = rkt });
            }


            return View((object)listaAllRank);











            //var AllRecord = _PushNotificationService.SearchPushNotification(search.Expression);
            //var totRecord = AllRecord.Count();
            //Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            //dynamic pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(totRecord);

            //// Generate a list of shapes, restricting by pager parameters
            //var list = _orchardServices.New.List();
            //list.AddRange(AllRecord.Skip(pager.GetStartIndex())
            //                    .Take(pager.PageSize)
            //    // .Select(r => _orchardService.ContentManager.BuildDisplay(r, "ciao"))
            //                    );
            ////   (object) new model {Orders: list, Pager: pagerShape, Admn: hasPermission};

            ////var model = Shape.Orders(Orders: list, Pager: pagerShape, Admn: hasPermission, OrderPayedCount: countOrdersNew, Search: search);
            //var model = new PushIndex(list, search, pagerShape);

            //return View((object)model);
            ////return View((object)new {
            ////    Orders = list,
            ////    Pager = pagerShape,
            ////    Admn = hasPermission
            ////});
        }
        //The GelistSingleGame methods get the rankings for a single name, identified by its ID, and for a single Device.
        //For any user (identified by its phone number) only one score is in the output of the method.
        //TODO: Currently, DB accesses and list manipulations are done separately, but they should be merged into a single query to decrease data transfer between app and DB
        [HttpGet]
        [Admin]
        public ActionResult GetListSingleGame(int ID, int? page, int? pageSize, string deviceType = "General", bool ascending = false) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.GameRanking)) //(Permissions.AccessStatistics)) //(StandardPermissions.SiteOwner)) //
                return new HttpUnauthorizedResult();
            return GetListSingleGame(ID, new PagerParameters {
                Page = page, PageSize = pageSize
            }, DeviceType: deviceType, Ascending: ascending);
        }
        [HttpPost]
        [Admin]
        public ActionResult GetListSingleGame(int ID, PagerParameters pagerParameters, string DeviceType = "General", bool Ascending = false) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.GameRanking)) //(Permissions.AccessStatistics)) //(StandardPermissions.SiteOwner)) //
                return new HttpUnauthorizedResult();

            if (pagerParameters.PageSize == null)
                pagerParameters.PageSize = _orchardServices.WorkContext.CurrentSite.PageSize;
            if (pagerParameters.Page == null)
                pagerParameters.Page = 1;
            

            var query = _orchardServices.ContentManager.Query();
            var list = query.ForPart<GamePart>().Where<GamePartRecord>(x => x.Id == ID).List(); //list all games with the selected ID (should be only one)
            GamePart gp = list.FirstOrDefault(); //the game for which we want the rankings
            //Assuming there was no issues, gp should never be null. If gp is null, it probably means something happened in the DB, since we
            //read the ID from the DB to create the "caller" page, and the we read again in this method.
            ContentItem Ci = gp.ContentItem;
            string titolo = Ci.As<TitlePart>().Title;

            string devString = "General";
            //query to get the ranking out of the db, already sorted and paged, with multiple scores by a same user removed
            string queryDeviceCondition = "";
            if (DeviceType == "Apple") {
                queryDeviceCondition += "AND Device = '" + TipoDispositivo.Apple + "' ";
                devString = "Apple";
            } else if (DeviceType == "Android") {
                queryDeviceCondition += "AND Device = '" + TipoDispositivo.Android + "' ";
                devString = "Android";
            } else if (DeviceType == "Windows Phone") {
                queryDeviceCondition += "AND Device = '" + TipoDispositivo.WindowsMobile + "' ";
                devString = "Windows Phone";
            }
            string subQueryPoints = "SELECT MAX(Point) "
                    + "FROM [Orchard_FestivalTV].[dbo].[Laser_Orchard_Questionnaires_RankingPartRecord] " //Laser.Orchard.Questionnaires.Models.RankingPartRecord "
                    + "WHERE ContentIdentifier=" + ID + " " + queryDeviceCondition
                    + "AND Identifier = rpr.Identifier ";
            string subQueryDate = "SELECT MIN(RegistrationDate) "
                    + "FROM [Orchard_FestivalTV].[dbo].[Laser_Orchard_Questionnaires_RankingPartRecord] " //Laser.Orchard.Questionnaires.Models.RankingPartRecord "
                    + "WHERE ContentIdentifier=" + ID + " " + queryDeviceCondition
                    + "GROUP BY Identifier, Point ";
            string queryTable = "SELECT rpr.[Point], rpr.[Identifier], rpr.[UsernameGameCenter], rpr.[Device], rpr.[ContentIdentifier], rpr.[User_Id], rpr.[AccessSecured], rpr.[RegistrationDate] "
                    + "FROM [Orchard_FestivalTV].[dbo].[Laser_Orchard_Questionnaires_RankingPartRecord] as rpr " //Laser.Orchard.Questionnaires.Models.RankingPartRecord as rpr "
                    + "WHERE ContentIdentifier=" + ID + " " + queryDeviceCondition
                    + "AND Point = ( " + subQueryPoints + " ) "
                    + "AND RegistrationDate IN ( " + subQueryDate + " ) ";
            if (Ascending)
                queryTable += "ORDER BY Point ";
            else
                queryTable += "ORDER BY Point DESC ";
            //paging
            queryTable += "OFFSET " + (pagerParameters.PageSize.Value * (pagerParameters.Page.Value - 1)).ToString() + " ROWS "
                    + "FETCH NEXT " + pagerParameters.PageSize.Value.ToString() + " ROWS ONLY ";
            var tableQuery = _sessionLocator.For(typeof(RankingPartRecord)).CreateSQLQuery(queryTable); //since we create a SQL query, we use [table names] instead of Domain.Names
            var ranking = tableQuery.List();

            List<RankingTemplateVM> lRank = new List<RankingTemplateVM>();
            foreach (Object[] obj in ranking) {
                RankingTemplateVM tmp = new RankingTemplateVM();
                tmp.Point = (Int32)obj[0];
                tmp.Identifier = (string)obj[1];
                tmp.UsernameGameCenter = (string)obj[2];
                if ((string)obj[3] == TipoDispositivo.Android.ToString())
                    tmp.Device = TipoDispositivo.Android;
                else if ((string)obj[3] == TipoDispositivo.Apple.ToString())
                    tmp.Device = TipoDispositivo.Apple;
                else if ((string)obj[3] == TipoDispositivo.WindowsMobile.ToString())
                    tmp.Device = TipoDispositivo.WindowsMobile;
                tmp.ContentIdentifier = (Int32)obj[4];
                tmp.name = getusername((Int32)obj[5]);
                tmp.AccessSecured = (bool)obj[6];
                tmp.RegistrationDate = (DateTime)obj[7];
                lRank.Add(tmp);
            }

            var session = _sessionLocator.For(typeof(RankingPartRecord));
            string queryString = "SELECT COUNT(DISTINCT Identifier) "
                + "FROM Laser.Orchard.Questionnaires.Models.RankingPartRecord as rpr "
                + "WHERE rpr.ContentIdentifier=" + ID + " ";

            if (DeviceType == "Apple") {
                queryString += "AND rpr.Device = '" + TipoDispositivo.Apple + "' ";
            } else if (DeviceType == "Android") {
                queryString += "AND rpr.Device = '" + TipoDispositivo.Android + "' ";
            } else if (DeviceType == "Windows Phone") {
                queryString += "AND rpr.Device = '" + TipoDispositivo.WindowsMobile + "' ";
            }
            var countQuery = session.CreateQuery(queryString);
            //var asd = countQuery.List(); // countQuery.UniqueResult();
            int scoresCount = (int)(countQuery.UniqueResult<long>());

            //create and initialize pager
            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(scoresCount);
            int listStart = pager.GetStartIndex();
            int listEnd = listStart + ((pager.PageSize > scoresCount) ? scoresCount : pager.PageSize);
            listEnd = listEnd > scoresCount ? scoresCount : listEnd;
            DisplaRankingTemplateVM pageOfScores = new DisplaRankingTemplateVM {
                Title = titolo,
                GameID = ID,
                Device = devString,
                ListRank = lRank //innerquery
            };

            var model = new DisplayRankingTemplateVMModel();
            model.Pager = pagerShape;
            model.drtvm = pageOfScores;

            return View((object)model); //((object)listaAllRank);
        }
        private string getusername(int id) {
            if (id > 0) {
                try {
                    return ((dynamic)_orchardServices.ContentManager.Get(id)).UserPart.UserName;
                }
                catch (Exception) {
                    return "No User";
                }
            }
            else
                return "No User";
        }

        //Adding functionality to list all games (published and unpublished)
        [HttpGet]
        [Admin]
        public ActionResult Index(int? page, int? pageSize, string searchExpression) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.GameRanking)) //(Permissions.AccessStatistics)) //
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, searchExpression);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, string searchExpression) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.GameRanking)) //(Permissions.AccessStatistics)) //
                return new HttpUnauthorizedResult();

            IContentQuery<ContentItem> contentQuery =
            //IContentQuery<GamePart> contentQuery =
                _orchardServices.ContentManager.Query()
                                               //.ForPart<GamePart>();
                                               .ForType("Game")
                                               .OrderByDescending<CommonPartRecord>(cpr => cpr.ModifiedUtc);

            if (!string.IsNullOrEmpty(searchExpression))
                contentQuery = contentQuery.Where<TitlePartRecord>(w => w.Title.Contains(searchExpression));

            Pager pager = new Pager(_orchardServices.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = _orchardServices.New.Pager(pager).TotalItemCount(contentQuery.Count());
            var pageOfContentItems = contentQuery.Slice(pager.GetStartIndex(), pager.PageSize);

            var model = new GamePartSearchViewModel();
            model.Pager = pagerShape;
            model.GameParts = pageOfContentItems;

            return View((object)model);
        }
    }
}