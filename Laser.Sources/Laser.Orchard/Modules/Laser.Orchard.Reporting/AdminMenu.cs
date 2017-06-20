using Laser.Orchard.Reporting.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.Projections;
using Orchard.UI.Navigation;
using Orchard.Mvc.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Laser.Orchard.Reporting
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IContentManager _contentManager;
        public AdminMenu(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("Data Reporting").Add(T("Data Reports"), "3",
                menu =>
                    menu.Add(T("Data Reports"), "1.0",
                    q => q.Action("Index", "Report", new { area = "Laser.Orchard.Reporting" }).Permission(Permissions.ManageQueries).LocalNav()), null);

            // report list
            var reportViewers = _contentManager.Query<DataReportViewerPart>().List();
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            builder.Add(item => {
                item.Caption(T("Admin Data Reports"))
                    .Permission(Security.Permissions.ShowAdminReports)
                    .Position("3.05")
                    .LinkToFirstChild(false);
                foreach(var report in reportViewers) {
                    item.Add(sub => sub.Caption(T(report.ContentItem.As<TitlePart>().Title ?? "[No title]"))
                    .Url(urlHelper.ItemDisplayUrl(report))
                    );
                }
            });
        }
    }
}