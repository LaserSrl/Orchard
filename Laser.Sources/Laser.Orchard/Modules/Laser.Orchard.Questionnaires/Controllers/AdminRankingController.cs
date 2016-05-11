using Laser.Orchard.Questionnaires.Models;
using Laser.Orchard.Questionnaires.Services;
using Laser.Orchard.Questionnaires.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
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
        public AdminRankingController(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
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
        //This method gets the rankings for a single name, identified by its ID, and for a single Device.
        //For any user (identified by its phone number)  only one score is in the output of this method
        [Admin]
        public ActionResult GetListSingleGame(int ID, string deviceType = "General", bool ascending = false) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.SiteOwner))
                return new HttpUnauthorizedResult();
            var query = _orchardServices.ContentManager.Query();
            var list = query.ForPart<GamePart>().List(); //list all games
            var listranking = _orchardServices.ContentManager.Query().ForPart<RankingPart>().List();
            List<DisplaRankingTemplateVM> listaAllRank = new List<DisplaRankingTemplateVM>(); //list to pass data to cshtml
            GamePart gp = list.Where(x => x.Id == ID).FirstOrDefault(); //the game for which we want the rankings
            ContentItem Ci = gp.ContentItem;
            string titolo = Ci.As<TitlePart>().Title;

            if (deviceType == "Apple") {
                var listordered = listranking.Where(z =>
                z.As<RankingPart>().ContentIdentifier == Ci.Id && z.As<RankingPart>().Device == TipoDispositivo.Apple)
                .OrderByDescending(y => y.Point);
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
                listaAllRank.Add(new DisplaRankingTemplateVM { Title = titolo, Device = "Apple", GameID = ID, ListRank = rkt });
            } else if (deviceType == "Android") {
                var listordered = listranking.Where(z => 
                    z.As<RankingPart>().ContentIdentifier == Ci.Id && z.As<RankingPart>().Device == TipoDispositivo.Android)
                    .OrderByDescending(y => y.Point);
                var rkt = new List<RankingTemplateVM>();
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
                listaAllRank.Add(new DisplaRankingTemplateVM { Title = titolo, Device = "Android", GameID = ID, ListRank = rkt });
            } else if (deviceType == "Windows Phone") {
                var listordered = listranking.Where(z =>
                    z.As<RankingPart>().ContentIdentifier == Ci.Id && z.As<RankingPart>().Device == TipoDispositivo.WindowsMobile)
                    .OrderByDescending(y => y.Point);
                var rkt = new List<RankingTemplateVM>();
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
                listaAllRank.Add(new DisplaRankingTemplateVM { Title = titolo, Device = "Windows Phone", GameID = ID, ListRank = rkt });
            } else {
                var listordered = listranking.Where(z =>
                    z.As<RankingPart>().ContentIdentifier == Ci.Id)
                    .OrderByDescending(y => y.Point);
                var rkt = new List<RankingTemplateVM>();
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
                listaAllRank.Add(new DisplaRankingTemplateVM { Title = titolo, Device = "General", GameID = ID, ListRank = rkt });
            }

            List<DisplaRankingTemplateVM> distinct = new List<DisplaRankingTemplateVM>();
            foreach (DisplaRankingTemplateVM drtvm in listaAllRank) {
                distinct.Add(new DisplaRankingTemplateVM {
                    Title = titolo, Device = drtvm.Device, GameID = ID,
                    ListRank = drtvm.ListRank //this is already sorted by score
                                    .GroupBy(x => x.Identifier) //group elements by phone number
                                    .Select(g => g.First()) //consider first element of each group
                                    .ToList()
                });
            }
            if (ascending)
                distinct.Reverse();

            return View((object)distinct); //((object)listaAllRank);
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
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessStatistics))
                return new HttpUnauthorizedResult();
            return Index(new PagerParameters {
                Page = page,
                PageSize = pageSize
            }, searchExpression);
        }

        [HttpPost]
        [Admin]
        public ActionResult Index(PagerParameters pagerParameters, string searchExpression) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.AccessStatistics))
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