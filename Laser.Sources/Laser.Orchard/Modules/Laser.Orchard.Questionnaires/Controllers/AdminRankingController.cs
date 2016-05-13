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
        public AdminRankingController(IOrchardServices orchardServices, IQuestionnairesServices questionnairesServices, IRepository<RankingPartRecord> repoRanking) {
            _orchardServices = orchardServices;
            _questionnairesServices = questionnairesServices;
            _repoRanking = repoRanking;
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
            //query to get the ranking out of the db, already sorted, with multiple scores by a same user removed
            var tblQuery = _repoRanking.Table.Where(x => x.ContentIdentifier == ID);
            if (DeviceType == "Apple") {
                tblQuery = tblQuery.Where(z => z.Device == TipoDispositivo.Apple);
                devString = "Apple";
            } else if (DeviceType == "Android") {
                tblQuery = tblQuery.Where(z => z.Device == TipoDispositivo.Android);
                devString = "Android";
            } else if (DeviceType == "Windows Phone") {
                tblQuery = tblQuery.Where(z => z.Device == TipoDispositivo.WindowsMobile);
                devString = "Windows Phone";
            }
            //list of RankingTemplateVM
            List<RankingTemplateVM> innerquery;

            var groupquery = tblQuery.GroupBy(
                    //grouping by User_id and UsernameGameCenter is redundant, but allows us to easily access that information for the Select() later
                    f => new { f.Identifier, f.Device, f.User_Id, f.UsernameGameCenter })
                    .Select(result => new RankingTemplateVM {
                        Point = result.Max(s => s.Point),
                        ContentIdentifier = ID,
                        Device = result.Key.Device,
                        Identifier = result.Key.Identifier,
                        name = result.Key.User_Id.ToString(), //the User_Id is a number uniquely identifying the user. Later we will get the name from it
                        UsernameGameCenter = result.Key.UsernameGameCenter,//(result.Where(r => r.Identifier == result.Key.Identifier)).First().UsernameGameCenter,
                        //we would like to take the date when the user first obtained his high score:
                        //the following query actually returns the date of the oldest score by the user
                        //result.Where<RankingPartRecord>(s => s.Point == result.Max(e => e.Point)).Min(t => t.RegistrationDate)
                        RegistrationDate = result.Where<RankingPartRecord>(s => s.Point == result.Max(e => e.Point)).Min(t => t.RegistrationDate) //result.Max(s => s.RegistrationDate) //
                    });
                    
            //The only difference between the two conditions is the OrderBy used. However if I split the query I get compilation errors
            if (Ascending)
                innerquery = groupquery
                    .OrderBy(o => o.Point)
                    ////Next two lines implement the paging
                    .Skip(pagerParameters.PageSize.Value * (pagerParameters.Page.Value - 1))
                    .Take(pagerParameters.PageSize.Value)
                    .ToList();
            else
                innerquery = groupquery
                   .OrderByDescending(o => o.Point)
                   // //Next two lines implement the paging
                   .Skip(pagerParameters.PageSize.Value * (pagerParameters.Page.Value - 1))
                   .Take(pagerParameters.PageSize.Value)
                   .ToList();

            //If we do not do the ToList(), Count == 1 for some reason. THis probably has to do with Count() working on Ienumerables,
            //while groupquery is an IQueriable
            int scoresCount = groupquery.ToList().Count();

            //Get the actual username from the User_Id
            foreach (RankingTemplateVM rtvm in innerquery) {
                rtvm.name = getusername(Int32.Parse(rtvm.name));
            }

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
                ListRank = innerquery
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